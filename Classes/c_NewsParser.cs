using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;

public class c_NewsParser
{
    public List<c_NewsItem> Parse(string content)
    {
        var list = new List<c_NewsItem>();
        if (string.IsNullOrWhiteSpace(content)) return list;

        XElement? root = null;
        try
        {
            root = XElement.Parse(content);
        }
        catch
        {
            try
            {
                root = XElement.Parse($"<root>{content}</root>");
            }
            catch
            {
                return list;
            }
        }

        List<XElement> posts = root.Descendants("div").Where(d => (((string)d.Attribute("class")) ?? "").Contains("news-post")).ToList();
        foreach (var art in posts)
        {
            try
            {
                var h2 = art.Descendants("h2").FirstOrDefault();
                var aTitle = h2?.Element("a");
                string title = aTitle?.Value?.Trim() ?? string.Empty;
                string link = aTitle?.Attribute("href")?.Value ?? string.Empty;

                var img = art.Descendants("img").FirstOrDefault();
                string image = string.Empty;
                if (img != null)
                {
                    image = img.Attribute("src")?.Value ?? string.Empty;
                    if (string.IsNullOrEmpty(image))
                    {
                        var srcset = img.Attribute("srcset")?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(srcset))
                        {
                            var first = srcset.Split(',').FirstOrDefault()?.Trim();
                            if (!string.IsNullOrEmpty(first))
                            {
                                var parts = first.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                image = parts[0];
                            }
                        }
                    }
                }

                var postContent = art.Descendants().FirstOrDefault(e => (((string)e.Attribute("class")) ?? "").Contains("post-content"));
                string description = postContent?.Elements("p").FirstOrDefault()?.Value?.Trim() ?? string.Empty;

                var meta = art.Descendants().FirstOrDefault(e => (((string)e.Attribute("class")) ?? "").Contains("entry-meta"));
                string author = string.Empty;
                string dateIso = string.Empty;
                if (meta != null)
                {
                    var textNodes = meta.Nodes().OfType<XText>().Select(t => t.Value.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    if (textNodes.Count > 0)
                        author = textNodes[0].Replace("\u00A0", " ").Trim();

                    var timeEl = meta.Descendants().FirstOrDefault(e => e.Name.LocalName == "time");
                    if (timeEl != null)
                    {
                        var dtAttr = timeEl.Attribute("datetime")?.Value;
                        if (!string.IsNullOrEmpty(dtAttr))
                            dateIso = dtAttr;
                        else
                        {
                            var txt = timeEl.Value?.Trim();
                            if (DateTime.TryParse(txt, new CultureInfo("es-ES"), DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                                dateIso = dt.ToString("o");
                            else if (DateTime.TryParse(txt, out dt))
                                dateIso = dt.ToString("o");
                        }
                    }
                }

                var item = new c_NewsItem
                {
                    Title = title,
                    Author = author,
                    Date = DateTime.Parse(dateIso),
                    Description = description,
                    Link = link,
                    Image = image
                };
                list.Add(item);
            }
            catch
            {
                continue;
            }
        }

        return list;
    }
}
