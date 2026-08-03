// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Helpers;

namespace WinGetXAvalonia.Tests;

// kaneki
public class FeaturedParserTests
{
    [Fact]
    public void Parse_ReadsTitleAndId()
    {
        const string json = """
            [
              { "title": "Git", "id": "Git.Git" },
              { "title": "Visual Studio Code", "id": "Microsoft.VisualStudioCode" }
            ]
            """;

        var items = FeaturedParser.Parse(json);

        Assert.Equal(2, items.Count);
        Assert.Equal("Git", items[0].Title);
        Assert.Equal("Git.Git", items[0].Id);
        Assert.Equal("Visual Studio Code", items[1].Title);
        Assert.Equal("Microsoft.VisualStudioCode", items[1].Id);
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsEmpty()
        => Assert.Empty(FeaturedParser.Parse(""));

    [Fact]
    public void Parse_InvalidJson_ReturnsEmpty()
        => Assert.Empty(FeaturedParser.Parse("not json"));
}
// kaneki
