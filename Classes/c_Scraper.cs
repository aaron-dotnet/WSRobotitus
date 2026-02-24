using System.Net;

public class c_Scraper : IDisposable
{
    private HttpClient _client;
    private HttpClientHandler _handler;
    private CookieContainer _cookieContainer = new();
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
    }

    private void Headers()
    {
        string version = "146.0";
        string userAgent = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:{version}) Gecko/20100101 Firefox/{version}";
        _client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        _client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        _client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        if (!string.IsNullOrEmpty(Origin))
        {
            _client.DefaultRequestHeaders.Add("Origin", Origin);
        }
        if (!string.IsNullOrEmpty(Host))
        {
            _client.DefaultRequestHeaders.Host = Host;
        }
        if (!string.IsNullOrEmpty(Referer))
        {
            _client.DefaultRequestHeaders.Referrer = new Uri(Referer);
        }
    }

    public async Task<string> Get(string url)
    {
        Headers();
        using (HttpResponseMessage response = await _client.GetAsync(url))
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        return string.Empty;
    }
    public async Task<string> Post(string url, HttpContent content)
    {
        Headers();
        using (HttpResponseMessage response = await _client.PostAsync(url, content))
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