using System.Data;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nyanbyte.PPCheck.Lib.Models;

namespace Nyanbyte.PPCheck.Lib;

public class ApiClient : IDisposable
{
    private const string BaseAddr = "https://rps.ms.gov.pl/pl-PL/api/";

    private readonly HttpClient _http;
    private readonly bool _isHttpExternal;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        AllowTrailingCommas = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IgnoreReadOnlyProperties = false,
        IncludeFields = false,
        NumberHandling = JsonNumberHandling.Strict,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        WriteIndented = false
    };

    public ApiClient(HttpClient http)
    {
        _http = http;
        _isHttpExternal = true;
    }

    public ApiClient()
    {
        _http ??= new HttpClient
        {
            BaseAddress = new Uri(BaseAddr)
        };

        _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:143.0) Gecko/20100101 Firefox/143.0");
        _http.DefaultRequestHeaders.Add("Referer", "https://rps.ms.gov.pl/pl-PL/Public");
        _http.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
        _http.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
    }

    public async Task<SearchResponse> Search(SearchRequest req, bool loadPictures = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "persone/search");

        request.Content = new StringContent(JsonSerializer.Serialize(req, JsonOpts), null, "application/json");

        var httpResponse = await _http.SendAsync(request);
        httpResponse.EnsureSuccessStatusCode();

        var resp = await httpResponse.Content.ReadFromJsonAsync<SearchResponse>() ?? throw new DataException("Invalid data returned.");

        if (!loadPictures || !resp.Data.Any()) return resp;
        foreach (var info in resp.Data)
        {
            var picture = await GetImage(info);
            foreach (var persona in info.Personas)
            {
                persona.Picture = picture;
            }
        }

        return resp;
    }

    public async Task<byte[]?> GetImage(Guid identityId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"Persone/ImageGet/{identityId}");

        var httpResponse = await _http.SendAsync(request);
        httpResponse.EnsureSuccessStatusCode();

        string resp = (await httpResponse.Content.ReadAsStringAsync()).Trim('"');

        if (string.IsNullOrWhiteSpace(resp) || resp.Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        byte[] img = Convert.FromBase64String(resp);
        return img;
    }

    public async Task<byte[]?> GetImage(OffenderInformation i) => await GetImage(i.PersonIdentityId);


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (!_isHttpExternal)
        {
            _http.Dispose();
        }
    }
}
