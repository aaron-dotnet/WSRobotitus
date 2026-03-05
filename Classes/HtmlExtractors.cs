using System.Text.RegularExpressions;
using static c_Functions;

public static partial class HtmlExtractors
{
    private const string Domain = "robotitus.com";

    private static readonly string[] SocialDomains =
    [
        "facebook.com", "youtube.com", "twitter.com", "x.com", "instagram.com", "tiktok.com"
    ];

    private static readonly string[] AppStoreDomains =
    [
        "apps.apple.com", "play.google.com"
    ];

    private static readonly string[] DefaultRelatedChannels =
    [
        "El Robot de Platón", "El Robot de Colón"
    ];

    private static readonly string[] ExcludedPatterns =
    [
        "category", "on-cloud", "mailto:"
    ];

    [GeneratedRegex("<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex HtmlCommentRegex();

    [GeneratedRegex(@"<a[^>]+href\s*=\s*[""']([^""']+)[""'][^>]*>([\s\S]*?)</a>", RegexOptions.IgnoreCase)]
    private static partial Regex AnchorTagRegex();

    [GeneratedRegex(@"/category/([^/""']+)/?", RegexOptions.IgnoreCase)]
    private static partial Regex CategoryPathRegex();

    [GeneratedRegex(@"page/(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex PageNumberRegex();

    [GeneratedRegex(@"mailto:([^""'>]+)")]
    private static partial Regex EmailAddressRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex StripHtmlTagsRegex();

    [GeneratedRegex(@"alt\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex ExtractAltRegex();

    public static string RemoveHtmlComments(string input) =>
        HtmlCommentRegex().Replace(input, string.Empty);

    public static List<c_Link> ExtractAnchors(string input) =>
        AnchorTagRegex().Matches(input)
            .Select(m =>
            {
                string url = m.Groups[1].Value;
                string innerHtml = m.Groups[2].Value;
                string name = StripHtmlTagsRegex().Replace(innerHtml, "").Trim();

                if (string.IsNullOrEmpty(name))
                {
                    var imgMatch = ExtractAltRegex().Match(innerHtml);
                    if (imgMatch.Success)
                        name = imgMatch.Groups[1].Value;
                }

                return new c_Link(name, url);
            })
            .ToList();

    public static List<c_Link> ExtractAnchors(string input, Func<c_Link, bool> filter) =>
        ExtractAnchors(input).Where(filter).ToList();

    public static List<c_Link> ExtractByDomain(string html, string[] domains, bool exactMatch = false) =>
        ExtractAnchors(html, link =>
            exactMatch
                ? domains.Any(d => Uri.TryCreate(link.Url, UriKind.Absolute, out var u) && u.Host.Equals(d, StringComparison.OrdinalIgnoreCase))
                : domains.Any(d => link.Url.Contains(d, StringComparison.OrdinalIgnoreCase)));

    public static List<c_Link> ExtractByPath(string html, string[] paths) =>
        ExtractAnchors(html, link => paths.Any(p => link.Url.Contains(p, StringComparison.OrdinalIgnoreCase)));

    public static List<c_Link> ExtractCategories(string html) =>
        ExtractAnchors(html, link => CategoryPathRegex().IsMatch(link.Url));

    public static List<c_Link> ExtractSocialNetworks(string html) =>
        ExtractByDomain(html, SocialDomains);

    public static List<c_Link> ExtractRelatedChannels(string html, string[]? keywords = null) =>
        ExtractAnchors(html, link =>
            (keywords ?? DefaultRelatedChannels)
                .Any(k => link.Name.Contains(k, StringComparison.OrdinalIgnoreCase)));

    public static List<c_Link> ExtractApps(string html) =>
        ExtractByDomain(html, AppStoreDomains);

    public static List<c_Link> ExtractSiteLinks(string html) =>
        ExtractAnchors(html, link =>
            link.Url.Contains(Domain, StringComparison.OrdinalIgnoreCase) &&
            !ExcludedPatterns.Any(p => link.Url.Contains(p)) &&
            !SocialDomains.Any(d => link.Url.Contains(d)) &&
            !AppStoreDomains.Any(d => link.Url.Contains(d)) &&
            !string.IsNullOrEmpty(link.Name))
            .GroupBy(a => a.Name)
            .Select(g => g.First())
            .ToList();

    public static string? ExtractContactEmail(string html)
    {
        var match = EmailAddressRegex().Match(html);
        return match.Success ? match.Groups[1].Value : null;
    }

    public static int ExtractTotalPages(string html)
    {
        string start, end, filteredContent;

        // Todo el nav de pagination
        start = "<nav class=\"navigation pagination\"";
        end = "</nav>";
        filteredContent = GetString(html, start, end, firstCoincidence: true);

        // Seccionamos para evitar duplicados
        start = "<span class=\"page-numbers dots\">";
        end = "</nav>";
        filteredContent = GetString(filteredContent, start, end);

        // Link de la última página
        start = "<a class=\"page-numbers\" href=\"";
        end = "\">";
        string link = GetString(filteredContent, start, end, firstCoincidence: true)
            .Replace(end, string.Empty)
            .Trim();

        if (string.IsNullOrEmpty(link)) return 1;

        //string? part = link.Split('/').LastOrDefault();
        return int.TryParse(link.Split('/').Last(), out int pageNum) ? pageNum : 1;
    }
}
