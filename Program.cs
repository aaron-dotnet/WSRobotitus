using static c_Functions;

internal class Program
{
    private static Settings _config = null!;
    private static c_Scraper _scraper = null!;

    private static readonly Dictionary<string, Func<string, List<c_Link>>> FooterExtractors = new()
    {
        ["SECCIONES"] = HtmlExtractors.ExtractCategories,
        ["REDES SOCIALES"] = HtmlExtractors.ExtractSocialNetworks,
        ["CANALES RELACIONADOS"] = html => HtmlExtractors.ExtractRelatedChannels(html, ["El Robot de Platón", "El Robot de Colón"]),
        ["APLICACIONES"] = HtmlExtractors.ExtractApps,
        ["SITIO"] = HtmlExtractors.ExtractSiteLinks
    };

    private static async Task Main(string[] args)
    {
        // Cargar configuración
        _config = AppConfig.Load("appsettings.json");

        string category = _config.Scraper.DefaultCategory;
        if (args.Length > 0)
        {
            category = args[0].ToLower();
        }

        string url = $"https://{_config.Scraper.BaseUrl}/category/{category}";
        Console.WriteLine($"[INFO] Categoría: {category}");
        Console.WriteLine($"[INFO] URL: {url}");

        using (_scraper = new c_Scraper())
        {
            string content = await FetchContent(url, referer: _config.Scraper.BaseReferer, host: _config.Scraper.BaseUrl);

            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("[ERROR] No se pudo obtener el contenido");
                return;
            }

            ExtractFooterInfo(content);

            await ScrapePagination(content, url);
        }

        Console.WriteLine("\n=== PROCESO COMPLETADO ===");
    }

    private static async Task<string> FetchContent(string url, string? referer = null, string? host = null)
    {
        return await _scraper.Get(url, referer ?? _config.Scraper.BaseReferer, host);
    }

    private static void ExtractFooterInfo(string content)
    {
        Console.WriteLine("\n=== EXTRACCIÓN DEL FOOTER ===");

        string footerPart = GetFooter(content);

        foreach (var (title, extractor) in FooterExtractors)
        {
            List<c_Link> allLinks = extractor(footerPart);

            Console.WriteLine($"\n[{title}] ({allLinks.Count} encontrados):");

            foreach (c_Link link in allLinks)
            {
                Console.WriteLine($"  - {link.Name}: {link.Url}");
            }
        }

        string? email = HtmlExtractors.ExtractContactEmail(footerPart);
        if (!string.IsNullOrEmpty(email))
        {
            Console.WriteLine($"\n[CONTACTO]:");
            Console.WriteLine($"  - Email: {email}");
        }
    }

    private static async Task ScrapePagination(string content, string baseUrl)
    {
        Console.WriteLine("\n=== PAGINACIÓN ===");

        int totalPages = HtmlExtractors.ExtractTotalPages(content);
        Console.WriteLine($"[INFO] Total de páginas detectadas: {totalPages}");

        int pagesToProcess = Math.Min(_config.Scraper.PagesToScrape, totalPages);
        Console.WriteLine($"[INFO] Se procesarán las próximas {pagesToProcess} páginas\n");

        string mainContent = GetMainContent(content);
        ParseContent(mainContent);

        if (pagesToProcess > 1)
        {
            // Limitado a tres concurrencias
            using var semaphore = new SemaphoreSlim(3);
            List<Task> tasks = [];

            for (int i = 2; i <= pagesToProcess; i++)
            {
                int pageNum = i;
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        string pageUrl = $"{baseUrl}/page/{pageNum}";
                        string referer = pageNum > 2 ? $"{baseUrl}/page/{pageNum - 1}" : baseUrl;
                        string pageContent = await FetchContent(pageUrl, referer, _config.Scraper.BaseUrl);

                        if (!string.IsNullOrEmpty(pageContent))
                        {
                            string pageMain = GetMainContent(pageContent);
                            ParseContent(pageMain);
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }

        Console.WriteLine($"\n[TOTAL] Páginas procesadas: {pagesToProcess}");
    }

    private static string GetMainContent(string content)
    {
        string mainContent = GetString(content, "<main", "</main>");
        string navPart = GetString(mainContent, "<nav", "</nav>");
        string cleanContent = mainContent.Replace(navPart, "").Replace("&nbsp;", "").Replace("&hellip;", "");

        return cleanContent;
    }

    private static string GetFooter(string content)
    {
        string footerPart = GetString(content, "<footer", "</footer>", firstCoincidence: true);
        return HtmlExtractors.RemoveHtmlComments(footerPart);
    }

    private static void ParseContent(string content)
    {
        c_NewsParser parser = new();
        List<c_NewsItem> news = parser.Parse(content);

        foreach (c_NewsItem thisnew in news)
        {
            Console.WriteLine($"{thisnew.Date:dd/MM/yyyy} | {thisnew.Author} - {thisnew.Title}");
        }

        Console.WriteLine($"  -> {news.Count} artículos extraídos");
    }
}
