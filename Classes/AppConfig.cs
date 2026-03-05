using System.Text.Json;
using System.IO;

public static class AppConfig
{
    private static Settings? _settings;

    public static Settings Load(string configPath = "appsettings.json")
    {
        if (_settings != null) return _settings;

        try
        {
            if (!File.Exists(configPath))
            {
                c_Functions.Log($"Archivo de configuración no encontrado: {configPath}", LogLevel.WARN);
                _settings = GetDefaultSettings();
                return _settings;
            }

            string json = File.ReadAllText(configPath);
            JsonDocument doc = JsonDocument.Parse(json);

            _settings = new Settings
            {
                Scraper = ParseScraperConfig(doc.RootElement.GetProperty("Scraper")),
                Output = ParseOutputConfig(doc.RootElement.GetProperty("Output"))
            };
        }
        catch (Exception ex)
        {
            c_Functions.Log($"Error al cargar configuración: {ex.Message}", LogLevel.ERROR);
            _settings = GetDefaultSettings();
        }

        return _settings;
    }

    private static ScraperConfig ParseScraperConfig(JsonElement elem) => new()
    {
        BaseUrl = elem.GetProperty("BaseUrl").GetString() ?? "robotitus.com",
        BaseReferer = elem.GetProperty("BaseReferer").GetString() ?? "https://robotitus.com/",
        DefaultCategory = elem.GetProperty("DefaultCategory").GetString() ?? "tecnologia",
        PagesToScrape = elem.GetProperty("PagesToScrape").GetInt32()
    };

    private static OutputConfig ParseOutputConfig(JsonElement elem) => new()
    {
        SaveToFile = elem.GetProperty("SaveToFile").GetBoolean(),
        OutputFormat = elem.GetProperty("OutputFormat").GetString() ?? "json",
        OutputDirectory = elem.GetProperty("OutputDirectory").GetString() ?? "./output"
    };

    private static Settings GetDefaultSettings() => new()
    {
        Scraper = new ScraperConfig
        {
            BaseUrl = "robotitus.com",
            BaseReferer = "https://robotitus.com/",
            DefaultCategory = "tecnologia",
            PagesToScrape = 10
        },
        Output = new OutputConfig
        {
            SaveToFile = true,
            OutputFormat = "json",
            OutputDirectory = "./output"
        }
    };
}

public class Settings
{
    public ScraperConfig Scraper { get; set; } = new();
    public OutputConfig Output { get; set; } = new();
}

public class ScraperConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string BaseReferer { get; set; } = string.Empty;
    public string DefaultCategory { get; set; } = string.Empty;
    public int PagesToScrape { get; set; }
}

public class OutputConfig
{
    public bool SaveToFile { get; set; }
    public string OutputFormat { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
}
