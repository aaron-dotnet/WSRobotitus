using static c_Functions;

internal class Program
{
    const string BASE_URL = "robotitus.com";
    const string BASE_REFERER = "https://robotitus.com/";
    const int PAGES_TO_SCRAPE = 10;

    static string CATEGORY = "tecnologia";

    private static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            CATEGORY = args[0].ToLower();
        }

        string url = $"https://{BASE_URL}/category/{CATEGORY}";
        Console.WriteLine($"[INFO] Categoría: {CATEGORY}");
        Console.WriteLine($"[INFO] URL: {url}");

        string content = await FetchContent(url);

        if (string.IsNullOrEmpty(content))
        {
            Console.WriteLine("[ERROR] No se pudo obtener el contenido");
            return;
        }

        ExtractFooterInfo(content, url);

        await ScrapePagination(content, url);

        Console.WriteLine("\n=== PROCESO COMPLETADO ===");
    }

    private static async Task<string> FetchContent(string url, string? referer = null)
    {
        Console.WriteLine("[INFO] Descargando contenido desde la web...");
        using (c_Scraper scraper = new())
        {
            scraper.Host = BASE_URL;
            scraper.Referer = referer ?? BASE_REFERER;
            return await scraper.Get(url);
        }
    }

    private static void ExtractFooterInfo(string content, string baseUrl)
    {
        Console.WriteLine("\n=== EXTRACCIÓN DEL FOOTER ===");

        string footerPart = GetFooter(content);
        if (string.IsNullOrEmpty(footerPart))
        {
            Console.WriteLine("[WARN] No se encontró el footer");
            return;
        }

        var categories = c_Helper.ExtractCategories(footerPart);
        Console.WriteLine($"\n[SECCIONES] ({categories.Count} encontradas):");
        foreach (var (name, url) in categories)
        {
            Console.WriteLine($"  - {name}: {url}");
        }

        var social = c_Helper.ExtractSocialNetworks(footerPart);
        Console.WriteLine($"\n[REDES SOCIALES] ({social.Count} encontradas):");
        foreach (var (name, url) in social)
        {
            Console.WriteLine($"  - {name}: {url}");
        }

        string[] relatedKeywords = { "El Robot de Platón", "El Robot de Colón" };
        var channels = c_Helper.ExtractRelatedChannels(footerPart, relatedKeywords);
        Console.WriteLine($"\n[CANALES RELACIONADOS] ({channels.Count} encontrados):");
        foreach (var (name, url) in channels)
        {
            Console.WriteLine($"  - {name}: {url}");
        }

        var apps = c_Helper.ExtractApps(footerPart);
        Console.WriteLine($"\n[APLICACIONES] ({apps.Count} encontradas):");
        foreach (var (name, url) in apps)
        {
            Console.WriteLine($"  - {name}: {url}");
        }

        var siteLinks = c_Helper.ExtractSiteLinks(footerPart);
        Console.WriteLine($"\n[SITIO] ({siteLinks.Count} enlaces encontrados):");
        foreach (var (name, url) in siteLinks)
        {
            Console.WriteLine($"  - {name}: {url}");
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

        int pagesToProcess = Math.Min(PAGES_TO_SCRAPE, totalPages);
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
