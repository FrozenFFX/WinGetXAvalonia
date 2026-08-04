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

## Geçiş

Tek projeden çoklu projeye geçiş adım adım yapılır, her adımda kod derlenir ve testler yeşil kalır:

1. `WinGetX.Core` projesi → `Core/`, `Infrastructure/` taşınır.
2. `WinGetX.UI` projesi → `Views/`, `Controls/`, `ViewModels/`, `Themes/` taşınır.
3. `WinGetX.Features` projesi → özellik modülleri ayrılır.
4. `WinGetX.Founder` projesi → Founder modülleri eklenir.

İlgili notlar: `docs/technical-debt.md`, `docs/backlog-ideas.md`.
