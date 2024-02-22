using System.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nyanbyte.PPCheck.Lib.Models;

namespace Nyanbyte.PPCheck.Lib;

public class ApiClient : IDisposable
{
    internal const string BaseAddr = "https://rps.ms.gov.pl/pl-PL/api/";

    private readonly HttpClient _http;
    private readonly bool _isHttpExternal;

    private static readonly JsonSerializerOptions _jsonOpts = new()
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
        _http = new HttpClient
        {
            BaseAddress = new Uri(BaseAddr)
        };
    }

    public async Task<SearchResponse> Search(SearchRequest req)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "persone/search");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:122.0) Gecko/20100101 Firefox/122.0");
        request.Headers.Add("Referer", "https://rps.ms.gov.pl/pl-PL/Public");
        request.Headers.Add("Accept", "application/json, text/plain, */*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");

        request.Content = new StringContent(JsonSerializer.Serialize(req, _jsonOpts), null, "application/json");

        var httpResponse = await _http.SendAsync(request);
        httpResponse.EnsureSuccessStatusCode();

        var resp = await httpResponse.Content.ReadFromJsonAsync<SearchResponse>() ?? throw new DataException("Invalid data returned.");

        return resp;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (!_isHttpExternal)
        {
            _http.Dispose();
        }
    }
}
