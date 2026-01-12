using MarkdownViewer.Models;

namespace MarkdownViewer.Tests;

public class DocumentMetadataTests
{
    [Fact]
    public void HasMetadata_WithCategories_ReturnsTrue()
    {
        var metadata = new DocumentMetadata
        {
            Categories = ["Test", "Category"]
        };

        Assert.True(metadata.HasMetadata);
    }

    [Fact]
    public void HasMetadata_WithDate_ReturnsTrue()
    {
        var metadata = new DocumentMetadata
        {
            PublicationDate = DateTime.Now
        };

        Assert.True(metadata.HasMetadata);
    }

    [Fact]
    public void HasMetadata_WithBoth_ReturnsTrue()
    {
        var metadata = new DocumentMetadata
        {
            Categories = ["Test"],
            PublicationDate = DateTime.Now
        };

        Assert.True(metadata.HasMetadata);
    }

    [Fact]
    public void HasMetadata_Empty_ReturnsFalse()
    {
        var metadata = new DocumentMetadata();

        Assert.False(metadata.HasMetadata);
    }

    [Fact]
    public void HasMetadata_EmptyCategories_ReturnsFalse()
    {
        var metadata = new DocumentMetadata
        {
            Categories = []
        };

        Assert.False(metadata.HasMetadata);
    }

    [Fact]
    public void Categories_DefaultsToEmptyList()
    {
        var metadata = new DocumentMetadata();

        Assert.NotNull(metadata.Categories);
        Assert.Empty(metadata.Categories);
    }

    [Fact]
    public void PublicationDate_DefaultsToNull()
    {
        var metadata = new DocumentMetadata();

        Assert.Null(metadata.PublicationDate);
    }
}
