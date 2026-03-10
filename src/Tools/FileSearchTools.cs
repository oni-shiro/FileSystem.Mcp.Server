using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using FileSystem.Mcp.Server.Services;
using ModelContextProtocol.Server;

namespace FileSystem.Mcp.Server.Tools;

[McpServerToolType]
internal class FileSearchTools
{
    private IFileSystemService _fileSystemService;
    private RootProvider _rootProvider;

    public FileSearchTools(IFileSystemService fileSystemService, RootProvider rootProvider)
    {
        _fileSystemService = fileSystemService;
        _rootProvider = rootProvider;
    }


    [McpServerTool]
    [Description("Searches for a text query inside files (txt, pdf, xlsx) and returns matching snippets.")]
    public async Task<string> SearchInsideFiles(
        [Description("The directory to search in (relative to root)")] string? directory,
        [Description("The text or regex to search for")] string query,
        [Description("Whether to search subdirectories")] bool recursive = true)
    {
        var searchPath = _rootProvider.Resolve(directory ?? "");
        var results = new List<SearchResult>();

        var files = Directory.EnumerateFiles(searchPath, "*.*",
            recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            try
            {
                // Get the text version of the file via our Lenses
                string content = _fileSystemService.ReadFile(file);

                // Find matches and extract snippets
                var matches = Regex.Matches(content, Regex.Escape(query), RegexOptions.IgnoreCase);

                foreach (Match match in matches.Take(5)) // Limit to 5 snippets per file
                {
                    results.Add(new SearchResult
                    {
                        FileName = Path.GetFileName(file),
                        Path = _rootProvider.Resolve(file),
                        Snippet = GetContextSnippet(content, match.Index, query.Length)
                    });
                }
            }
            catch { /* Skip files that the lens can't read */ }
        }

        return results.Any()
            ? JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true })
            : "No matches found.";
    }

    private string GetContextSnippet(string text, int index, int matchLength)
    {
        int start = Math.Max(0, index - 50);
        int end = Math.Min(text.Length, index + matchLength + 50);
        return "..." + text.Substring(start, end - start).Replace("\n", " ") + "...";
    }
}

public class SearchResult
{
    public string? FileName { get; set; }
    public string? Path { get; set; }
    public string? Snippet { get; set; }
}
    