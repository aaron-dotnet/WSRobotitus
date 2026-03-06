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

        List<c_NewsItem> allNews = [];
        string mainContent = GetMainContent(content);
        allNews.AddRange(ParseContent(mainContent, print: false));

        if (pagesToProcess > 1)
        {
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
                            lock (allNews)
                            {
                                allNews.AddRange(ParseContent(pageMain, print: false));
                            }
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

        Console.WriteLine($"\n[TOTAL] Artículos encontrados: {allNews.Count}");

        if (_config.Scraper.ArticlesToScrape > 0)
        {
            await ScrapeArticles(allNews);
        }
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

    private static List<c_NewsItem> ParseContent(string content, bool print = true)
    {
        c_NewsParser parser = new();
        List<c_NewsItem> news = parser.Parse(content);

        if (print)
        {
            foreach (c_NewsItem thisnew in news)
            {
                Console.WriteLine($"{thisnew.Date:dd/MM/yyyy} | {thisnew.Author} - {thisnew.Title}");
            }

            Console.WriteLine($"  -> {news.Count} artículos extraídos");
        }

        return news;
    }

    private static string GetArticleContent(string article)
    {
        string start = "<div class=\"entry-content post-content\">";
        string end = "<footer class=\"entry-footer\">";

        string content = GetString(article, start, end).Replace(end, string.Empty);

        string adsBlock = "<div class='code-block code-block";
        while (content.Contains(adsBlock))
        {
            // quitamos los bloques de publicidad
            string part = GetString(content, adsBlock, "</div>", firstCoincidence: true);
            if (!string.IsNullOrEmpty(part))
                content = content.Replace(part, string.Empty);
        }

        // Convertir etiquetas p en saltos de línea
        content = content.Replace("</p>", "\n");
        content = content.Replace("<h2>", "## ").Replace("</h2>", "\n");
        content = content.Replace("<h3>", "### ").Replace("</h3>", "\n");

        content = HtmlExtractors.StripHtmlTags(content);
        content = HtmlExtractors.CleanWhitespace(content);

        // Eliminar espacios al inicio de cada línea
        var lines = content.Split('\n');
        content = string.Join("\n", lines.Select(l => l.Trim()));

        return content.Trim();
    }

    private static async Task ScrapeArticles(List<c_NewsItem> articles)
    {
        Console.WriteLine("\n=== ARTÍCULOS ===");
        int articlesToProcess = Math.Min(_config.Scraper.ArticlesToScrape, articles.Count);
        Console.WriteLine($"[INFO] Se procesarán los próximos {articlesToProcess} artículos\n");

        using var semaphore = new SemaphoreSlim(3);
        var tasks = new List<Task<c_Article>>();

        for (int i = 0; i < articlesToProcess; i++)
        {
            int index = i;
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var article = articles[index];
                    Console.WriteLine($"[{index + 1}] Obteniendo: {article.Title}");

                    string articleHtml = await FetchContent(article.Link, referer: article.Link, host: _config.Scraper.BaseUrl);

                    if (!string.IsNullOrEmpty(articleHtml))
                    {
                        string articleContent = GetArticleContent(articleHtml);
                        //Console.WriteLine($"    Contenido: {articleContent.Substring(0, Math.Min(100, articleContent.Length))}...");
                        Console.WriteLine($"    Contenido: \n{articleContent}");
                        Console.WriteLine("\n- - - - - - - - - - - - \n");
                    }
                }
                finally
                {
                    semaphore.Release();
                }
                return new c_Article("", "", "", "", "", DateTime.Now, "", "", 0);
            }));
        }

        await Task.WhenAll(tasks);
    }
}
