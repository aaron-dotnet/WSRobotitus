using System.Text.RegularExpressions;

public static partial class c_Helper
{
    [GeneratedRegex("<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex HtmlCommentRegex();

    [GeneratedRegex(@"<a[^>]+href\s*=\s*[""']([^""']+)[""'][^>]*>([\s\S]*?)</a>", RegexOptions.IgnoreCase)]
    private static partial Regex AnchorRegex();

    [GeneratedRegex(@"/category/([^/""]+)/?", RegexOptions.IgnoreCase)]
    private static partial Regex CategoryRegex();

    [GeneratedRegex(@"(facebook\.com|youtube\.com|twitter\.com|x\.com|instagram\.com|tiktok\.com)", RegexOptions.IgnoreCase)]
    private static partial Regex SocialNetworkRegex();

    [GeneratedRegex(@"page/(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex PageNumberRegex();

    [GeneratedRegex(@"mailto:([^""'>]+)")]
    private static partial Regex EmailRegex();

    public static string RemoveHtmlComments(string input) =>
        HtmlCommentRegex().Replace(input, string.Empty);

    public static List<c_Link> ExtractAnchors(string input)
    {
        return AnchorRegex().Matches(input)
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

    public static List<c_Link> ExtractByPattern(string html, Func<string, bool> matcher) =>
        ExtractAnchors(html).Where(a => matcher(a.Url)).ToList();

    public static List<c_Link> ExtractCategories(string html) =>
        ExtractByPattern(html, url => CategoryRegex().IsMatch(url));

    public static List<c_Link> ExtractSocialNetworks(string html) =>
        ExtractByPattern(html, url => SocialNetworkRegex().IsMatch(url));

    public static List<c_Link> ExtractRelatedChannels(string html, string[] keywords) =>
        ExtractAnchors(html).Where(a => keywords.Any(k => a.Name.Contains(k, StringComparison.OrdinalIgnoreCase))).ToList();

    public static List<c_Link> ExtractApps(string html) =>
        ExtractByPattern(html, url => url.Contains("apps.apple.com") || url.Contains("play.google.com"));

    public static List<c_Link> ExtractSiteLinks(string html)
    {
        string[] excluded = { "category", "facebook", "youtube", "twitter", "instagram", "tiktok", "apps.apple", "play.google", "on-cloud", "mailto:" };

        return ExtractAnchors(html).Where(a =>
            !a.Url.StartsWith("http") || a.Url.Contains("robotitus.com"))
            .Where(a => !excluded.Any(p => a.Url.Contains(p)))
            .Where(a => !string.IsNullOrEmpty(a.Name))
            .GroupBy(a => a.Name)
            .Select(g => g.First())
            .ToList();
    }

    public static string? ExtractContactEmail(string html) =>
        EmailRegex().Match(html).Success ? EmailRegex().Match(html).Groups[1].Value : null;

    public static int ExtractTotalPages(string content) =>
        PageNumberRegex().Matches(content).Select(m => int.TryParse(m.Groups[1].Value, out int p) ? p : 0).DefaultIfEmpty(1).Max();

    public static List<string> ExtractPageLinks(string content, string baseUrl) =>
        Enumerable.Range(1, ExtractTotalPages(content)).Select(i => $"{baseUrl}/page/{i}").ToList();
}
