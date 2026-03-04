using System.Net;

public class c_Scraper : IDisposable
{
    private readonly HttpClient _client;
    private readonly HttpClientHandler _handler;
    private readonly CookieContainer _cookieContainer = new();
    public string Host { get; set; } = string.Empty;
    public string Referer { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;

    public c_Scraper()
    {
        _handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.All,
            CookieContainer = _cookieContainer,
            UseCookies = true
        };
        _client = new HttpClient(_handler);
        SetHeaders();
    }

    private void SetHeaders()
    {
        const string VERSION = "146.0";
        _client.DefaultRequestHeaders.UserAgent.ParseAdd($"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:{VERSION}) " +
                                                         $"Gecko/20100101 Firefox/{VERSION}");
        _client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
        _client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br, zstd");
        _client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");

        if (!string.IsNullOrWhiteSpace(Origin))
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", Origin);

        if (!string.IsNullOrWhiteSpace(Host))
            _client.DefaultRequestHeaders.Host = Host;

        if (Uri.TryCreate(Referer, UriKind.Absolute, out var u))
            _client.DefaultRequestHeaders.Referrer = u;
    }

    public async Task<string> Get(string url)
    {
        using (HttpResponseMessage response = await _client.GetAsync(url))
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        return string.Empty;
    }
    public void Dispose()
    {
        _client?.Dispose();
        _handler?.Dispose();
        GC.SuppressFinalize(this);
    }
}