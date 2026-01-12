using MarkdownViewer.Services;
using MarkdownViewer.Models;

namespace MarkdownViewer.Tests;

public class MarkdownServiceTests
{
    private readonly MarkdownService _service = new();

    #region Metadata Extraction Tests

    [Fact]
    public void ExtractMetadata_WithCategories_ReturnsCategories()
    {
        // Arrange
        var content = @"# Test
<!--category-- ASP.NET, PostgreSQL, Search, RRF -->
Some content here.";

        // Act
        var metadata = _service.ExtractMetadata(content);

        // Assert
        Assert.True(metadata.HasMetadata);
        Assert.Equal(4, metadata.Categories.Count);
        Assert.Contains("ASP.NET", metadata.Categories);
        Assert.Contains("PostgreSQL", metadata.Categories);
        Assert.Contains("Search", metadata.Categories);
        Assert.Contains("RRF", metadata.Categories);
    }

    [Fact]
    public void ExtractMetadata_WithDateTime_ReturnsPublicationDate()
    {
        // Arrange
        var content = @"# Test
<datetime class=""hidden"">2026-01-14T12:00</datetime>
Some content here.";

        // Act
        var metadata = _service.ExtractMetadata(content);

        // Assert
        Assert.True(metadata.HasMetadata);
        Assert.NotNull(metadata.PublicationDate);
        Assert.Equal(2026, metadata.PublicationDate!.Value.Year);
        Assert.Equal(1, metadata.PublicationDate.Value.Month);
        Assert.Equal(14, metadata.PublicationDate.Value.Day);
    }

    [Fact]
    public void ExtractMetadata_WithBothTags_ReturnsBoth()
    {
        // Arrange
        var content = @"# Test
<!--category-- Testing, Mermaid -->
<datetime class=""hidden"">2026-01-12T14:00</datetime>
Content here.";

        // Act
        var metadata = _service.ExtractMetadata(content);

        // Assert
        Assert.True(metadata.HasMetadata);
        Assert.Equal(2, metadata.Categories.Count);
        Assert.NotNull(metadata.PublicationDate);
    }

    [Fact]
    public void ExtractMetadata_WithNoTags_ReturnsEmptyMetadata()
    {
        // Arrange
        var content = @"# Simple Document
Just plain markdown here.";

        // Act
        var metadata = _service.ExtractMetadata(content);

        // Assert
        Assert.False(metadata.HasMetadata);
        Assert.Empty(metadata.Categories);
        Assert.Null(metadata.PublicationDate);
    }

    [Fact]
    public void ExtractMetadata_CategoriesAreTrimmed()
    {
        // Arrange
        var content = "<!--category--   Spaces  ,  Around  ,  Values  -->";

        // Act
        var metadata = _service.ExtractMetadata(content);

        // Assert
        Assert.Equal(3, metadata.Categories.Count);
        Assert.Contains("Spaces", metadata.Categories);
        Assert.Contains("Around", metadata.Categories);
        Assert.Contains("Values", metadata.Categories);
    }

    #endregion

    #region Markdown Processing Tests

    [Fact]
    public void ProcessMarkdown_RemovesMetadataTags()
    {
        // Arrange
        var content = @"# Title
<!--category-- Test, Category -->
<datetime class=""hidden"">2026-01-14T12:00</datetime>
Body content here.";

        // Act
        var processed = _service.ProcessMarkdown(content);

        // Assert
        Assert.DoesNotContain("<!--category--", processed);
        Assert.DoesNotContain("<datetime", processed);
        Assert.Contains("# Title", processed);
        Assert.Contains("Body content here", processed);
    }

    [Fact]
    public void ProcessMarkdown_ConvertsMermaidToCodeBlock()
    {
        // Arrange
        var content = @"# Diagram Test

```mermaid
flowchart TD
    A --> B
```

After diagram.";

        // Act
        var processed = _service.ProcessMarkdown(content);

        // Assert
        Assert.Contains("Mermaid Diagram", processed);
        Assert.Contains("flowchart TD", processed);
        Assert.Contains("A --> B", processed);
        Assert.DoesNotContain("```mermaid", processed);
    }

    [Fact]
    public void ProcessMarkdown_PreservesRegularCodeBlocks()
    {
        // Arrange
        var content = @"# Code Test

```csharp
public class Test { }
```

```javascript
console.log('hello');
```";

        // Act
        var processed = _service.ProcessMarkdown(content);

        // Assert
        Assert.Contains("```csharp", processed);
        Assert.Contains("public class Test", processed);
        Assert.Contains("```javascript", processed);
        Assert.Contains("console.log", processed);
    }

    [Fact]
    public void ProcessMarkdown_HandlesMultipleMermaidBlocks()
    {
        // Arrange
        var content = @"# Multiple Diagrams

```mermaid
flowchart LR
    A --> B
```

Some text.

```mermaid
sequenceDiagram
    User->>Server: Request
```";

        // Act
        var processed = _service.ProcessMarkdown(content);

        // Assert
        Assert.DoesNotContain("```mermaid", processed);
        Assert.Contains("flowchart LR", processed);
        Assert.Contains("sequenceDiagram", processed);
    }

    [Fact]
    public void ProcessMarkdown_PreservesHeadings()
    {
        // Arrange
        var content = @"# Heading 1
## Heading 2
### Heading 3";

        // Act
        var processed = _service.ProcessMarkdown(content);

        // Assert
        Assert.Contains("# Heading 1", processed);
        Assert.Contains("## Heading 2", processed);
        Assert.Contains("### Heading 3", processed);
    }

    [Fact]
    public void ProcessMarkdown_PreservesLists()
    {
        // Arrange
        var content = @"- Item 1
- Item 2
  - Nested
1. Numbered
2. List";

        // Act
        var processed = _service.ProcessMarkdown(content);

        // Assert
        Assert.Contains("- Item 1", processed);
        Assert.Contains("- Nested", processed);
        Assert.Contains("1. Numbered", processed);
    }

    [Fact]
    public void ProcessMarkdown_PreservesTables()
    {
        // Arrange
        var content = @"| Column 1 | Column 2 |
|----------|----------|
| Data 1   | Data 2   |";

        // Act
        var processed = _service.ProcessMarkdown(content);

        // Assert
        Assert.Contains("| Column 1 |", processed);
        Assert.Contains("| Data 1   |", processed);
    }

    #endregion
}
