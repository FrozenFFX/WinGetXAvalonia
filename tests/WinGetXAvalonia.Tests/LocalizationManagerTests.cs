// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.RegularExpressions;
using System.Xml.Linq;
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
            Assert.Equal(
                "245 yüklü • 3 güncelleme",
                localization.TranslateValue("245 Installed • 3 Updates"));
            Assert.Equal(
                "Güncelleme tamamlandı — 3 başarılı, 2 başarısız, 1 iptal edildi.",
                localization.TranslateValue("Update complete — 3 successful, 2 failed, 1 cancelled."));
            Assert.Equal(
                "Notepad paketini güncelle",
                localization.TranslateValue("Update Notepad"));
            Assert.Equal(
                "'Notepad' ile eşleşen paket bulunamadı.",
                localization.TranslateValue("No package matched 'Notepad'."));
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

    [Fact]
    public async Task TurkishLocalization_CoversStaticViewText()
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
            localization.SetLanguage("Turkish");

            var repository = new DirectoryInfo(AppContext.BaseDirectory);
            while (repository is not null
                   && !File.Exists(Path.Combine(repository.FullName, "WinGetXAvalonia.csproj")))
            {
                repository = repository.Parent;
            }

            Assert.NotNull(repository);
            var untranslated = new HashSet<string>(StringComparer.Ordinal);
            var technicalTerms = new HashSet<string>(StringComparer.Ordinal)
            {
                ".NET", "Avalonia", "Chrome", "Ctrl + K", "CSV", "Enter", "Excel", "F5",
                "Görkem Yıldız",
                "Founder", "Git", "GitHub", "HTML", "JSON", "OBS Studio", "OS", "PDF", "RAM",
                "VSCode", "WinGetX", "WinGetX Studio", "Windows", "Windows 10",
                "Windows 11", "Winget", "v0.2.0-alpha"
            };
            var keyPattern = new Regex(
                "^\\{loc:Translate Key=\"(?<key>.*)\"\\}$",
                RegexOptions.CultureInvariant);

            var uiDirectories = new[] { "Views", "Controls", "Dialogs" };
            foreach (var directory in uiDirectories)
            {
                foreach (var file in Directory.EnumerateFiles(
                             Path.Combine(repository!.FullName, directory),
                             "*.axaml",
                             SearchOption.AllDirectories))
                {
                    var document = XDocument.Load(file);
                    foreach (var attribute in document.Descendants().Attributes())
                    {
                        var match = keyPattern.Match(attribute.Value);
                        if (!match.Success)
                            continue;

                        var key = match.Groups["key"].Value;
                        if (!technicalTerms.Contains(key)
                            && localization.TranslateValue(key) == key)
                        {
                            untranslated.Add(key);
                        }
                    }

                    foreach (var element in document.Descendants()
                                 .Where(element => element.Name.LocalName == "String"))
                    {
                        var value = element.Value;
                        if (!string.IsNullOrWhiteSpace(value)
                            && !technicalTerms.Contains(value)
                            && localization.TranslateValue(value) == value)
                        {
                            untranslated.Add(value);
                        }
                    }
                }
            }

            Assert.True(
                untranslated.Count == 0,
                string.Join(Environment.NewLine, untranslated.OrderBy(value => value)));
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
