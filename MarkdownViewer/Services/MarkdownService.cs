using System.Text.RegularExpressions;
using MarkdownViewer.Models;

namespace MarkdownViewer.Services;

public partial class MarkdownService
{
    private string? _basePath;
    private string? _baseUrl;

    public void SetBasePath(string? path)
    {
        _basePath = path;
        _baseUrl = null;
    }

    public void SetBaseUrl(string? url)
    {
        _baseUrl = url?.TrimEnd('/');
        _basePath = null;
    }

    /// <summary>
    /// Extract metadata from markdown content (categories, publication date)
    /// </summary>
    public DocumentMetadata ExtractMetadata(string content)
    {
        var metadata = new DocumentMetadata();

        // Extract categories: <!--category-- ASP.NET, PostgreSQL, Search -->
        var categoryMatch = CategoryRegex().Match(content);
        if (categoryMatch.Success)
        {
            var categoriesStr = categoryMatch.Groups[1].Value;
            metadata.Categories = categoriesStr
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        // Extract publication date: <datetime class="hidden">2026-01-14T12:00</datetime>
        var dateMatch = DatetimeRegex().Match(content);
        if (dateMatch.Success && DateTime.TryParse(dateMatch.Groups[1].Value, out var pubDate))
        {
            metadata.PublicationDate = pubDate;
        }

        return metadata;
    }

    public string ProcessMarkdown(string content)
    {
        // Remove metadata tags from rendered content (they'll be shown separately)
        content = CategoryRegex().Replace(content, "");
        content = DatetimeRegex().Replace(content, "");

        // Process relative image paths
        content = ProcessImagePaths(content);

        // Process mermaid code blocks (placeholder for now)
        content = ProcessMermaidBlocks(content);

        return content.Trim();
    }

    private string ProcessImagePaths(string content)
    {
        var imageRegex = ImageRegex();

        return imageRegex.Replace(content, match =>
        {
            var alt = match.Groups[1].Value;
            var path = match.Groups[2].Value;

            // Already absolute URL - leave as-is
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return match.Value;
            }

            // Already absolute file path - leave as-is
            if (Path.IsPathRooted(path))
            {
                return match.Value;
            }

            // Resolve relative path
            string resolvedPath;

            if (!string.IsNullOrEmpty(_baseUrl))
            {
                // URL-based resolution
                var cleanPath = path.TrimStart('.', '/');
                resolvedPath = $"{_baseUrl}/{cleanPath}";
            }
            else if (!string.IsNullOrEmpty(_basePath))
            {
                // File-based resolution
                resolvedPath = Path.GetFullPath(Path.Combine(_basePath, path));
                // Convert to file URI for Avalonia
                resolvedPath = new Uri(resolvedPath).AbsoluteUri;
            }
            else
            {
                return match.Value;
            }

            return $"![{alt}]({resolvedPath})";
        });
    }

    private string ProcessMermaidBlocks(string content)
    {
        var mermaidRegex = MermaidBlockRegex();

        return mermaidRegex.Replace(content, match =>
        {
            var mermaidCode = match.Groups[1].Value.Trim();

            // Display as a styled code block with diagram type indicator
            // Future: Could shell out to mmdc or use a WASM renderer
            return $"""
                > **Mermaid Diagram**

                ```
                {mermaidCode}
                ```
                """;
        });
    }

    [GeneratedRegex(@"!\[(.*?)\]\(([^)]+)\)", RegexOptions.Compiled)]
    private static partial Regex ImageRegex();

    [GeneratedRegex(@"```mermaid\s*\n([\s\S]*?)```", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex MermaidBlockRegex();

    // Metadata extraction patterns
    [GeneratedRegex(@"<!--\s*category\s*--\s*(.+?)\s*-->", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CategoryRegex();

    [GeneratedRegex(@"<datetime[^>]*>([^<]+)</datetime>", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex DatetimeRegex();
}
