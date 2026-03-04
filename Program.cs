using static c_Functions;

internal class Program
{
    private static Settings _config = null!;

    private static readonly Dictionary<string, Func<string, List<c_Link>>> FooterExtractors = new()
    {
        ["SECCIONES"] = html => HtmlExtractors.ExtractCategories(html),
        ["REDES SOCIALES"] = html => HtmlExtractors.ExtractSocialNetworks(html),
        ["CANALES RELACIONADOS"] = html => HtmlExtractors.ExtractRelatedChannels(html, ["El Robot de Platón", "El Robot de Colón"]),
        ["APLICACIONES"] = html => HtmlExtractors.ExtractApps(html),
        ["SITIO"] = html => HtmlExtractors.ExtractSiteLinks(html)
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

        string content = await FetchContent(url);

        if (string.IsNullOrEmpty(content))
        {
            Console.WriteLine("[ERROR] No se pudo obtener el contenido");
            return;
        }

        ExtractFooterInfo(content);

        await ScrapePagination(content, url);

        Console.WriteLine("\n=== PROCESO COMPLETADO ===");
    }

    private static async Task<string> FetchContent(string url, string? referer = null)
    {
        Console.WriteLine("[INFO] Descargando contenido desde la web...");

        using c_Scraper scraper = new();
        scraper.Host = _config.Scraper.BaseUrl;
        scraper.Referer = referer ?? _config.Scraper.BaseReferer;
        return await scraper.Get(url);
    }

    private static void ExtractFooterInfo(string content)
    {
        Console.WriteLine("\n=== EXTRACCIÓN DEL FOOTER ===");

        string footerPart = GetFooter(content);

        foreach (var (title, extractor) in FooterExtractors)
        {
            var items = extractor(footerPart);
            Console.WriteLine($"\n[{title}] ({items.Count} encontrados):");
            foreach (var link in items)
                Console.WriteLine($"  - {link.Name}: {link.Url}");
        }

        var email = HtmlExtractors.ExtractContactEmail(footerPart);
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

        var pageLinks = HtmlExtractors.ExtractPageLinks(content, baseUrl);
        Console.WriteLine($"[INFO] Links de páginas generados: {pageLinks.Count}");

        int pagesToProcess = Math.Min(_config.Scraper.PagesToScrape, totalPages);
        Console.WriteLine($"[INFO] Se procesarán las próximas {pagesToProcess} páginas\n");

        string mainContent = GetString(content, "<main", "</main>");
        string navPart = GetString(mainContent, "<nav", "</nav>");
        string cleanContent = mainContent.Replace(navPart, "").Replace("&nbsp;", "").Replace("&hellip;", "");

        for (int i = 1; i <= pagesToProcess; i++)
        {
            Console.WriteLine($"--- Página {i}/{pagesToProcess} ---");

            if (i == 1)
            {
                ParseContent(cleanContent);
            }
            else
            {
                string pageUrl = $"{baseUrl}/page/{i}";
                string referer = i > 2 ? $"{baseUrl}/page/{i - 1}" : baseUrl;
                string pageContent = await FetchContent(pageUrl, referer);

                if (!string.IsNullOrEmpty(pageContent))
                {
                    string pageMain = GetString(pageContent, "<main", "</main>");
                    string pageNav = GetString(pageMain, "<nav", "</nav>");
                    string pageClean = pageMain.Replace(pageNav, "").Replace("&nbsp;", "").Replace("&hellip;", "");
                    ParseContent(pageClean);
                }
            }
        }

        Console.WriteLine($"\n[TOTAL] Páginas procesadas: {pagesToProcess}");
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

        Console.WriteLine($"  -> {news.Count} artículos extraídos");
    }
}
