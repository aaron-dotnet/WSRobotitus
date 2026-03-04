using System.Text.RegularExpressions;

/// <summary>
/// Utilidades para extracción de información del HTML usando Regex
/// </summary>
public static class c_Helper
{
    public static string RemoveHtmlComments(string input) =>
        RegexPatterns.HtmlComment().Replace(input, string.Empty);

    /// <summary>
    /// Extrae todos los enlaces &lt;a&gt; del HTML con su URL y nombre
    /// </summary>
    public static List<c_Link> ExtractAnchors(string input)
    {
        return RegexPatterns.AnchorTag().Matches(input)
            .Select(m =>
            {
                string url = m.Groups[1].Value;
                string innerHtml = m.Groups[2].Value;
                string name = Regex.Replace(innerHtml, @"<[^>]+>", "").Trim();

                if (string.IsNullOrEmpty(name))
                {
                    var imgMatch = Regex.Match(innerHtml, @"alt\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                    if (imgMatch.Success)
                        name = imgMatch.Groups[1].Value;
                }

                return new c_Link(name, url);
            })
            .ToList();
    }

    /// <summary>
    /// Extrae enlaces que coinciden con un filtro personalizado
    /// </summary>
    public static List<c_Link> ExtractAnchorsBy(string html, Func<c_Link, bool> filter) =>
        ExtractAnchors(html).Where(filter).ToList();

    public static List<c_Link> ExtractCategories(string html) =>
        ExtractAnchorsBy(html, link => RegexPatterns.CategoryPath().IsMatch(link.Url));

    public static List<c_Link> ExtractSocialNetworks(string html) =>
        ExtractAnchorsBy(html, link => RegexPatterns.SocialNetworkUrl().IsMatch(link.Url));

    public static List<c_Link> ExtractRelatedChannels(string html, string[] keywords) =>
        ExtractAnchorsBy(html, link => keywords.Any(k => link.Name.Contains(k, StringComparison.OrdinalIgnoreCase)));

    public static List<c_Link> ExtractApps(string html) =>
        ExtractAnchorsBy(html, link => link.Url.Contains("apps.apple.com") || link.Url.Contains("play.google.com"));

    public static List<c_Link> ExtractSiteLinks(string html)
    {
        string[] excluded = { "category", "facebook", "youtube", "twitter", "instagram", "tiktok", "apps.apple", "play.google", "on-cloud", "mailto:" };

        return ExtractAnchorsBy(html, link =>
            (!link.Url.StartsWith("http") || link.Url.Contains("robotitus.com")) &&
            !excluded.Any(p => link.Url.Contains(p)) &&
            !string.IsNullOrEmpty(link.Name))
            .GroupBy(a => a.Name)
            .Select(g => g.First())
            .ToList();
    }

    public static string? ExtractContactEmail(string html)
    {
        var match = RegexPatterns.EmailAddress().Match(html);
        return match.Success ? match.Groups[1].Value : null;
    }

    public static int ExtractTotalPages(string content) =>
        RegexPatterns.PageNumber().Matches(content)
            .Select(m => int.TryParse(m.Groups[1].Value, out int p) ? p : 0)
            .DefaultIfEmpty(1)
            .Max();

    public static List<string> ExtractPageLinks(string content, string baseUrl) =>
        Enumerable.Range(1, ExtractTotalPages(content))
            .Select(i => $"{baseUrl}/page/{i}")
            .ToList();
}
