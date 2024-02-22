using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Nyanbyte.PPCheck.Lib;
using Nyanbyte.PPCheck.Lib.Models;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Starting...");
        string file = args[0];

        var wb = new XLWorkbook(file);
        var ws = wb.Worksheets.First();

        Dictionary<string, IXLCell> dic = new();
        Dictionary<string, (SearchResponse, DateTimeOffset)> results = new();

        {
            var cell = ws.Cell("B2");
            while (!cell.IsEmpty())
            {
                string name = cell.Value.GetText().Trim();
                if (!cell.CellRight().CellRight().IsEmpty())
                {
                    Console.WriteLine($"Skipping {name}...");
                    cell = cell.CellBelow();
                    continue;
                }

                dic[name] = cell;

                cell = cell.CellBelow();
            }
        }

        Console.WriteLine($"Consolidated {dic.Count} records!\n");

        var api = new ApiClient();

        foreach (var record in dic)
        {
            string name = record.Key;
            Console.Write($"Processing: {name}... ");

            string[] names = name.Split(' ');
            if (names.Length != 2)
            {
                Console.WriteLine($"INVALID!\nCannot process '{name}' due to an irregular name!");
                continue;
            }

            SearchRequest req = new()
            {
                FirstName = names[1],
                LastName = names[0]
            };

            try
            {
                var result = await api.Search(req);

                if (result.HasError)
                {
                    Console.WriteLine($"ERR!\nCannot process '{name}' due to an unknown error!");
                    continue;
                }

                results[name] = (result, DateTimeOffset.Now);

                if (result.Data.Any())
                {
                    Console.WriteLine($"ATTENTION!\n\x1b[1;91mRecord exists for '\x1b[1;93m{name}\x1b[1;91m'!!!\x1b[0m");
                }
                else
                {
                    Console.WriteLine("OK");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Aborting searches due to an exception...");
                break;
            }

            int rand = Random.Shared.Next(200);
            await Task.Delay(rand + 1000);
        }

        Console.WriteLine("\nSaving results...");

        if (File.Exists("pedooutput.csv"))
            File.Delete("pedooutput.csv");

        if (File.Exists("pedoerrors.csv"))
            File.Delete("pedoerrors.csv");

        using var fs = new StreamWriter(File.OpenWrite("pedooutput.csv"));
        using var errfs = new StreamWriter(File.OpenWrite("pedoerrors.csv"));
        await fs.WriteLineAsync("Name,Date,LikelyRecords");
        await errfs.WriteLineAsync("Name");

        foreach (var rec in dic)
        {
            var cell = rec.Value.CellRight().CellRight();
            string name = rec.Key;

            if (!results.ContainsKey(name))
            {
                cell.CreateRichText().AddText("błąd sprawdzania").SetFontColor(XLColor.DarkRed);
                await errfs.WriteLineAsync($"{name}");
                continue;
            }

            var (result, date) = results[name];

            string strRec = result.Total == 0 ? "brak rekordów" : (result.Total == 1 ? "1 rekord" : $"{result.Total} rekordy");
            cell.CreateRichText().AddText($"{date:dd.MM.yyyy, HH:mm:ss} - {strRec}").SetFontColor(result.Total == 0 ? XLColor.DarkGreen : XLColor.Red);

            await fs.WriteLineAsync($"{name},{date.ToUnixTimeMilliseconds()},{result.Total}");
        }

        wb.Save();

        if (!Directory.Exists("pedofiles"))
            Directory.CreateDirectory("pedofiles");

        foreach (var records in results.Where(f => f.Value.Item1.Total > 0))
        {
            string sub = $"pedofiles/{records.Key}";
            Directory.CreateDirectory(sub);

            foreach (var data in records.Value.Item1.Data)
            {
                try
                {
                    var img = await api.GetImage(data.PersonIdentityId);
                    if (img != null)
                    {
                        await File.WriteAllBytesAsync($"{sub}/{data.PersonIdentityId}.png", img);
                    }
                }
                catch (Exception)
                {
                }

                using var write = new StreamWriter(File.OpenWrite($"{sub}/{data.PersonIdentityId}.txt"));

                await write.WriteLineAsync($"Identyfikator: {data.PersonIdentityId}");
                await write.WriteLineAsync($"Ilość adnotacji: {data.AnnotationCount}\n");
                await write.WriteLineAsync("Znane tożsamości:\n");
                foreach (var persona in data.Personas)
                {
                    await write.WriteLineAsync($"Id: {persona.Id}");
                    await write.WriteLineAsync($"Imię: {persona.FirstName} {(persona.SecondName != null ? $"{persona.SecondName} " : "")}{persona.LastName}");
                    await write.WriteLineAsync($"Date urodzenia: {persona.DateOfBirth.ToShortDateString()}");
                    await write.WriteLineAsync($"Miejsce urodzenia: {persona.CityOfBirth}");
                    await write.WriteLineAsync($"Płeć: {persona.OffenderSex}");
                    await write.WriteLineAsync($"Nazwisko rodowe: {persona.FamilyName}");
                    await write.WriteLineAsync($"Państwo urodzenia: {persona.CountryOfBirth}");
                    await write.WriteLineAsync($"Obywatelstwo: {persona.Nationalities}");
                    await write.WriteLineAsync($"Miejscowość przebywania: {persona.DwellingPlace}\n");
                }
            }

            await Task.Delay(100);
        }

        Console.WriteLine("Done!");
    }
}
