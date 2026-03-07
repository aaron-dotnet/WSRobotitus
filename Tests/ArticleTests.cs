using System;
using System.IO;
using System.Linq;
using Xunit;
using WSRobotitus.Classes;

public class ArticleTests
{
    [Fact]
    public void c_Article_Extends_c_NewsItem()
    {
        var article = new Article(
            "Test Title",
            "https://robotitus.com/test",
            "https://robotitus.com/image.jpg",
            "Test description",
            "Author Name",
            new DateTime(2026, 3, 5),
            "Article content here",
            "TECNOLOGIA",
            5
        );

        Assert.Equal("Test Title", article.Title);
        Assert.Equal("https://robotitus.com/test", article.Link);
        Assert.Equal("https://robotitus.com/image.jpg", article.ImageLink);
        Assert.Equal("Test description", article.Description);
        Assert.Equal("Author Name", article.Author);
        Assert.Equal(new DateTime(2026, 3, 5), article.Date);
        Assert.Equal("Article content here", article.Content);
        Assert.Equal("TECNOLOGIA", article.Category);
        Assert.Equal(5, article.ReadingTimeMinutes);
    }

    [Fact]
    public void StripHtmlTags_RemovesAllHtmlTags()
    {
        string html = "<p>Hello <b>World</b></p><a href='test'>Link</a>";
        string result = HtmlExtractors.StripHtmlTags(html);

        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
        Assert.Equal("Hello WorldLink", result);
    }

    [Fact]
    public void StripHtmlTags_PreservesTextContent()
    {
        string html = "<div><h1>Title</h1><p>Paragraph with <span>span</span></p></div>";
        string result = HtmlExtractors.StripHtmlTags(html);

        Assert.Equal("TitleParagraph with span", result);
    }

    [Fact]
    public void CleanWhitespace_ReplacesMultipleSpaces()
    {
        string input = "Hello    World   Test";
        string result = HtmlExtractors.CleanWhitespace(input);

        Assert.Equal("Hello World Test", result);
    }

    [Fact]
    public void CleanWhitespace_ReplacesMultipleSpacesAndPreservesNewline()
    {
        string input = "Line1\nLine2   Line3";
        string result = HtmlExtractors.CleanWhitespace(input);

        Assert.Equal("Line1\nLine2 Line3", result);
    }

    [Fact]
    public void CleanWhitespace_HandlesEmptyString()
    {
        string input = "";
        string result = HtmlExtractors.CleanWhitespace(input);

        Assert.Equal("", result);
    }

    [Fact]
    public void CleanWhitespace_HandlesStringWithOnlyWhitespace()
    {
        string input = "   ";
        string result = HtmlExtractors.CleanWhitespace(input);

        Assert.Equal(" ", result);
    }
}
