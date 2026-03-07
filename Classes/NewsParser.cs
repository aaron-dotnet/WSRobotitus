namespace WSRobotitus.Classes;

using System.Xml.Linq;
using WSRobotitus.Enums;

public class NewsParser
{
    public List<NewsItem> Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return [];

        try
        {
            XElement root = XElement.Parse(content);
            List<NewsItem> articles = [.. root.Descendants("div")
                .Where(d => d.Attribute("class")?.Value?
                .Contains("news-post") == true)
                .Select(ExtractNewsItem)];

            return articles;
        }
        catch (Exception ex)
        {
            Helper.Log(ex.Message, LogLevel.ERROR);
            return [];
        }
    }
    private NewsItem ExtractNewsItem(XElement article)
    {
        XElement? h2 = article.Descendants("h2").FirstOrDefault();
        XElement? meta = GetByClass(article, "entry-meta");
        XElement? content = GetByClass(article, "post-content");

        XElement? anchor = h2?.Element("a");

        return new NewsItem(
            Title: GetFirstText(anchor),
            Link: anchor?.Attribute("href")?.Value ?? string.Empty,
            ImageLink: GetImageSrc(article) ?? string.Empty,
            Description: GetFirstText(content?.Element("p")),
            Author: GetFirstText(meta?.Nodes().OfType<XText>().FirstOrDefault()),
            Date: ExtractDate(meta)
        );
    }

    private static XElement? GetByClass(XElement root, string className) =>
        root.Descendants()
            .FirstOrDefault(e => e.Attribute("class")?.Value?
            .Contains(className) == true);

    private static string GetFirstText(XNode? node) =>
        node switch
        {
            XElement el => el.Value.Trim(),
            XText tx => tx.Value.Trim(),
            _ => string.Empty,
        };

    private static string? GetImageSrc(XElement article)
    {
        // busca la tag <img>
        XElement? img = article.Descendants("img").FirstOrDefault();
        if (img == null) return null;

        // revisa si tiene una imagen en el atributo 'src'
        string? source = img.Attribute("src")?.Value;
        if (!string.IsNullOrEmpty(source)) return source;

        // caso contrario busca en el atributo 'srcset'
        // y solo obtiene el primer enlace (part)
        string? srcset = img.Attribute("srcset")?.Value;
        return srcset?.Split(',')
            .Select(part => part.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
            .FirstOrDefault(url => !string.IsNullOrEmpty(url));
    }

    private static DateTime ExtractDate(XElement? meta)
    {
        if (meta == null) return DateTime.MinValue;
        //const string iso = "yyyy-MM-ddTHH:mm:ssK";

        string? dateStr = meta.Descendants("time")
            .FirstOrDefault()?.Attribute("datetime")?.Value;

        return DateTime.TryParse(dateStr, out DateTime result) ? result : DateTime.MinValue;
    }
}