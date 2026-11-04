// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Localization;

namespace WinGetXAvalonia.Tests;

public class LocalizationManagerTests
{
    [Fact]
    public async Task LanguageSetting_UpdatesBoundTextAndPersists()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"),
            "settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        try
        {
            var settings = new SettingsService(new SettingsStore(path));
            await settings.LoadAsync();
            var localization = new LocalizationManager(settings);
            var text = localization.GetText("General");
            var languageChanged = 0;
            localization.LanguageChanged += (_, _) => languageChanged++;

            localization.SetLanguage("Turkish");
            Assert.Equal("Genel", text.Value);
            Assert.Equal("2 araç", localization.TranslateValue("2 tools"));
            Assert.Equal(1, languageChanged);

            await settings.UpdateAsync(current => current.Language = "Turkish");
            Assert.Equal("Turkish", settings.Settings.Language);
            var reloaded = new SettingsStore(path);
            await reloaded.LoadAsync();
            Assert.Equal("Turkish", reloaded.Settings.Language);

            await settings.UpdateAsync(current => current.Language = "English");
            localization.SetLanguage("English");
            Assert.Equal("General", text.Value);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Theory]
    [InlineData("Turkish", "Turkish")]
    [InlineData("Türkçe", "Turkish")]
    [InlineData("English", "English")]
    [InlineData("Unknown", "English")]
    public void SettingsValidator_NormalizesSupportedLanguages(
        string input,
        string expected)
    {
        var settings = new WinGetXAvalonia.Models.AppSettings { Language = input };

        SettingsValidator.Validate(settings);

        Assert.Equal(expected, settings.Language);
    }
}
