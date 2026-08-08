// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Domain.Palette;

namespace WinGetXAvalonia.Tests;

public class PaletteQueryParserTests
{
    [Theory]
    [InlineData(">install", true)]
    [InlineData(" > vscode", true)]
    [InlineData("install", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("  ", false)]
    public void IsCommandMode_DetectsCommandPrefix(string? input, bool expected)
    {
        Assert.Equal(expected, PaletteQueryParser.IsCommandMode(input));
    }

    [Theory]
    [InlineData(">install vscode", "install vscode")]
    [InlineData(">", "")]
    [InlineData("> ", "")]
    [InlineData("> settings", "settings")]
    [InlineData("install", "install")]
    public void CommandText_StripsCommandPrefix(string? input, string expected)
    {
        Assert.Equal(expected, PaletteQueryParser.CommandText(input));
    }

    [Theory]
    [InlineData("install vscode", "install", "vscode")]
    [InlineData("install   chrome", "install", "chrome")]
    [InlineData("refresh", "refresh", "")]
    [InlineData("open settings", "open", "settings")]
    [InlineData("", "", "")]
    [InlineData(null, "", "")]
    public void Split_ExtractsVerbAndArgument(string? text, string verb, string argument)
    {
        var (actualVerb, actualArgument) = PaletteQueryParser.Split(text);

        Assert.Equal(verb, actualVerb);
        Assert.Equal(argument, actualArgument);
    }

    [Fact]
    public void Split_LowercasesVerbOnly()
    {
        var (verb, argument) = PaletteQueryParser.Split("Install Visual Studio Code");

        Assert.Equal("install", verb);
        Assert.Equal("Visual Studio Code", argument);
    }
}
