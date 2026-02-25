using System.IO;
using System.Text;

public static class c_Functions
{
    private static readonly Lazy<string> _configPath = new(() => InitializeConfigPath());
    private static readonly object _syncLock = new();

    private static string InitializeConfigPath()
    {
        string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string configPath = Path.Combine(userHome, ".config", "wsrobotitus");

        try
        {
            // $HOME/.config/wsrobotitus
            // %UserProfile%\.config\wsrobotitus
            Directory.CreateDirectory(configPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al inicializar config: {ex.Message}");
            return string.Empty;
        }
        return configPath;
    }

    public static void Log(string message, LogLevel level = LogLevel.INFO)
    {
        string configPaht = _configPath.Value;

        if (string.IsNullOrEmpty(configPaht))
        {
            return;
        }

        string logPath = Path.Combine(configPaht, "log.txt");
        lock (_syncLock)
        {
            try
            {
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] - {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al escribir log: {ex.Message}");
            }
        }
    }
    public static void SaveToFile(string fileName, string content)
    {
        Encoding encoding = Encoding.UTF8;
        File.WriteAllText(fileName, content, encoding);
    }
    public static string GetString(string fullString, string startStr, string endStr,
                                    int excessAmount = 0, bool firstCoincidence = false)
    {
        int startWord, endWord;
        StringComparison comparison = StringComparison.OrdinalIgnoreCase;

        startWord = fullString.IndexOf(startStr, comparison);
        if (startWord == -1) return string.Empty;

        if (firstCoincidence)
        {
            endWord = fullString.IndexOf(endStr, fullString.IndexOf(startStr), comparison);
        }
        else
        {
            endWord = fullString.LastIndexOf(endStr, comparison);
        }

        return fullString.AsSpan(startWord, endWord - startWord + endStr.Length - excessAmount).ToString();
    }
}
