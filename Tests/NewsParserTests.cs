using System;
using System.IO;
using Xunit;
using WSRobotitus.Classes;

public class NewsParserTests
{
    // AI test gen
    [Fact]
    public void Parse_ParsedXml_Returns_Items_WithTitleAndLink()
    {
        string testFile = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Resources", "parsed.xml"));
        Assert.True(File.Exists(testFile), $"Fixture file not found: {testFile}");

        Console.WriteLine(" - - - Reading file - - - ");
        string content = File.ReadAllText(testFile);
        var parser = new NewsParser();
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
