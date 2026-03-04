using static c_Functions;

internal class Program
{
    private static Settings _config = null!;

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

        var categories = c_Helper.ExtractCategories(footerPart);
        Console.WriteLine($"\n[SECCIONES] ({categories.Count} encontradas):");
        foreach (c_Link link in categories)
        {
            Console.WriteLine($"  - {link.Name}: {link.Url}");
        }

        var social = c_Helper.ExtractSocialNetworks(footerPart);
        Console.WriteLine($"\n[REDES SOCIALES] ({social.Count} encontradas):");
        foreach (c_Link link in social)
        {
            Console.WriteLine($"  - {link.Name}: {link.Url}");
        }

        string[] relatedKeywords = { "El Robot de Platón", "El Robot de Colón" };
        var channels = c_Helper.ExtractRelatedChannels(footerPart, relatedKeywords);
        Console.WriteLine($"\n[CANALES RELACIONADOS] ({channels.Count} encontrados):");
        foreach (c_Link link in channels)
        {
            Console.WriteLine($"  - {link.Name}: {link.Url}");
        }

        var apps = c_Helper.ExtractApps(footerPart);
        Console.WriteLine($"\n[APLICACIONES] ({apps.Count} encontradas):");
        foreach (c_Link link in apps)
        {
            Console.WriteLine($"  - {link.Name}: {link.Url}");
        }

        var siteLinks = c_Helper.ExtractSiteLinks(footerPart);
        Console.WriteLine($"\n[SITIO] ({siteLinks.Count} enlaces encontrados):");
        foreach (c_Link link in siteLinks)
        {
            Console.WriteLine($"  - {link.Name}: {link.Url}");
        }

        var email = c_Helper.ExtractContactEmail(footerPart);
        if (!string.IsNullOrEmpty(email))
        {
            Console.WriteLine($"\n[CONTACTO]:");
            Console.WriteLine($"  - Email: {email}");
        }
    }

    private static async Task ScrapePagination(string content, string baseUrl)
    {
        Console.WriteLine("\n=== PAGINACIÓN ===");

        int totalPages = c_Helper.ExtractTotalPages(content);
        Console.WriteLine($"[INFO] Total de páginas detectadas: {totalPages}");

        var pageLinks = c_Helper.ExtractPageLinks(content, baseUrl);
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
        return c_Helper.RemoveHtmlComments(footerPart);
    }

    private static void ParseContent(string content)
    {
        c_NewsParser parser = new();
        List<c_NewsItem> news = parser.Parse(content);

        Console.WriteLine($"  -> {news.Count} artículos extraídos");
    }
}
