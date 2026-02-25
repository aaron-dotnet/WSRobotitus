using static c_Functions;


internal class Program
{
    //string main_url = "https://robotitus.com";
    const string URL = "https://robotitus.com/category/tecnologia";
    private static string _host = "robotitus.com";
    private static string _referer = "https://robotitus.com/";
    private static async Task Main(string[] args)
    {
        string content = string.Empty;
        using (c_Scraper scraper = new())
        {
            scraper.Host = _host;
            scraper.Referer = _referer;
            content = await scraper.Get(URL);
        }

        // si no hay nada se sale.
        if (string.IsNullOrEmpty(content)) return;

        content = GetString(content, "<main", "</main>");
        string navPart = GetString(content, "<nav", "</nav>");
        content = content.Replace(navPart, "").Replace("&nbsp;", "").Replace("&hellip;", "");

        ParseContent(content);
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
            Console.WriteLine($"Link: {item.Link}");
            Console.WriteLine("Description: " + item.Description);
            Console.WriteLine($"Date: {item.Date:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine($"Image: {item.ImageLink}");
        }
    }
}