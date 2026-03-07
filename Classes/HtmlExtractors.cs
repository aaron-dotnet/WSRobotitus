using System.Text.RegularExpressions;

public static partial class HtmlExtractors
{

    [GeneratedRegex("<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex HtmlCommentRegex();

    [GeneratedRegex(@"<a[^>]+href\s*=\s*[""']([^""']+)[""'][^>]*>([\s\S]*?)</a>", RegexOptions.IgnoreCase)]
    private static partial Regex AnchorTagRegex();

    [GeneratedRegex(@"mailto:([^""'>]+)")]
    private static partial Regex EmailAddressRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex StripHtmlTagsRegex();

    [GeneratedRegex(@"alt\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex ExtractAltRegex();

    [GeneratedRegex(@"[^\S\n]+")]
    private static partial Regex CleanWhitespaceRegex();

    public static string RemoveHtmlComments(string input) =>
        HtmlCommentRegex().Replace(input, string.Empty);

    public static string StripHtmlTags(string input) =>
        StripHtmlTagsRegex().Replace(input, string.Empty);

    public static string CleanWhitespace(string input) =>
        CleanWhitespaceRegex().Replace(input, " ");

    public static List<c_Link> ExtractAllAnchors(string input)
    {
        return [.. AnchorTagRegex()
            .Matches(input)
            .Select(match =>
            {
                string url = match.Groups[1].Value;
                string innerHtml = match.Groups[2].Value;
                string name = StripHtmlTagsRegex().Replace(innerHtml, "").Trim();

                if (string.IsNullOrEmpty(name))
                {
                    Match imgMatch = ExtractAltRegex().Match(innerHtml);
                    if (imgMatch.Success)
                        name = imgMatch.Groups[1].Value;
                }

                return new c_Link(name, url);
            })];
    }

    public static string? ExtractContactEmail(string html)
    {
        Match match = EmailAddressRegex().Match(html);
        return match.Success ? match.Groups[1].Value : null;
    }

    public static int ExtractTotalPages(string html)
    {
        string start, end, filteredContent;

        start = "<nav class=\"navigation pagination\"";
        end = "</nav>";
        filteredContent = c_Functions.GetString(html, start, end, firstCoincidence: true);

        start = "<span class=\"page-numbers dots\">";
        end = "</nav>";
        filteredContent = c_Functions.GetString(filteredContent, start, end);

        start = "<a class=\"page-numbers\" href=\"";
        end = "\">";
        string link = c_Functions.GetString(filteredContent, start, end, firstCoincidence: true)
            .Replace(end, string.Empty)
            .Trim();

        if (string.IsNullOrEmpty(link)) return 1;

        return int.TryParse(link.Split('/').Last(), out int pageNum) ? pageNum : 1;
    }
}

public static partial class LinkFilters
{
    private const string Domain = "robotitus.com";

    private static readonly string[] SocialDomains =
        [
            "facebook.com", "youtube.com", "twitter.com",
            "x.com", "instagram.com", "tiktok.com"
        ];

    private static readonly string[] AppStoreDomains =
        ["apps.apple.com", "play.google.com"];

    private static readonly string[] DefaultRelatedChannels =
        ["El Robot de Platón", "El Robot de Colón"];

    private static readonly string[] ExcludedPatterns =
        ["category", "on-cloud", "mailto:"];

    [GeneratedRegex(@"/category/([^/""']+)/?", RegexOptions.IgnoreCase)]
    private static partial Regex CategoryPathRegex();

    public static bool IsCategory(c_Link link) =>
        CategoryPathRegex().IsMatch(link.Url);

    public static bool IsSocialNetwork(c_Link link) =>
        MatchByDomain(link, SocialDomains);

    public static bool IsApp(c_Link link) =>
        MatchByDomain(link, AppStoreDomains);

    public static bool IsSiteLink(c_Link siteLink)
    {
        return siteLink switch
        {
            null => false,
            c_Link link when string.IsNullOrEmpty(link.Name) => false,
            c_Link link when !link.Url.Contains(Domain, StringComparison.OrdinalIgnoreCase) => false,
            c_Link link when ExcludedPatterns.Any(link.Url.Contains) => false,
            c_Link link when SocialDomains.Any(link.Url.Contains) => false,
            c_Link link when AppStoreDomains.Any(link.Url.Contains) => false,
            _ => true,
        };
    }

    public static bool IsRelatedChannel(c_Link link, string[]? keywords = null)
    {
        string[]? terms = keywords ?? DefaultRelatedChannels;
        return terms.Any(keyword => link.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchByDomain(c_Link link, string[] domains) =>
        domains.Any(d => link.Url.Contains(d, StringComparison.OrdinalIgnoreCase));
}
