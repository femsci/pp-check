namespace Nyanbyte.PPCheck.Lib;

public class ApiClient : IDisposable
{
    internal const string BaseAddr = "https://rps.ms.gov.pl/pl-PL/api/";

    private readonly HttpClient _http;
    private readonly bool _isHttpExternal;

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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (!_isHttpExternal)
        {
            _http.Dispose();
        }
    }
}
