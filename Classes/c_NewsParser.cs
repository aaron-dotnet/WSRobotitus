using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class c_NewsParser
{
    public List<c_NewsItem> Parse(string content)
    {
        XElement root = XElement.Parse(content);

        var articles = root.Descendants("div")
            .Where(d => d.Attribute("class").Value.Contains("news-post"));

        return articles.Select(ExtractNewsItem).ToList();
    }

    private c_NewsItem ExtractNewsItem(XElement article)
    {
        return new c_NewsItem
        {
            Title = ExtractTitle(article),
            Link = ExtractLink(article),
            ImageLink = ExtractImage(article),
            Description = ExtractDescription(article),
            Author = ExtractAuthor(article),
            Date = ExtractDate(article)
        };
    }

    private string ExtractTitle(XElement article)
    {
        return article.Descendants("h2").First().Element("a").Value.Trim();
    }

    private string ExtractLink(XElement article)
    {
        return article.Descendants("h2").First().Element("a").Attribute("href").Value;
    }

    private string ExtractImage(XElement article)
    {
        XElement img = article.Descendants("img").First();
        var src = img.Attribute("src")?.Value;

        if (!string.IsNullOrEmpty(src))
            return src;

        var srcset = img.Attribute("srcset").Value;
        var firstUrl = srcset.Split(',').First().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).First();

        return firstUrl;
    }

    private string ExtractDescription(XElement article)
    {
        var postContent = article.Descendants()
            .First(e => e.Attribute("class")?.Value.Contains("post-content") ?? false);

        return postContent.Elements("p").First().Value.Trim();
    }

    private string ExtractAuthor(XElement article)
    {
        var meta = article.Descendants()
            .First(e => e.Attribute("class")?.Value.Contains("entry-meta") ?? false);

        var textNode = meta.Nodes()
            .OfType<XText>()
            .FirstOrDefault()
            ?.Value.Trim()
            .Replace("\u00A0", " ").Trim();

        return textNode ?? string.Empty;
    }

    private DateTime ExtractDate(XElement article)
    {
        var meta = article.Descendants()
            .First(e => e.Attribute("class")?.Value.Contains("entry-meta") ?? false);

        var dateStr = meta.Descendants("time").First().Attribute("datetime").Value;

        return DateTime.Parse(dateStr);
    }
}
