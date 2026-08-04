using WinGetXAvalonia.Helpers;

namespace WinGetXAvalonia.Tests;

public class WingetParserTests
{
    [Fact]
    public void ParseInstalled_ReadsColumns()
    {
        var lines = new List<string>
        {
            "Name  Id  Version  Available  Source",
            "----  --  -------  ---------  ------",
            "1Password  AgileBits.1Password  > 8.12.30.21  winget",
            "App Installer  Microsoft.AppInstaller  1.29.279.0  1.29.280.0  winget"
        };

        var result = WingetParser.ParseInstalled(lines);

        Assert.Equal(2, result.Count);

        Assert.Equal("1Password", result[0].Name);
        Assert.Equal("AgileBits.1Password", result[0].Id);
        Assert.Equal("8.12.30.21", result[0].Version);
        Assert.Equal("winget", result[0].Source);

        Assert.Equal("App Installer", result[1].Name);
        Assert.Equal("1.29.279.0", result[1].Version);
        Assert.Equal("winget", result[1].Source);
    }

    [Fact]
    public void ParseInstalled_EmptySourceAllowed()
    {
        var lines = new List<string>
        {
            "Name  Id  Version  Available  Source",
            "----  --  -------  ---------  ------",
            "AV1 Video Extension  MSIX\\Microsoft.AV1VideoExtension  2.0.24.0"
        };

        var result = WingetParser.ParseInstalled(lines);

        Assert.Single(result);
        Assert.Equal("AV1 Video Extension", result[0].Name);
        Assert.Equal("2.0.24.0", result[0].Version);
        Assert.Equal("", result[0].Source);
    }

    [Fact]
    public void ParseUpdates_ReadsVersionAndAvailable()
    {
        var lines = new List<string>
        {
            "Name  Id  Version  Available  Source",
            "----  --  -------  ---------  ------",
            "App Installer  Microsoft.AppInstaller  1.29.279.0  1.29.280.0  winget"
        };

        var result = WingetParser.ParseUpdates(lines);

        Assert.Single(result);
        Assert.Equal("App Installer", result[0].Name);
        Assert.Equal("Microsoft.AppInstaller", result[0].Id);
        Assert.Equal("1.29.279.0", result[0].Version);
        Assert.Equal("1.29.280.0", result[0].Available);
        Assert.Equal("winget", result[0].Source);
    }

    [Fact]
    public void ParseSearch_ReadsColumnsAndSkipsMatch()
    {
        var lines = new List<string>
        {
            "Name  Id  Version  Match  Source",
            "----  --  -------  -----  ------",
            "Microsoft Visual Studio Code  Microsoft.VisualStudioCode  1.130.0  Moniker: vscode  winget",
            "Codium  Alex313031.Codium  1.93.1.24277  Tag: vscode  winget"
        };

        var result = WingetParser.ParseSearch(lines);

        Assert.Equal(2, result.Count);

        Assert.Equal("Microsoft Visual Studio Code", result[0].Name);
        Assert.Equal("Microsoft.VisualStudioCode", result[0].Id);
        Assert.Equal("1.130.0", result[0].Version);
        Assert.Equal("winget", result[0].Source);

        Assert.Equal("Codium", result[1].Name);
        Assert.Equal("1.93.1.24277", result[1].Version);
    }

    [Fact]
    public void Parse_SkipsHeaderAndShortLines()
    {
        var lines = new List<string>
        {
            "Name  Id  Version",
            "----  --  -------",
            "Too short",
            "Valid  Valid.Id  1.0.0  winget"
        };

        var result = WingetParser.ParseInstalled(lines);

        Assert.Single(result);
        Assert.Equal("Valid", result[0].Name);
    }

    [Fact]
    public void ParseDetails_ReadsShowOutputFields()
    {
        var lines = new List<string>
        {
            "Found Git [Git.Git] Version 2.52.0",
            "Name: Git",
            "Publisher: Git for Windows",
            "Version: 2.52.0",
            "Homepage: https://git-scm.com/",
            "License: MIT",
            "Description: Distributed version control system.",
            "ReleaseNotes: Improvements and bug fixes."
        };

        var result = WingetParser.ParseDetails(lines);

        Assert.Equal("Git", result.Name);
        Assert.Equal("Git for Windows", result.Publisher);
        Assert.Equal("2.52.0", result.Version);
        Assert.Equal("https://git-scm.com/", result.Homepage);
        Assert.Equal("MIT", result.License);
        Assert.Equal("Distributed version control system.", result.Description);
        Assert.Equal("Improvements and bug fixes.", result.ReleaseNotes);
    }

    [Fact]
    public void ParseDetails_MissingFieldsStayEmpty()
    {
        var lines = new List<string>
        {
            "Found Git [Git.Git] Version 2.52.0",
            "Name: Git",
            "Version: 2.52.0"
        };

        var result = WingetParser.ParseDetails(lines);

        Assert.Equal("Git", result.Name);
        Assert.Equal("", result.Publisher);
        Assert.Equal("", result.License);
        Assert.Equal("", result.ReleaseNotes);
    }

    [Fact]
    public void ParseDetails_SkipsInvalidHomepage()
    {
        var lines = new List<string>
        {
            "Homepage: not-a-url",
            "Name: Git"
        };

        var result = WingetParser.ParseDetails(lines);

        Assert.Equal("", result.Homepage);
    }
}
