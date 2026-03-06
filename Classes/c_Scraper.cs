using System.Net;

public class c_Scraper : IDisposable
{
    private readonly HttpClient _client;
    private readonly SocketsHttpHandler _handler;
    private readonly CookieContainer _cookieContainer = new();

    public string Host { get; set; } = string.Empty;
    public string Referer { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;

    private const int MaxRetries = 3;
    private const int TimeoutSeconds = 30;

    public c_Scraper()
    {
        _handler = new SocketsHttpHandler()
        {
            AutomaticDecompression = DecompressionMethods.All,
            CookieContainer = _cookieContainer,
            UseCookies = true,
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        };

        _client = new HttpClient(_handler)
        {
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds)
        };

        SetHeaders();
    }

    private void SetHeaders()
    {
        const string VERSION = "146.0";
        var headers = _client.DefaultRequestHeaders;
        headers.UserAgent.ParseAdd($"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:{VERSION}) " +
                                   $"Gecko/20100101 Firefox/{VERSION}");
        headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        headers.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
        headers.AcceptEncoding.ParseAdd("gzip, deflate, br, zstd");
        headers.Connection.ParseAdd("keep-alive");

        if (!string.IsNullOrWhiteSpace(Origin))
            headers.TryAddWithoutValidation("Origin", Origin);

        if (!string.IsNullOrWhiteSpace(Host))
            headers.Host = Host;

        if (Uri.TryCreate(Referer, UriKind.Absolute, out var url))
            headers.Referrer = url;
    }

    public async Task<string> Get(string url, string? referer = null, string? host = null)
    {
        if (!string.IsNullOrEmpty(host))
            _client.DefaultRequestHeaders.Host = host;

        if (Uri.TryCreate(referer ?? Referer, UriKind.Absolute, out var refererUri))
            _client.DefaultRequestHeaders.Referrer = refererUri;

        int attempt = 0;

        while (attempt < MaxRetries)
        {
            try
            {
                using HttpResponseMessage response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                Console.WriteLine($"[WARN] Request failed ({response.StatusCode}), attempt {attempt + 1}/{MaxRetries}");
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"[WARN] Request timeout, attempt {attempt + 1}/{MaxRetries}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[WARN] Request error: {ex.Message}, attempt {attempt + 1}/{MaxRetries}");
            }

            attempt++;

            if (attempt < MaxRetries)
            {
                int delay = (int)Math.Pow(2, attempt) * 500;
                await Task.Delay(delay);
            }
        }

        Console.WriteLine($"[ERROR] Failed after {MaxRetries} attempts: {url}");
        return string.Empty;
    }

    public void Dispose()
    {
        _client?.Dispose();
        _handler?.Dispose();
        GC.SuppressFinalize(this);
    }
}
