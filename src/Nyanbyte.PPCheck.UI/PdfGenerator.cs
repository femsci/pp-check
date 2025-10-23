using System.Collections.Immutable;
using System.Globalization;
using Nyanbyte.PPCheck.Lib.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Nyanbyte.PPCheck.UI;

public static class PdfGenerator
{
    public static Document GenerateReport(ICollection<(string, string)> names,
        IDictionary<(string, string), HashSet<OffenderInformation>> offenderInfo)
    {
        var namesChk = names.Select(x => (x.Item1, x.Item2, offenderInfo.ContainsKey(x))).ToImmutableHashSet();
        return Document
            .Create(document =>
            {
                document.Page(p =>
                {
                    p.Size(PageSizes.A4);
                    p.Margin(1, Unit.Centimetre);

                    p.Header().PaddingBottom(1).Column(c =>
                    {
                        c.Spacing(5);
                        c.Item().Text("Raport").FontSize(18).Bold().AlignCenter();
                        c.Item().Text(t =>
                        {
                            t.AlignCenter();
                            t.Line(
                                "weryfikacji danych pracowników wobec\nRejestru Sprawców Przestępstw na Tle Seksualnym");
                            t.Line($"z dnia {DateTime.Today:dd.MM.yyyy} r.");
                        });
                    });
                    p.Footer().PaddingTop(10).AlignCenter().Text(t =>
                    {
                        t.Span("strona ");
                        t.CurrentPageNumber();
                        t.Span("/");
                        t.TotalPages();
                    });
                    p.Content().Column(pc =>
                    {
                        pc.Spacing(5);
                        pc.Item().Text(t =>
                        {
                            t.AlignCenter();
                            t.Span("znaleziono ");
                            t.Span(offenderInfo.Count.ToString()).Bold();
                            t.Span($" podejrze{offenderInfo.Count switch
                            {
                                1 => "nie",
                                > 1 and < 5 => "nia",
                                _ => "ń"
                            }}");
                        });
                        pc.Item().MultiColumn(c =>
                        {
                            c.BalanceHeight();
                            c.Columns();
                            c.Content().AlignCenter().Column(co =>
                            {
                                co.Spacing(1);
                                foreach (var row in namesChk.OrderBy(x => x.Item2, StringComparer.Create(new CultureInfo("pl-PL", false), true)))
                                {
                                    co.Item().Text(t =>
                                    {
                                        t.Span($"{row.Item2} {row.Item1}");
                                        t.Span(" — ");
                                        if (row.Item3)
                                        {
                                            t.Span("PODEJRZENIE").FontColor(Color.FromRGB(200, 0, 0)).Bold();
                                        }
                                        else
                                        {
                                            t.Span("NIE FIGURUJE").FontColor(Color.FromRGB(0, 150, 0));
                                        }
                                    });
                                }
                            });
                        });
                    });
                });

                foreach (var (ns, info) in offenderInfo)
                {
                    document.Page(p =>
                    {
                        p.Size(PageSizes.A4);
                        p.Margin(1, Unit.Centimetre);
                        p.Header().PaddingBottom(20).Column(c =>
                        {
                            c.Spacing(3);
                            c.Item().Text("Profile podejrzanego").Bold().FontSize(16).AlignCenter();
                            c.Item().Text($"{ns.Item1} {ns.Item2}").AlignCenter();
                        });
                        p.Footer().PaddingTop(10).AlignCenter().Text(t =>
                        {
                            t.Span("strona ");
                            t.CurrentPageNumber();
                            t.Span("/");
                            t.TotalPages();
                        });
                        p.Content().Column(col =>
                        {
                            col.Spacing(15);
                            foreach (var offender in info)
                            {
                                var persona = offender.Personas.First();

                                col.Item().Row(r =>
                                {
                                    if (persona.Picture != null)
                                    {
                                        File.WriteAllBytes("/tmp/pic", persona.Picture);
                                        r.AutoItem().MaxWidth(3, Unit.Centimetre).Image(persona.Picture!).FitWidth();
                                        r.RelativeItem().Table(t =>
                                        {
                                            t.ColumnsDefinition(c =>
                                            {
                                                c.RelativeColumn();
                                                c.RelativeColumn(2);
                                            });

                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text("Id").FontSize(12);
                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text(persona.PersonIdentityId.ToString());

                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text("Imię");
                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text(persona.FirstName);

                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text("Nazwisko");
                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text(persona.LastName);

                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text("Data urodzenia");
                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text(persona.DateOfBirth.ToString("dd.MM.yyyy"));

                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text("Miejsce urodzenia");
                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text(persona.CityOfBirth);

                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text("Państwo urodzenia");
                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text(persona.CountryOfBirth);

                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text("Obywatelstwo/a");
                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text(persona.Nationalities);

                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text("Płeć");
                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text(persona.Sex);

                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text("Miejscowość przebywania");
                                            t.Cell().BorderHorizontal(0.4f).BorderColor(Color.FromRGB(221, 221, 221))
                                                .Padding(3).Text(persona.DwellingPlace);
                                        });
                                    }
                                });
                            }
                        });
                    });
                }
            }).WithMetadata(new DocumentMetadata()
            {
                Title = "Raport",
                Subject = "Raport weryfikacji danych pracowników wobec Rejestru Sprawców Przestępstw na Tle Seksualnym",
                Creator = "ppcheck",
                Language = "pl-PL",
                CreationDate = DateTime.Now,
                ModifiedDate = DateTimeOffset.Now
            });
    }
}
