# WinGetX Contributor Guide

> Anayasa (AGENTS.md) ve `docs/architecture.md` bu rehberin temelidir.
> Yeni modül yazmadan önce: bir Issue seç, kodunu yaz (Çalışma Şekli).

## Kod Standartları

- Dil: C# (.NET 10), UI: Avalonia UI 12.x, MVVM: CommunityToolkit.Mvvm.
- Her dosya copyright/attribution başlığı taşır (ARTICLE XVI).
- Namespace kökü `WinGetXAvalonia.*`; katman adı namespace'e yansır.
- Yorum yazılmaz; kod kendini anlatır.
- Warning ve Exception'sız derleme hedefi: `dotnet build WinGetX.sln` → 0 Uyarı / 0 Hata.

## Katman Kuralları

```
Presentation (Views, Controls, XAML)  — yalnızca ViewModel'e bağlanır
    ↓
Application (ViewModels, Services)    — UI State, Command, Service çağrısı
    ↓
Core (iş mantığı, domain, provider)   — UI bilmez
    ↓
Infrastructure (Winget, Disk, Network) — provider uygulamaları
```

- Ters bağımlılık yok (ARTICLE I).
- ViewModel iş kuralları içermez; Service çağırır (ARTICLE II).
- Service bir iş yapar, birbirine minimum bağımlı (ARTICLE III).

## Yeni Modül Nasıl Yazılır

1. **Model** — `Models/` içinde veri modeli (UI State değil, veri).
2. **Service** — `Services/` içinde arayüz + uygulama. İşi service yapar.
   - Network, Disk, Winget, Process her zaman async (ARTICLE VI).
3. **ViewModel** — `ViewModels/` içinde UI State + Command.
   - `AsyncViewModel`'den türer; `InitializeAsync()` içinde yükler.
   - Her ekran Loading / Empty / Error / Success durumlarına sahiptir (ARTICLE VIII).
4. **View** — `Views/` içinde XAML. Sabit metin yazılmaz (ARTICLE XI).
5. **DI** — `Program.cs` içinde `AddSingleton` ile kayıt.
6. **Navigasyon** — `MainViewModel` menu + `_pages`, `CommandPaletteViewModel.PageEntries`.
7. **Test** — `tests/WinGetXAvalonia.Tests/` içinde xUnit.

## ViewModel Sözleşmesi

- UI State → `[ObservableProperty]` alanları (CommunityToolkit).
- Komutlar → `[RelayCommand]` ile `XxxCommand` olarak üretilir; testler
  `await XxxCommand.ExecuteAsync(arg)` çağırır.
- Durumlar → `IsLoading`, `IsLoaded`, `HasError`, `ErrorMessage` (AsyncViewModel).
- Veri koleksiyonları → `ObservableCollection<T>`; arama filtreleri ayrı bir
  hesaplanan liste (`FilteredComputers` gibi) olarak beslenir.

## Hata Zinciri

Her hata şu zincirden geçer (ARTICLE VII):

```
Hata → Log (LoggerService) → History (IHistoryService) → Notification (INotificationService) → UI
```

`catch` bloklarında kullanıcıya gösterim `_notifications.ShowError(...)` /
`_dialogs.ShowErrorAsync(...)` ile yapılır; exception UI thread'e taşmaz.

## Test Yaklaşımı

- Framework: xUnit.
- Her ViewModel için Fake service'ler (FakeNotificationService, FakeHistoryService,
  FakeDialogService vb.) `ExecuteAsync(null)` ile komut test edilir.
- Service'ler doğrudan test edilir; disk/network bağımlılıkları geçici klasörle
  izole edilir.
- Hedef: `dotnet test WinGetX.sln` tamamen yeşil.
- Kural: `IStorageFile` gibi user-kodla implement edilemeyen arayüzler testlerde
  taklit edilmez; bunun yerine logic bir service'e (örn. `FleetReportWriter`)
  çekilip dosya sistemi üzerinden test edilir.

## Yerelleştirme

- Hiçbir metin View içinde sabit yazılmaz (ARTICLE XI).
- Kullanıcıya gösterilen metinler (notifications, dialogs) Localization
  kaynaklarından gelir.

## Tema

- Renk, Font, Radius, Spacing, Shadow, Animation tek tema sisteminden gelir
  (ARTICLE XII). `docs/design-system.md`'deki token'lardan başka değer kullanılmaz.

## Özellik Bağımlılığı

Yeni özellik eklemeden önce Altın Kural (docs/vision.md) sorularından geçilir:

1. Gerçek bir kullanıcı sorununu çözüyor mu?
2. Windows'un mevcut araçlarına göre belirgin değer katıyor mu?
3. Diğer modüllerle doğal entegre oluyor mu?
4. Beş yıl sonra anlamlı olacak mı?

## Definition of Done

Bir Issue ancak şu maddelerin tamamı sağlanıyorsa kapanır:

- Kod derleniyor (0 warning / 0 error).
- Exception fırlatmıyor.
- Dark + Light tema düzgün.
- Localization hazır.
- Loading, Empty, Error durumları var.
- MVVM uygun, Service kullanıyor.
- Log + History + Notification çalışıyor.
- Testler yeşil.
- Commit atıldı (bir commit = bir sorun = bir çözüm).
- Review tamamlandı.

## Branch Stratejisi

- Ana dal `main`; geliştirme `develop` üzerinde.
- Özellik dalları: `feature/<isim>`.
- Founder: `founder/<modül>` (ayrı ürün, ARTICLE XIII).

## Geçiş Yolu

Tek projeden çoklu projeye geçiş (docs/architecture.md) her adımda derlenir ve
testler yeşil kalır: Core → UI → Features → Founder.
