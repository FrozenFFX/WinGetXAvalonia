# WinGetX Coding Constitution

Version: 1.0

==================================================

## ARTICLE I
### ARCHITECTURE

WinGetX katmanlarından oluşur:

```
Presentation
    ↓
Application
    ↓
Core
    ↓
Infrastructure
```

Katmanlar ters bağımlılık oluşturamaz.

==================================================

## ARTICLE II
### VIEWMODELS

ViewModel:
- UI State yönetir.
- Command çalıştırır.
- Service çağırır.

İş kuralları içermez.

==================================================

## ARTICLE III
### SERVICES

Service bir işi yapar.

Örnekler:
- ClipboardService
- HistoryService
- OperationService
- ThemeService
- SettingsService
- NotificationService
- ...

Service'ler birbirine minimum bağımlı olur.

==================================================

## ARTICLE IV
### DOMAIN

```
Business Logic
    ↓
Domain
```

Domain:
- UI bilmez.
- Avalonia bilmez.
- ViewModel bilmez.
- Sadece iş kurallarını bilir.

==================================================

## ARTICLE V
### MODELS

```
Model
    ↓
Data
```

```
UI State
    ↓
ViewModel
```

Karıştırılmaz.

==================================================

## ARTICLE VI
### ASYNC

Network, Disk, Winget, Process işlemleri her zaman async çalışır.

UI Thread bloklanmaz.

==================================================

## ARTICLE VII
### ERROR HANDLING

Her hata şu zincirden geçer:

```
Hata
    ↓
Log
    ↓
History
    ↓
Notification
    ↓
UI
```

==================================================

## ARTICLE VIII
### UI

Her ekran Loading, Empty, Error, Success durumlarına sahip olur.

==================================================

## ARTICLE IX
### PERFORMANCE

Aynı veri iki kez yüklenmez. Cache kullanılır.

==================================================

## ARTICLE X
### REUSE

İki kez yazılan kod refactor edilir.

==================================================

## ARTICLE XI
### LOCALIZATION

Hiçbir metin View içinde sabit yazılmaz. Localization kullanılmalıdır.

==================================================

## ARTICLE XII
### THEME

Renk, Font, Radius, Spacing, Shadow, Animation tek tema sisteminden gelir.

==================================================

## ARTICLE XIII
### FOUNDER

Founder ayrı üründür. GitHub sürümüne bağımlılık oluşturmaz.

==================================================

## ARTICLE XIV
### COMMITS

```
Bir Commit
    ↓
Bir Sorun
    ↓
Bir Çözüm
```

==================================================

## ARTICLE XV
### RELEASES

```
Feature Freeze
    ↓
Bug Fix
    ↓
RC
    ↓
Stable
```

Süreç değişmez.

==================================================

# 📋 WinGetX Definition of Done

Bir Issue ancak aşağıdakilerin tamamı sağlanıyorsa kapanır:

- [ ] Kod derleniyor.
- [ ] Warning yok.
- [ ] Exception fırlatmıyor.
- [ ] UI düzgün.
- [ ] Dark Theme düzgün.
- [ ] Light Theme düzgün.
- [ ] Localization hazır.
- [ ] Loading var.
- [ ] Empty State var.
- [ ] Error State var.
- [ ] MVVM uygun.
- [ ] Service kullanıyor.
- [ ] Log eklenmiş.
- [ ] History çalışıyor.
- [ ] Notification çalışıyor.
- [ ] Commit atıldı.
- [ ] Review tamamlandı.

# 📦 Branch Stratejisi

```
main
│
├── release/v1.0
│
├── develop
│
├── feature/dashboard
├── feature/installed
├── feature/browse
├── feature/updates
├── feature/history
├── feature/settings
│
└── founder
      ├── founder/theme-engine
      ├── founder/game-mode
      ├── founder/widgets
      └── founder/lab
```

# 📁 Klasör Kuralları

| Klasör | İçerik |
|--------|--------|
| `Core` | Sadece iş mantığı |
| `Services` | Uygulama servisleri |
| `Models` | Veri |
| `ViewModels` | UI State |
| `Views` | XAML |
| `Controls` | Yeniden kullanılabilir bileşenler |
| `Resources` | Brush, Styles, Icons |
| `Themes` | Tema dosyaları |
| `Assets` | Resimler, Fontlar |
| `Localization` | Dil dosyaları |
| `Founder` | Sadece Founder Edition |

# 🏗 Proje Yapısı (Hedef)

Founder özellikleri ana projeye karışmaz. Hedef çoklu proje düzeni:

```
WinGetX/
├── WinGetX.Core/        İş mantığı, domain, provider arayüzleri
├── WinGetX.UI/          Avalonia presentasyonu (Views, Controls, ViewModels, Themes)
├── WinGetX.Features/    Özellik modülleri
└── WinGetX.Founder/     Founder Edition (ayrı ürün, ana projeye karışmaz)
    ├── FounderCenter/
    ├── ThemeStudio/
    ├── GameMode/
    ├── FounderLab/
    ├── PerformanceOverlay/
    └── DeveloperConsole/
```

Detay: `docs/architecture.md`. Founder vizyon: `docs/founder-vision.md`, `docs/founder-roadmap.md`.

# 🏆 Nihai Hedef

```
WinGetX
    ↓
Open Source
    ↓
Production Ready
    ↓
Maintainable
    ↓
Extensible
    ↓
Fast
    ↓
Beautiful
    ↓
Reliable
    ↓
Founder Edition
```

# 🎯 Çalışma Şekli

Plan dondurulmuştur. Her oturum tek bir hedefe sahiptir:

1. Bir Issue seç.
2. Kodunu yaz.
