using System.Text.RegularExpressions;

public static partial class c_Helper
{
    [GeneratedRegex("<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex HtmlCommentRegex();

    public static string RemoveHtmlComments(string input) =>
        HtmlCommentRegex().Replace(input, string.Empty);

}
