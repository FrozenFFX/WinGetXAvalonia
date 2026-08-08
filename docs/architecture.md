# WinGetX Mimari

> Anayasa ARTICLE I–XIII mimarinin temelidir. Bu dosya mevcut durumu ve hedef yapıyı tanımlar.
> Kural: Katmanlar ters bağımlılık oluşturamaz (ARTICLE I).

## Katmanlar

```
Presentation
    ↓
Application
    ↓
Core
    ↓
Infrastructure
```

- **Presentation** — View, Control, XAML. Sadece ViewModel'i bağlar.
- **Application** — ViewModel, Service. UI State yönetir, Command çalıştırır, Service çağırır.
- **Core** — İş mantığı. UI bilmez, Avalonia bilmez, ViewModel bilmez.
- **Infrastructure** — Winget, Disk, Network. Provider'lar.

## Mevcut Durum (Tek Proje)

Şu an her şey `WinGetXAvalonia/` tek csproj'unda. Klasörler katmanları yansıtır:

| Klasör | Katman | Sorumluluk |
|--------|--------|------------|
| `Views`, `Controls`, `Dialogs` | Presentation | XAML, UI bileşenleri |
| `ViewModels` | Application | UI State, Command |
| `Services` | Application | Uygulama servisleri |
| `Models` | Data | Veri modelleri |
| `Core` | Core | İş mantığı, domain, provider arayüzleri |
| `Infrastructure` | Infrastructure | Provider uygulamaları, Winget |
| `Helpers`, `Collections`, `Converters` | Ortak | Yardımcılar |

## Hedef Yapı (Çoklu Proje)

```
WinGetX/
├── WinGetX.Core/        İş mantığı, domain, provider arayüzleri
├── WinGetX.UI/          Avalonia presentasyonu (Views, Controls, ViewModels, Themes)
├── WinGetX.Features/    Özellik modülleri (Collections, Workspaces, Automation, ...)
└── WinGetX.Founder/     Founder Edition modülleri (ayrı ürün)
    ├── FounderCenter/
    ├── ThemeStudio/
    ├── GameMode/
    ├── FounderLab/
    ├── PerformanceOverlay/
    └── DeveloperConsole/
```

## Bağımlılık Kuralları

```
Core ← WinGetX.UI
Core ← WinGetX.Features
WinGetX.UI ← WinGetX.Features
```

- `WinGetX.Core` dışındaki hiçbir proje bir üstteki projeye bağımlı olamaz.
- UI ve Features, Core'a bağımlı olabilir; tersi olamaz.
- Her proje kendi namespace'inde yaşar; `using` ile katman sınırı aşılmaz.

## Founder

Founder Edition ana projeden tamamen ayrıdır (ARTICLE XIII):

- Founder kodu `WinGetX.Founder/` içinde yaşar, ana projeye karışmaz.
- Açık kaynak sürüm (GitHub) Founder olmadan da eksiksiz çalışır.
- Ortak iş mantığı ihtiyacı doğarsa paylaşılan çekirdek kararı `docs/founder-roadmap.md`'de değerlendirilir.

## Event Sözleşmeleri (Event Contract Catalog)

Event Engine tektir; her olay bir kez publish edilir, çok modül dinler
(guardrails.md — Omurga). Sözleşme kataloğu (hedef):

| # | Olay | Anlamı |
|---|------|--------|
| 1 | `AppStarted` | Uygulama açıldı; oturum başladı. |
| 2 | `PackageChanged` | Paket kuruldu / güncellendi / kaldırıldı. |
| 3 | `EnvironmentChanged` | Environment durumu değişti. |
| 4 | `AutomationTriggered` | Otomasyon tetiklendi. |
| 5 | `TaskStatusChanged` | Bir Task başladı / ilerledi / bitti. |
| 6 | `SnapshotCreated` | Yeni snapshot alındı. |
| 7 | `HistoryRecorded` | Yeni history kaydı oluştu. |
| 8 | `SourceChanged` | Kaynak (winget, custom) eklendi / silindi / güncellendi. |
| 9 | `SettingsChanged` | Ayar değişti; ilgili state tazelenir. |
| 10 | `ThemeChanged` | Tema değişti; UI beslenir. |
| 11 | `NotificationRaised` | Bildirim yayınlandı. |
| 12 | `SecurityVerdictChanged` | Güven kararı (trust) değişti. |
| 13 | `IssueDetected` | Diagnostics yeni bir sorun buldu. |
| 14 | `BlueprintImported` | Blueprint içe aktarıldı. |
| 15 | `WorkspaceOpened` | Workspace açıldı; state yüklendi. |
| 16 | `DiscoverRefreshed` | Discover içeriği tazelendi. |
| 17 | `SessionEnded` | Oturum kapandı; özet işlenir. |

Mevcut kodda `AppEvent` + `PackageChangedEvent` vardır; katalog hedeftir.
Yeni olay eklenmeden önce kataloğa girilir — kopuk olay, kopuk modül demektir.

## Geçiş

Tek projeden çoklu projeye geçiş adım adım yapılır, her adımda kod derlenir ve testler yeşil kalır:

1. `WinGetX.Core` projesi → `Core/`, `Infrastructure/` taşınır.
2. `WinGetX.UI` projesi → `Views/`, `Controls/`, `ViewModels/`, `Themes/` taşınır.
3. `WinGetX.Features` projesi → özellik modülleri ayrılır.
4. `WinGetX.Founder` projesi → Founder modülleri eklenir.

İlgili notlar: `docs/technical-debt.md`, `docs/backlog-ideas.md`.
