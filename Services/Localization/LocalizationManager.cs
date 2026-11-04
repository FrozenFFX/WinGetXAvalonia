// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.ComponentModel;
using Avalonia;
using Avalonia.Threading;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Services.Localization;

public sealed class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> Turkish =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["All"] = "Tümü",
            ["All tools"] = "Tüm araçlar",
            ["Automation"] = "Otomasyon",
            ["Browse"] = "Keşfet",
            ["Category"] = "Kategori",
            ["Check for updates automatically"] = "Güncellemeleri otomatik denetle",
            ["Choose the language used throughout WinGetX Studio."] = "WinGetX Studio'da kullanılacak dili seçin.",
            ["Select the interface language. Pages not yet localized remain in English."] = "Arayüz dilini seçin. Henüz yerelleştirilmemiş sayfalar İngilizce kalır.",
            ["Keyboard Shortcuts"] = "Klavye kısayolları",
            ["Close to tray"] = "Kapatınca sistem tepsisine küçült",
            ["Coming Soon"] = "Yakında",
            ["Customization"] = "Kişiselleştirme",
            ["Dashboard"] = "Pano",
            ["Developer"] = "Geliştirici",
            ["Diagnostics"] = "Tanılama",
            ["Enterprise"] = "Kurumsal",
            ["Experimental"] = "Deneysel",
            ["Favorites"] = "Favoriler",
            ["Filter by category"] = "Kategoriye göre filtrele",
            ["Filter by status"] = "Duruma göre filtrele",
            ["Founder Center"] = "Founder Merkezi",
            ["General"] = "Genel",
            ["History"] = "Geçmiş",
            ["Home"] = "Ana sayfa",
            ["Game Mode"] = "Oyun modu",
            ["Blueprints"] = "Planlar",
            ["Identity"] = "Kimlik",
            ["Installed"] = "Yüklü",
            ["Language"] = "Dil",
            ["Loading studio tools..."] = "Studio araçları yükleniyor...",
            ["Marketplace"] = "Mağaza",
            ["Minimize to tray"] = "Sistem tepsisine küçült",
            ["No studio tools found"] = "Studio aracı bulunamadı",
            ["No tools match"] = "Eşleşen araç yok",
            ["Open command palette"] = "Komut paletini aç",
            ["Open tool"] = "Aracı aç",
            ["Packages"] = "Paketler",
            ["Queue"] = "Kuyruk",
            ["Ready"] = "Hazır",
            ["Refresh studio tools"] = "Studio araçlarını yenile",
            ["Refresh studio tools (F5)"] = "Studio araçlarını yenile (F5)",
            ["Run when Windows starts"] = "Windows açıldığında çalıştır",
            ["Search studio tools..."] = "Studio araçlarında ara...",
            ["Security"] = "Güvenlik",
            ["Select a tool from the list to open its details."] = "Ayrıntılarını görmek için listeden bir araç seçin.",
            ["Settings"] = "Ayarlar",
            ["Show Founder Signature"] = "Founder imzasını göster",
            ["Sources"] = "Kaynaklar",
            ["Start minimized"] = "Simge durumunda başlat",
            ["Startup"] = "Başlangıç",
            ["Status"] = "Durum",
            ["Studio"] = "Studio",
            ["System"] = "Sistem",
            ["The selected language is saved automatically."] = "Seçilen dil otomatik olarak kaydedilir.",
            ["Toggle favorite"] = "Favorilere ekle veya çıkar",
            ["Tool Details"] = "Araç ayrıntıları",
            ["Tools"] = "Araçlar",
            ["Tips"] = "İpuçları",
            ["Retry"] = "Yeniden dene",
            ["Use filters to focus on Ready or Experimental tools."] = "Hazır veya Deneysel araçlara odaklanmak için filtreleri kullanın.",
            ["Search by name, description or category."] = "Ada, açıklamaya veya kategoriye göre arayın.",
            ["Try a different search or filter."] = "Farklı bir arama veya filtre deneyin.",
            ["Try a different search term or filter."] = "Farklı bir arama terimi veya filtre deneyin.",
            ["Updates"] = "Güncellemeler",
            ["Workflows"] = "İş akışları",
            ["Unable to load studio tools"] = "Studio araçları yüklenemedi",
            ["System health and environment report"] = "Sistem sağlığı ve ortam raporu",
            ["Operation History"] = "İşlem geçmişi",
            ["Review package operations and their results"] = "Paket işlemlerini ve sonuçlarını inceleyin",
            ["Reports"] = "Raporlar",
            ["Package and usage reports"] = "Paket ve kullanım raporları",
            ["Queue Scheduler"] = "Kuyruk zamanlayıcısı",
            ["Schedule and prioritize operations"] = "İşlemleri zamanlayın ve önceliklendirin",
            ["Package and system security checks"] = "Paket ve sistem güvenlik denetimleri",
            ["Collections"] = "Koleksiyonlar",
            ["Group apps into reusable sets"] = "Uygulamaları yeniden kullanılabilir gruplarda toplayın",
            ["Rules and event triggers"] = "Kurallar ve olay tetikleyicileri",
            ["Themes"] = "Temalar",
            ["Customize colors and appearance"] = "Renkleri ve görünümü özelleştirin",
            ["Plugins"] = "Eklentiler",
            ["Extend WinGetX with add-ons"] = "WinGetX'i eklentilerle genişletin",
            ["Exclusive Founder Edition tools"] = "Founder Edition'a özel araçlar",
            ["All packages"] = "Tüm paketler",
            ["{0} tools"] = "{0} araç",
            ["1 tool"] = "1 araç",
            ["No tools"] = "Araç yok",
            ["Appearance"] = "Görünüm",
            ["Performance"] = "Performans",
            ["Winget"] = "Winget",
            ["Notifications"] = "Bildirimler",
            ["Backup"] = "Yedekleme",
            ["About"] = "Hakkında",
            ["Toggle Theme"] = "Temayı değiştir",
            ["Create Backup"] = "Yedek oluştur",
            ["Restore Backup"] = "Yedeği geri yükle"
        };

    private readonly Dictionary<string, LocalizedText> _texts = new(StringComparer.Ordinal);

    public static LocalizationManager? Current { get; private set; }

    public string Language { get; private set; } = "English";

    public event EventHandler? LanguageChanged;

    public LocalizationManager(ISettingsService settings)
    {
        Current = this;
        Language = NormalizeLanguage(settings.Settings.Language);
        settings.SettingsChanged += OnSettingsChanged;
    }

    public LocalizedText GetText(string key)
    {
        if (!_texts.TryGetValue(key, out var text))
        {
            text = new LocalizedText(TranslateValue(key));
            _texts.Add(key, text);
        }

        return text;
    }

    public string TranslateValue(string value)
    {
        if (Language != "Turkish")
            return value;

        if (Turkish.TryGetValue(value, out var translation))
            return translation;

        foreach (var (format, translatedFormat) in Turkish)
        {
            if (!format.Contains("{0}", StringComparison.Ordinal))
                continue;

            var placeholder = format.IndexOf("{0}", StringComparison.Ordinal);
            var prefix = format[..placeholder];
            var suffix = format[(placeholder + 3)..];
            if (value.StartsWith(prefix, StringComparison.Ordinal)
                && value.EndsWith(suffix, StringComparison.Ordinal)
                && int.TryParse(
                    value[prefix.Length..(value.Length - suffix.Length)],
                    out var count))
            {
                return string.Format(translatedFormat, count);
            }
        }

        return value;
    }

    public void SetLanguage(string? language)
    {
        var normalized = NormalizeLanguage(language);
        if (Language == normalized)
            return;

        Language = normalized;

        foreach (var (key, text) in _texts)
            text.Value = TranslateValue(key);

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
    {
        if (e.ChangedProperties.Contains(nameof(Models.AppSettings.Language)))
        {
            void Apply() => SetLanguage(e.Settings.Language);

            if (Application.Current is null || Dispatcher.UIThread.CheckAccess())
                Apply();
            else
                Dispatcher.UIThread.Post(Apply);
        }
    }

    private static string NormalizeLanguage(string? language)
        => string.Equals(language, "Turkish", StringComparison.OrdinalIgnoreCase)
           || string.Equals(language, "Türkçe", StringComparison.OrdinalIgnoreCase)
            ? "Turkish"
            : "English";
}

public sealed class LocalizedText(string value) : INotifyPropertyChanged
{
    private string _value = value;

    public string Value
    {
        get => _value;
        set
        {
            if (_value == value)
                return;

            _value = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
