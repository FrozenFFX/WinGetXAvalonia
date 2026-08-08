# WinGetX Design System

> ARTICLE XII: Renk, Font, Radius, Spacing, Shadow, Animation tek tema sisteminden gelir.
> Hiçbir View sabit değer içermez — sadece tema token'larını kullanır.

## Tema Dosyaları

Tüm token'lar `Themes/` klasöründedir:

| Dosya | İçerik |
|-------|--------|
| `Colors.axaml` | Renk token'ları |
| `Typography.axaml` | Font ölçekleri |
| `Spacing.axaml` | Boşluk ölçekleri |
| `Radius.axaml` | Köşe yarıçapı |
| `Elevation.axaml` | Gölge katmanları |
| `Animation.axaml` | Motion token'ları (süre, easing) |
| `Motion.axaml` | Kanonik motion stilleri (card, panel, button, textbox, list) |
| `Light.axaml` / `Dark.axaml` | Tema özel yüzey renkleri |
| `Controls.axaml` | Bileşen stilleri |

## Renk Token'ları

Temel semantic renkler `Colors.axaml`:

| Token | Değer | Kullanım |
|-------|-------|----------|
| `Primary` / `PrimaryBrush` | `#3E8EF7` | Birincil aksiyon |
| `PrimaryHover` | `#2F7CE8` | Birincil hover |
| `AccentBrush` | `#3B82F6` | Vurgu, seçim, fokus |
| `Danger` / `DangerBrush` | `#F44336` | Hata, silme, risk |
| `Success` / `SuccessBrush` | `#4CAF50` | Başarı, sağlık |
| `Warning` / `WarningBrush` | `#FFB300` | Uyarı, pending |
| `Info` / `InfoBrush` | `#2196F3` | Bilgi, durum |

Yüzey renkleri (arka plan, kart, kenarlık, metin) yalnızca `Light.axaml` ve
`Dark.axaml` içinden `DynamicResource` ile çözülür; View'larda sabit renk yazılmaz.

## Tipografi

`Typography.axaml` font ölçekleri (Segoe UI Variable):

| Token | Boyut | Kullanım |
|-------|-------|----------|
| `FontHero` / `FontDisplay` | 32 | Hero, dashboard değerleri |
| `FontPageTitle` / `FontTitle` | 26 | Sayfa başlıkları |
| `FontSection` / `FontSubtitle` | 20 | Bölüm başlıkları |
| `FontCardTitle` | 16 | Kart başlıkları |
| `FontBody` | 14 | Gövde metni (varsayılan) |
| `FontCaption` | 13 | Açıklama, alt bilgi |
| `FontSecondary` | 11 | İkincil etiketler |

## Spacing

`Spacing.axaml` ölçekleri: `Space4` → `Space8` → `Space12` → `Space16` →
`Space20` → `Space24` → `Space32` → `Space40` → `Space48`. Tutarlılık için
View'larda yalnızca bu token'lar kullanılır; serbest değer yazılmaz.

## Radius

`Radius.axaml` ölçekleri: `RadiusXS` (6) → `RadiusSmall` (8) → `RadiusMedium`
(12) → `RadiusCard` (16) → `RadiusLarge` (18) → `RadiusXL` (24). Kartlar
`RadiusCard` veya `RadiusLarge` kullanır; butonlar `RadiusSmall`/`RadiusMedium`.

## Elevation

`Elevation.axaml` katmanları:

- `Elevation0` — düz yüzey
- `Elevation1` — hafif yükselti (hover'da kart)
- `Elevation2` — belirgin yükselti (kart :pointerover, drawer)

## Animasyon (Motion System)

Tek kaynak: `Themes/Animation.axaml` (token) + `Themes/Motion.axaml` (stiller).

Süre token'ları:

| Token | Süre | Kullanım |
|-------|------|----------|
| `MotionDurationInstant` | 80ms | anlık mikro geri bildirim |
| `MotionDurationFast` | 100ms | Button hover |
| `MotionDurationNormal` | 150ms | Card hover, ListBoxItem |
| `MotionDurationSlow` | 200ms | TextBox fokus, Sidebar |
| `MotionDurationEntrance` | 250ms | giriş/sayfa geçişleri |

Tüm geçişler `MotionEasingOut` (CubicEaseOut) kullanır. View'larda sabit süre
yazılmaz.

Kanonik davranışlar (`Motion.axaml`):

| Bileşen | Davranış |
|---------|----------|
| `Border.card` | hover: scale 1.02 + yukarı kayma + arka plan açılır + border parlar + shadow büyür (150ms); pressed: scale 0.99 |
| `Border.panel` | statik yüzey; hover yalnızca arka plan/border/shadow (200ms) |
| `Button` | hover: arka plan/border/opaklık (100ms) |
| `TextBox` | fokus outline yumuşak gelir (200ms) |
| `ListBoxItem` | seçim/hover vurgusu (150ms) |
| Sidebar | seçim glow + hover (200ms) |

Kart dili tek sınıftır: etkileşimli kartlar `card`, statik yüzeyler `panel`.
Hiçbir bileşen kendi animasyon değerini tanımlamaz.

## Bileşen Envanteri

`Controls/` altında yeniden kullanılabilir bileşenler:

### Shared (`Controls/Shared/`)

- `AppButton` — Button; primary/fluent varyantları
- `AppCard` — kart yüzeyi
- `AppTextField` — giriş alanı
- `AppBadge`, `AppIcon`, `AppSeparator`, `AppToolbar`
- `AppDrawer`, `AppDialog`, `AppWindow`
- `AppEmptyState` — Empty State bileşeni
- `AppSection`, `SectionHeader`, `SectionCard`, `PageHeader`
- `LoadingOverlay`, `LoadingState`, `ErrorState`, `EmptyState`
- `MasterDetailPage` — liste/detay düzeni
- `SearchBox`, `SearchBar`, `FilterBar`, `CommandBar`
- `AppNavigation`, `AppSidebarSection`, `AppGrid`, `AppScrollViewer`
- `AppProgress`, `Chip`, `Badge`
- `Layout/Toolbar`, `Layout/SplitView`, `Layout/PropertyRow`, `Layout/PropertyGrid`
- `Inputs/` — giriş bileşenleri

### Cards

- `StatCard` — ikon + büyük değer + alt başlık (`cards:StatCard`)
- `InfoCard`, `ActionCard`, `MetricCard`, `AppCard`
- `Controls/Cards/Card` (`Border.card` sınıfı)

### Domain bileşenleri

- `HistoryCard`, `PackageCard`, `PackageRow`, `BrowseCard`, `PackageDetailsPanel`
- `QueuePanel`, `SearchBar`, `NotificationBell`, `TopBar`, `Sidebar`, `CommandPalette`
- `Workspace/*`, `Studio/*`, `Operation/*`, `Layout/*`

## Durum Kuralları

Her ekran Loading, Empty, Error, Success durumlarına sahiptir (ARTICLE VIII):

- **Loading** → `LoadingOverlay` / `LoadingState`
- **Empty** → `AppEmptyState` / `EmptyState`
- **Error** → `ErrorState` + Hata zinciri (Log → History → Notification → UI)
- **Success** → içerik + `NotificationService`

## Yeni Bileşen Kuralı

1. İki kez kullanılacaksa `Controls/Shared/` altına genel bileşen yaz (ARTICLE X).
2. Bileşen tema token'larından başka hiçbir sabit değer içeremez.
3. Yerelleştirme: View içinde metin yazılmaz (ARTICLE XI).
4. Bağımlılık: bileşen yalnızca ViewModel'e bağlanır; iş mantığı içermez.
5. Bileşen klasör adı + `.axaml` + `.axaml.cs` birlikte yaşar.

## Erişilebilirlik

- Metinlerde yeterli kontrast (Dark/Light ayrı token'lar).
- Hover/pressed/focus durumları her etkileşimli bileşende tanımlı olmalı.
- Görsel durumlar metin/ikon ile desteklenir (yalnızca renge güvenilmez).
