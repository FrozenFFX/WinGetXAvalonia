# WinGetX Technical Debt

> Şimdi düzeltilmesi gerekmeyen ama unutulmaması gereken yapısal işler.
> Kural: aktif backlog'a giremez, v1.0'a dokunmaz. v1.1 planlanırken tekrar değerlendirilir.

## 1. Services/Core klasörünü feature bazlı böl

**Durum:** 🔜 v1.1 öncesi
**Etki:** Orta

`Services/Core/` büyüyor (16 dosya, farklı sorumluluklar tek klasörde). Önerilen bölünme:

```
Services/
├── Clipboard/      ClipboardService, IClipboardService
├── Launcher/       LauncherService, ILauncherService
├── Navigation/     NavigationService
├── Settings/       SettingsService, SettingsStore, SettingsValidator
├── Search/         PackageSearchCache
├── Winget/         WingetService
├── Logging/        LoggerService
├── Time/           ClockService, TimeService
├── Network/        GithubService
└── ...
```

Kararsız kalanlar:
- `FavoritesService` → `Services/Favorites/` önerisi
- `FeaturedCatalog`, `IFeaturedCatalog` → `Services/Featured/` önerisi

Her taşımada namespace güncellemesi + `Program.cs` DI kaydı kontrolü gerekir.

## 2. SettingsMigration

**Durum:** 🔜 v1.1
**Etki:** Düşük

`SettingsService` + `SettingsStore` + `SettingsValidator` yapısı sağlam. Versiyon ilerledikçe kaydedilmiş ayarların şeması değişebilir. Bu yüzden:

- `SettingsMigration` (versiyon → versiyon yükseltme zinciri)
- Ayarlara `schemaVersion` alanı
- Varsayılanlarla doldurma, bilinmeyen anahtarları koruma

## 3. LoggerService → Services/Logging/

**Durum:** 🔜 v1.1
**Etki:** Düşük

Logging başlı başına bir modül (Issue #24: v1.0 Basic File+Rolling, v1.1 Structured Levels/Export/Filters).
`Services/Core/LoggerService.cs` → `Services/Logging/` altına taşınır; namespace + DI güncellenir.

## 4. GithubService → Services/Network/

**Durum:** 🔜 v1.1
**Etki:** Düşük

`GithubService` aslında bir network servisi, Core genel servisleriyle ilgisi yok.
`Services/Core/GithubService.cs` → `Services/Network/` altına taşınır.
(İleride Proxy, Mirror, Network Diagnostics gibi network servisleri de aynı klasöre toplanır.)
