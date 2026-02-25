using System;
using System.IO;
using Xunit;

public class NewsParserTests
{
    // AI test gen
    [Fact]
    public void Parse_FormatedXml_Returns_Items_WithTitleAndLink()
    {
        string testFile = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Resources", "formated.xml"));
        Assert.True(File.Exists(testFile), $"Fixture file not found: {testFile}");

        Console.WriteLine(" - - - Reading file - - - ");
        string content = File.ReadAllText(testFile);
        var parser = new c_NewsParser();
        var list = parser.Parse(content);
        Console.WriteLine(" - - - Content Parsed - - - ");

        Assert.NotNull(list);
        Assert.NotEmpty(list);

        // Every item should have a non-empty title and a link (basic sanity checks)
        foreach (var item in list)
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Title));
            Assert.False(string.IsNullOrWhiteSpace(item.Link));
            System.Console.WriteLine(item.Title);
            System.Console.WriteLine(new string('-', 20));
        }
    }
}
