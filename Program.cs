using System.Text;
using System.Xml.Linq;
using static System.Net.WebUtility;

internal class Program
{
    //string main_url = "https://robotitus.com";
    const string URL = "https://robotitus.com/category/tecnologia";
    private static string _host = "robotitus.com";
    private static string _referer = "https://robotitus.com/";
    private static async Task Main(string[] args)
    {
        using c_Scraper scraper = new()
        {
            Host = _host,
            Referer = _referer
        };

        string content = await scraper.Get(URL);
        content = GetString(content, "<main", "</main>");

        string navPart = GetString(content, "<nav", "</nav>");
        content = content.Replace(navPart, "").Replace("&nbsp;", "").Replace("&hellip;", "");

        ParseContent(content);
        // ToDo: Mejorar filtro, listar articulos, y paginacion y leer un articulo.
    }
    private static void ParseContent(string content)
    {
        XElement xe = XElement.Parse(content);
        //SaveToFile("filtered.xml", content);
    }
    private static void SaveToFile(string fileName, string content)
    {
        Encoding encoding = Encoding.UTF8;
        System.IO.File.WriteAllText(fileName, content, encoding);
    }
    private static string GetString(string fullString, string startStr, string endStr,
                                    int excessAmount = 0, bool firstCoincidence = false)
    {
        int startWord = fullString.IndexOf(startStr, StringComparison.OrdinalIgnoreCase);
        if (startWord == -1) return string.Empty;

        int endWord;

        if (firstCoincidence)
        {
            endWord = fullString.IndexOf(endStr, fullString.IndexOf(startStr), StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            endWord = fullString.LastIndexOf(endStr, StringComparison.OrdinalIgnoreCase);
        }

        return fullString.AsSpan(startWord, endWord - startWord + endStr.Length - excessAmount).ToString();
    }
}