using System.Text.RegularExpressions;

/// <summary>
/// Centraliza todos los patrones Regex utilizados en el proyecto
/// </summary>
public static partial class RegexPatterns
{
    /// <summary>Extrae comentarios HTML</summary>
    [GeneratedRegex("<!--.*?-->", RegexOptions.Singleline)]
    public static partial Regex HtmlComment();

    /// <summary>Extrae enlaces &lt;a&gt; con href y contenido interno</summary>
    [GeneratedRegex(@"<a[^>]+href\s*=\s*[""']([^""']+)[""'][^>]*>([\s\S]*?)</a>", RegexOptions.IgnoreCase)]
    public static partial Regex AnchorTag();

    /// <summary>Extrae rutas de categoría (/category/...)</summary>
    [GeneratedRegex(@"/category/([^/""]+)/?", RegexOptions.IgnoreCase)]
    public static partial Regex CategoryPath();

    /// <summary>Identifica URLs de redes sociales</summary>
    [GeneratedRegex(@"(facebook\.com|youtube\.com|twitter\.com|x\.com|instagram\.com|tiktok\.com)", RegexOptions.IgnoreCase)]
    public static partial Regex SocialNetworkUrl();

    /// <summary>Extrae números de página (/page/...)</summary>
    [GeneratedRegex(@"page/(\d+)", RegexOptions.IgnoreCase)]
    public static partial Regex PageNumber();

    /// <summary>Extrae direcciones de email</summary>
    [GeneratedRegex(@"mailto:([^""'>]+)")]
    public static partial Regex EmailAddress();
}
