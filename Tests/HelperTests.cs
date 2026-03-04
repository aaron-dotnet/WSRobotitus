using System;
using System.IO;
using Xunit;

public class HelperTests
{
    private readonly string _footerHtml;

    public HelperTests()
    {
        string testFile = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Resources", "footer_only.html");
        if (!File.Exists(testFile))
        {
            testFile = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "footer_only.html");
        }
        string rawHtml = File.ReadAllText(testFile);
        _footerHtml = HtmlExtractors.RemoveHtmlComments(rawHtml);
    }

    [Fact]
    public void ExtractCategories_Returns10Categories()
    {
        var categories = HtmlExtractors.ExtractCategories(_footerHtml);
        
        Assert.Equal(10, categories.Count);
        Assert.Contains(categories, c => c.Name.Contains("ESPACIO"));
        Assert.Contains(categories, c => c.Name.Contains("FÍSICA"));
    }

    [Fact]
    public void ExtractSocialNetworks_ReturnsAtLeast2Networks()
    {
        var social = HtmlExtractors.ExtractSocialNetworks(_footerHtml);
        
        Assert.True(social.Count >= 2);
        Assert.Contains(social, s => s.Name.Contains("facebook", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(social, s => s.Name.Contains("youtube", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ExtractRelatedChannels_Returns2Channels()
    {
        string[] keywords = { "El Robot de Platón", "El Robot de Colón" };
        var channels = HtmlExtractors.ExtractRelatedChannels(_footerHtml, keywords);
        
        Assert.Equal(2, channels.Count);
        Assert.Contains(channels, c => c.Name.Contains("El Robot de Platón"));
        Assert.Contains(channels, c => c.Name.Contains("El Robot de Colón"));
    }

    [Fact]
    public void ExtractApps_Returns2Apps()
    {
        var apps = HtmlExtractors.ExtractApps(_footerHtml);
        
        Assert.Equal(2, apps.Count);
        Assert.Contains(apps, a => a.Url.Contains("apps.apple.com"));
        Assert.Contains(apps, a => a.Url.Contains("play.google.com"));
    }

    [Fact]
    public void ExtractSiteLinks_Returns5Links()
    {
        var siteLinks = HtmlExtractors.ExtractSiteLinks(_footerHtml);
        
        Assert.Equal(5, siteLinks.Count);
        Assert.Contains(siteLinks, l => l.Name.Contains("Inicio"));
        Assert.Contains(siteLinks, l => l.Name.Contains("Acerca de"));
    }

    [Fact]
    public void ExtractContactEmail_ReturnsEmail()
    {
        var email = HtmlExtractors.ExtractContactEmail(_footerHtml);
        
        Assert.NotNull(email);
        Assert.Equal("contacto@robotitus.com", email);
    }

    [Fact]
    public void ExtractAnchors_ReturnsMultipleLinks()
    {
        var anchors = HtmlExtractors.ExtractAnchors(_footerHtml);
        
        Assert.NotEmpty(anchors);
        Assert.All(anchors, a => Assert.False(string.IsNullOrEmpty(a.Url)));
    }

    [Fact]
    public void RemoveHtmlComments_RemovesComments()
    {
        string htmlWithComments = "<!-- comment --> <a href='test'>Link</a>";
        var result = HtmlExtractors.RemoveHtmlComments(htmlWithComments);
        
        Assert.DoesNotContain("<!--", result);
        Assert.DoesNotContain("-->", result);
        Assert.Contains("<a href='test'>Link</a>", result);
    }
}
