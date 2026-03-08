# AGENTS.md - WSRobotitus Development Guide

## Project Overview
WSRobotitus is a web scraper for robotitus.com written in C# (.NET 10.0). It extracts news articles, footer info, and handles pagination using xUnit for testing.

---

## Build Commands
```bash
dotnet build                    # Build entire solution
dotnet build WSRobotitus.csproj #dotnet build -c Release         # Release Build specific project
 build
dotnet run                      # Run application
dotnet run -- categoria         # Run with category argument
```

## Test Commands
```bash
dotnet test                                    # Run all tests
dotnet test -v n                               # Verbose output
dotnet test --filter "FullyQualifiedName~TestMethodName"  # Single test
dotnet test Tests/WSRobotitus.Tests.csproj     # Specific project
dotnet test --collect:"XPlat Code Coverage"   # With coverage
```

---

## Code Style Guidelines

### Project Structure
- Main source: Root directory
- Tests: `Tests/`
- Enums: `Enums/`
- Classes: `Classes/`

### Language Features
- **Implicit Usings**: Enabled - no explicit system `using` statements
- **Nullable Reference Types**: Enabled
- **Target-typed new**: Use `new()` instead of `new ClassName()`
- **Collection expressions**: Prefer `[]`, `[.. items]` over `new List<T>()`

### Naming Conventions
| Element | Convention | Example |
|---------|------------|---------|
| Classes/Methods/Properties | PascalCase | `NewsParser`, `Parse()`, `BaseUrl` |
| Private fields | _camelCase | `_config`, `_client` |
| Constants | PascalCase | `MaxRetries`, `TimeoutSeconds` |
| Records | PascalCase | `NewsItem` |
| Parameters | camelCase | `fullString`, `startStr` |

### File Organization
- Use **file-scoped namespaces**: `namespace WSRobotitus.Classes;`
- Group classes by type in appropriate directories

### Formatting
- 4 spaces for indentation (no tabs)
- Opening brace on same line
- Use expression-bodied members when appropriate
- Use `var` when type is obvious
- Keep lines under 120 characters

### Imports
- Use file-scoped namespaces instead of block namespaces
- Project imports: `using WSRobotitus.Classes;`, `using WSRobotitus.Enums;`

### Error Handling
- Use try-catch for operations that may fail (file I/O, network)
- Return empty collections (`[]`) rather than null
- Log errors: `Helper.Log(message, LogLevel.ERROR)`
- Log warnings: `Helper.Log(message, LogLevel.WARN)`

---

## Common Patterns

### Dictionary with Func delegates (Program.cs)
```csharp
private static readonly Dictionary<string, Func<string, List<Link>>> FooterExtractors = new()
{
    ["SECCIONES"] = html => [.. HtmlExtractors.ExtractAllAnchors(html).Where(LinkFilters.IsCategory)],
};
```

### Pattern matching (NewsParser.cs)
```csharp
private static string GetFirstText(XNode? node) =>
    node switch
    {
        XElement el => el.Value.Trim(),
        XText tx => tx.Value.Trim(),
        _ => string.Empty,
    };
```

### Records for DTOs
```csharp
public record NewsItem(string Title, string Link, string ImageLink, string Description, string Author, DateTime Date);
```

### Async with Semaphore (Program.cs)
```csharp
using var semaphore = new SemaphoreSlim(3);
List<Task> tasks = [];
// ... add tasks
await Task.WhenAll(tasks);
```

### HTTP Client with retry (Scraper.cs)
- Use `HttpClient` with `SocketsHttpHandler` for connection pooling
- Implement retry logic with exponential backoff
- Dispose properly using `IDisposable`

### Console Logging
- Use `[INFO]`, `[WARN]`, `[ERROR]` prefixes

---

## Configuration (appsettings.json)
```json
{
  "Scraper": { "BaseUrl": "robotitus.com", "BaseReferer": "https://robotitus.com/", "DefaultCategory": "tecnologia", "PagesToScrape": 3, "ArticlesToScrape": 3 },
  "Output": { "SaveToFile": true, "OutputFormat": "json", "OutputDirectory": "./output" }
}
```

---

## Dependencies
- Microsoft.NET.Test.Sdk (17.9.0)
- xunit (2.5.3)
- xunit.runner.visualstudio (2.5.3)
- System.Xml.Linq (built-in)

## Notes
- Project targets .NET 10.0
- Comments may be in Spanish
- No linting tools configured
