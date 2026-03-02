using static c_Functions;


internal class Program
{
    //string main_url = "https://robotitus.com";
    const string URL = "https://robotitus.com/category/tecnologia";
    const string HOST = "robotitus.com";
    const string REFERER = "https://robotitus.com/";
    private static async Task Main(string[] args)
    {
        //Log("Init");
        string content = string.Empty;
        using (c_Scraper scraper = new())
        {
            scraper.Host = HOST;
            scraper.Referer = REFERER;
            content = await scraper.Get(URL);
        }

        // si no hay nada se sale.
        if (string.IsNullOrEmpty(content)) return;

        content = GetString(content, "<main", "</main>");
        string navPart = GetString(content, "<nav", "</nav>");
        content = content.Replace(navPart, "").Replace("&nbsp;", "").Replace("&hellip;", "");

        ParseContent(content);

        // FOOTER:
        // string footerPart = GetFooter(content);
        // XElement xe = XElement.Parse(footerPart);
        // Console.WriteLine(xe);
    }

    private static string GetPagination(string content)
    {
        string startStr = "<nav class=\"navigation pagination\">";
        string endStr = "</nav>";
        string navPart = GetString(content, startStr, endStr, firstCoincidence: true);

        // Paginación, asi podemos saber cuantas páginas se pueden scrapear.
        // https://robotitus.com/category/[CATEGORIA]/page/[2,3,...]
        // La primera página no tiene '/page/1'
        // ToDo: Implementar la lógica.
        return navPart;
    }

    private static string GetFooter(string content)
    {
        string startStr = "<footer";
        string endStr = "</footer>";
        string footerPart = GetString(content, startStr, endStr, firstCoincidence: true);


        footerPart = c_Helper.RemoveHtmlComments(footerPart);

        // Enlaces de las redes de Robotitus y categorias.
        // ToDo: Implementar la lógica.
        return footerPart;
    }

    private static void ParseContent(string content)
    {
        c_NewsParser parser = new();
        List<c_NewsItem> news = parser.Parse(content);


        Console.WriteLine($"Extracted {news.Count} news items.");
        foreach (c_NewsItem item in news)
        {
            Console.WriteLine(new string('-', 20));
            Console.WriteLine($"Title: {item.Title}");
            Console.WriteLine($"Author: {item.Author}");
            Console.WriteLine($"Link: {item.Link}");
            Console.WriteLine("Description: " + item.Description);
            Console.WriteLine($"Date: {item.Date:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine($"Image: {item.ImageLink}");
        }
    }
}