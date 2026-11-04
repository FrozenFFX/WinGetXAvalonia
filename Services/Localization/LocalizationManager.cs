// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Threading;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Services.Localization;

public sealed class LocalizationManager : INotifyPropertyChanged
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
            ["Choose the interface language."] = "Arayüz dilini seçin.",
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

    private static readonly IReadOnlyDictionary<string, string> AdditionalTurkish =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["64-bit OS"] = "64 bit işletim sistemi",
            ["A real security pipeline needs Publisher, Signature, Hash, Source and Verification steps. None of these are wired up yet, so showing fake Trust or Health percentages would be misleading."] = "Yayıncı, imza, karma, kaynak ve doğrulama adımlarını içeren gerçek bir güvenlik altyapısı henüz bağlı değil. Bu nedenle sahte güven veya sağlık yüzdeleri göstermek yanıltıcı olur.",
            ["About"] = "Hakkında",
            ["Description"] = "Açıklama",
            ["Developer mode"] = "Geliştirici modu",
            ["CPU"] = "İşlemci",
            ["CPU Time"] = "İşlemci süresi",
            ["Disk (C:)"] = "Sürücü (C:)",
            ["Everything is working normally."] = "Her şey yolunda.",
            ["Critical Updates"] = "Kritik güncellemeler",
            ["Failed Installations"] = "Başarısız yüklemeler",
            ["GPU"] = "Grafik işlemci",
            ["Handles"] = "İşleyiciler",
            ["Loading automations..."] = "Otomasyonlar yükleniyor...",
            ["Loading dashboard..."] = "Pano yükleniyor...",
            ["Loading developer tools..."] = "Geliştirici araçları yükleniyor...",
            ["Loading enterprise data..."] = "Kurumsal veriler yükleniyor...",
            ["Loading fleet..."] = "Cihaz filosu yükleniyor...",
            ["Loading favorites..."] = "Favoriler yükleniyor...",
            ["Loading history..."] = "Geçmiş yükleniyor...",
            ["Loading identities..."] = "Yazılım kimlikleri yükleniyor...",
            ["Loading installed packages..."] = "Yüklü paketler yükleniyor...",
            ["Loading marketplace..."] = "Mağaza yükleniyor...",
            ["Loading package details..."] = "Paket ayrıntıları yükleniyor...",
            ["Loading settings..."] = "Ayarlar yükleniyor...",
            ["Loading sources..."] = "Kaynaklar yükleniyor...",
            ["Loading workspaces..."] = "Çalışma alanları yükleniyor...",
            ["Checking for package updates..."] = "Paket güncellemeleri denetleniyor...",
            ["Memory"] = "Bellek",
            ["Name"] = "Ad",
            ["No license has been assigned to this repository."] = "Bu depo için henüz bir lisans belirlenmedi.",
            ["Online"] = "Çevrimiçi",
            ["Open navigation menu"] = "Gezinme menüsünü aç",
            ["Package"] = "Paket",
            ["Parallel downloads"] = "Eşzamanlı indirme sayısı",
            ["Pending updates"] = "Bekleyen güncellemeler",
            ["pending updates"] = "bekleyen güncellemeler",
            [" pending updates"] = " bekleyen güncelleme",
            ["Read from Windows"] = "Windows'tan oku",
            ["Record Rollback"] = "Geri alma kaydı oluştur",
            ["Record Update"] = "Güncelleme kaydı oluştur",
            ["Refresh"] = "Yenile",
            ["Refresh history (F5)"] = "Geçmişi yenile (F5)",
            ["Remote Installation"] = "Uzaktan yükleme",
            ["Remove"] = "Kaldır",
            ["Rendering"] = "Görüntüleme",
            ["Report Issue"] = "Sorun bildir",
            ["Reset"] = "Sıfırla",
            ["Reset Settings"] = "Ayarları sıfırla",
            ["Resources"] = "Kaynaklar",
            ["Restart"] = "Yeniden başlat",
            ["Results"] = "Sonuçlar",
            ["Rounded corners"] = "Yuvarlatılmış köşeler",
            ["Running"] = "Çalışıyor",
            ["Runtime"] = "Çalışma zamanı",
            ["Scanning gaming setup..."] = "Oyun sistemi taranıyor...",
            ["Search"] = "Ara",
            ["Search packages"] = "Paketlerde ara",
            ["Searching packages..."] = "Paketler aranıyor...",
            ["Security status and package verification."] = "Güvenlik durumu ve paket doğrulaması.",
            ["security"] = "güvenlik",
            ["Send PowerShell"] = "PowerShell komutu gönder",
            ["Send Script"] = "Betik gönder",
            ["Software Identity"] = "Yazılım kimliği",
            ["Something went wrong"] = "Bir sorun oluştu",
            ["Successful"] = "Başarılı",
            ["Theme"] = "Tema",
            ["Threads"] = "İş parçacıkları",
            ["Today"] = "Bugün",
            ["Total"] = "Toplam",
            ["Trigger"] = "Tetikleyici",
            ["update"] = "güncelle",
            ["Update"] = "Güncelle",
            ["User"] = "Kullanıcı",
            ["Verbose logs"] = "Ayrıntılı günlükler",
            ["Verified"] = "Doğrulandı",
            ["Window"] = "Pencere",
            ["windows"] = "Windows",
            ["workspace This Computer"] = "Çalışma alanı: Bu bilgisayar",
            ["Workspaces"] = "Çalışma alanları",
            ["1 item"] = "1 öğe",
            ["1 operation"] = "1 işlem",
            ["1 package found"] = "1 paket bulundu",
            ["1 workspace"] = "1 çalışma alanı",
            ["Accent color and theme presets"] = "Vurgu rengi ve tema seçenekleri",
            ["Dark"] = "Koyu",
            ["Light"] = "Açık",
            ["System"] = "Sistem",
            ["Blue"] = "Mavi",
            ["Green"] = "Yeşil",
            ["Orange"] = "Turuncu",
            ["Purple"] = "Mor",
            ["Red"] = "Kırmızı",
            ["Pink"] = "Pembe",
            ["Teal"] = "Turkuaz",
            ["Lime"] = "Açık yeşil",
            ["Slate"] = "Gri-mavi",
            ["Gray"] = "Gri",
            ["White"] = "Beyaz",
            ["Black"] = "Siyah",
            ["Notify me when Chrome updates"] = "Chrome güncellendiğinde bana bildir",
            ["Refresh Pip after Python installs"] = "Python yüklendikten sonra Pip'i yenile",
            ["Take a snapshot before every update"] = "Her güncellemeden önce anlık görüntü al",
            ["Find and install apps in one click"] = "Uygulamaları tek tıkla bulup yükleyin",
            ["Categories help you discover new tools"] = "Kategoriler yeni araçları keşfetmenize yardımcı olur",
            ["Results are enriched with update status"] = "Sonuçlarda güncelleme durumu da gösterilir",
            ["Check the spelling of the package name"] = "Paket adının yazımını denetleyin",
            ["Try a shorter or more common name"] = "Daha kısa veya yaygın bir ad deneyin",
            ["Browse a category instead"] = "Bunun yerine bir kategoriye göz atın",
            ["Recreate your setup on a new PC"] = "Kurulumunuzu yeni bir bilgisayarda yeniden oluşturun",
            ["Share a standard dev environment"] = "Standart bir geliştirme ortamını paylaşın",
            ["Reinstall apps after a format"] = "Biçimlendirme sonrasında uygulamaları yeniden yükleyin",
            ["Installs, updates and removals in one place"] = "Yükleme, güncelleme ve kaldırma işlemleri tek yerde",
            ["Roll back to any snapshot in two clicks"] = "Herhangi bir anlık görüntüye iki tıkla geri dönün",
            ["Compare what changed between two points"] = "İki tarih arasındaki değişiklikleri karşılaştırın",
            ["Group related apps into one workspace"] = "İlgili uygulamaları tek çalışma alanında toplayın",
            ["Export and share your setup"] = "Kurulumunuzu dışa aktarın ve paylaşın",
            ["Install a whole workspace at once"] = "Bir çalışma alanındaki tüm uygulamaları birlikte yükleyin",
            ["Notifications"] = "Bildirimler",
            ["No notifications yet."] = "Henüz bildirim yok.",
            ["Enter"] = "Enter",
            ["Up/Down Navigate"] = "Yukarı/Aşağı gezin",
            ["Enter Execute"] = "Enter çalıştır",
            ["Esc Close"] = "Esc kapat",
            ["Ctrl+K Close"] = "Ctrl+K kapat",
            ["Selected"] = "Seçildi",
            ["Open Homepage"] = "Ana sayfayı aç",
            ["Copy ID"] = "Kimliği kopyala",
            ["Copy Name"] = "Adı kopyala",
            ["Operations"] = "İşlemler",
            ["No operations yet."] = "Henüz işlem yok.",
            ["Version"] = "Sürüm",
            ["Clear Completed"] = "Tamamlananları temizle",
            ["Remove completed operations"] = "Tamamlanan işlemleri kaldır",
            ["Installed"] = "Yüklü",
            ["Available"] = "Kullanılabilir",
            ["Source Trust"] = "Kaynak güveni",
            ["Update Frequency"] = "Güncelleme sıklığı",
            ["Latest Version"] = "En son sürüm",
            ["Publisher"] = "Yayıncı",
            ["License"] = "Lisans",
            ["Release Notes"] = "Sürüm notları",
            ["Recommended"] = "Önerilen",
            ["Search workspaces..."] = "Çalışma alanlarında ara...",
            ["Refresh workspaces (F5)"] = "Çalışma alanlarını yenile (F5)",
            ["Check installed packages"] = "Yüklü paketleri denetle",
            ["Remove from workspace"] = "Çalışma alanından kaldır",
            ["No packages yet"] = "Henüz paket yok",
            ["Add packages to turn this workspace into a one-click setup."] = "Tek tıkla kurulum için paketleri bu çalışma alanına ekleyin.",
            ["Workspace name"] = "Çalışma alanı adı",
            ["Workspace description (optional)"] = "Çalışma alanı açıklaması (isteğe bağlı)",
            ["Open"] = "Aç",
            ["Edit"] = "Düzenle",
            ["More actions"] = "Daha fazla işlem",
            [" Packages"] = " Paketler",
            ["+ Add Package"] = "+ Paket ekle",
            ["+ New Workspace"] = "+ Yeni çalışma alanı",
            ["Refresh (F5)"] = "Yenile (F5)",
            ["Please wait..."] = "Lütfen bekleyin...",
            ["Create"] = "Oluştur",
            ["Error"] = "Hata",
            ["Good Afternoon"] = "İyi günler",
            ["Good Evening"] = "İyi akşamlar",
            ["Good Morning"] = "Günaydın",
            ["Loading"] = "Yükleniyor",
            ["Connecting"] = "Bağlanıyor",
            ["Offline"] = "Çevrimdışı",
            ["Waiting"] = "Bekliyor",
            ["Success"] = "Başarılı",
            ["Unknown"] = "Bilinmiyor",
            ["N/A"] = "Yok",
            ["Install All ({0})"] = "Tümünü yükle ({0})",
            ["Install All ({pending})"] = "Tümünü yükle ({pending})",
            ["New Workspace"] = "Yeni çalışma alanı",
            ["Edit Workspace"] = "Çalışma alanını düzenle",
            ["New Automation"] = "Yeni otomasyon",
            ["New Blueprint"] = "Yeni plan",
            ["No items"] = "Öğe yok",
            ["No packages found"] = "Paket bulunamadı",
            ["1 package"] = "1 paket",
            ["Try a different package name or category."] = "Başka bir paket adı deneyin veya bir kategori seçin.",
            ["No results for \"{LastQuery}\""] = "\"{LastQuery}\" için sonuç bulunamadı",
            ["No results for \"{0}\""] = "\"{0}\" için sonuç bulunamadı",
            ["Ready"] = "Hazır",
            ["Working"] = "Çalışıyor",
            ["Pending"] = "Bekliyor",
            ["Completed"] = "Tamamlandı",
            ["Failed"] = "Başarısız",
            ["Cancelled"] = "İptal edildi",
            ["Install {argument}"] = "{argument} paketini yükle",
            ["Install {query}"] = "{query} paketini yükle",
            ["Uninstall {argument}"] = "{argument} paketini kaldır",
            ["Uninstall {query}"] = "{query} paketini kaldır",
            ["Update {argument}"] = "{argument} paketini güncelle",
            ["Update {query}"] = "{query} paketini güncelle",
            ["Search for {argument}"] = "{argument} için ara",
            ["Search for {query}"] = "{query} için ara",
            ["'{item.Name}' is now ready to use."] = "'{item.Name}' artık kullanıma hazır.",
            ["'{payload}' completed on {computer.Name}."] = "'{payload}' işlemi {computer.Name} bilgisayarında tamamlandı.",
            ["{computer.Name} is offline; command queued."] = "{computer.Name} çevrimdışı; komut kuyruğa alındı.",
            ["{identity.Name} health decreased."] = "{identity.Name} sağlık puanı düştü.",
            ["{identity.Name} profile deleted."] = "{identity.Name} profili silindi.",
            ["{identity.Name} profile updated."] = "{identity.Name} profili güncellendi.",
            ["Delete '{target.Name}'? This cannot be undone."] = "'{target.Name}' silinsin mi? Bu işlem geri alınamaz.",
            ["Delete policy '{rule.Name}'?"] = "'{rule.Name}' ilkesi silinsin mi?",
            ["No package matched '{item.Parameter}'."] = "'{item.Parameter}' ile eşleşen paket bulunamadı.",
            ["No packages found for '{query}'"] = "'{query}' için paket bulunamadı",
            ["Package not found"] = "Paket bulunamadı",
            ["Package operation"] = "Paket işlemi",
            ["Unknown page"] = "Bilinmeyen sayfa",
            ["Not implemented"] = "Henüz uygulanmadı",
            ["Not supported"] = "Desteklenmiyor",
            ["Loading packages..."] = "Paketler yükleniyor...",
            ["Loading updates..."] = "Güncellemeler yükleniyor...",
            ["Updating packages… {p.Completed} / {p.Total}"] = "Paketler güncelleniyor… {p.Completed} / {p.Total}",
            ["Updating packages… {progress.Completed} / {progress.Total}"] = "Paketler güncelleniyor… {progress.Completed} / {progress.Total}",
            ["Operation History"] = "İşlem geçmişi",
            ["Update All ({pending})"] = "Tümünü güncelle ({pending})",
            ["Install Failed"] = "Yükleme başarısız",
            ["Uninstall Failed"] = "Kaldırma başarısız",
            ["Remote command failed on {computer.Name}"] = "{computer.Name} bilgisayarında uzak komut başarısız oldu",
            ["Identity Removed"] = "Yazılım kimliği kaldırıldı",
            ["Identity Updated"] = "Yazılım kimliği güncellendi",
            ["Identities Exported"] = "Yazılım kimlikleri dışa aktarıldı",
            ["Report Exported"] = "Rapor dışa aktarıldı",
            ["Fleet report saved as CSV."] = "Cihaz filosu raporu CSV olarak kaydedildi.",
            ["Fleet report saved as JSON."] = "Cihaz filosu raporu JSON olarak kaydedildi.",
            ["Export operation history"] = "İşlem geçmişini dışa aktar",
            ["Search packages in Discover"] = "Keşfet'te paket ara",
            ["Find '{query}' in Discover"] = "'{query}' ifadesini Keşfet'te ara",
            ["Find '{argument}' in Discover"] = "'{argument}' ifadesini Keşfet'te ara",
            ["Application Timeline"] = "Uygulama zaman çizelgesi",
            ["Type a package name to install"] = "Yüklenecek paketin adını yazın",
            ["Type a package name to update"] = "Güncellenecek paketin adını yazın",
            ["Type a package name to uninstall"] = "Kaldırılacak paketin adını yazın",
            ["1 tool"] = "1 araç",
            ["Accent Color"] = "Vurgu rengi",
            ["Action"] = "İşlem",
            ["Add"] = "Ekle",
            ["Add a package source to get started."] = "Başlamak için bir paket kaynağı ekleyin.",
            ["Add managed devices to start using Enterprise."] = "Kurumsal özellikleri kullanmak için yönetilen cihaz ekleyin.",
            ["Add Package"] = "Paket ekle",
            ["Add packages to your favorites to see them here."] = "Burada görmek için paketleri favorilerinize ekleyin.",
            ["Alerts"] = "Uyarılar",
            ["All packages are up to date."] = "Tüm paketler güncel.",
            ["Applications installed with WinGet will appear here."] = "WinGet ile yüklenen uygulamalar burada görünür.",
            ["Architecture"] = "Mimari",
            ["Auto backup settings"] = "Ayarları otomatik yedekle",
            ["Auto refresh package lists"] = "Paket listelerini otomatik yenile",
            ["Automation name"] = "Otomasyon adı",
            ["Backup Failed"] = "Yedekleme başarısız",
            ["Blocked"] = "Engellendi",
            ["Blueprint name"] = "Plan adı",
            ["Browse by topic"] = "Konuya göre göz atın",
            ["Browse Packages"] = "Paketlere göz at",
            ["Build Information"] = "Derleme bilgileri",
            ["Cache package lists"] = "Paket listelerini önbelleğe al",
            ["Cache size (MB)"] = "Önbellek boyutu (MB)",
            ["Cancel"] = "İptal",
            ["Categories"] = "Kategoriler",
            ["Check Violations"] = "İhlalleri denetle",
            ["Choose a device to run remote commands."] = "Uzak komut çalıştırmak için bir cihaz seçin.",
            ["Choose a package from the catalog or try one of these categories."] = "Katalogdan bir paket seçin veya şu kategorilerden birini deneyin.",
            ["Choose a workspace from the list to view its packages and details."] = "Paketlerini ve ayrıntılarını görmek için listeden bir çalışma alanı seçin.",
            ["Choose an identity to view its living profile."] = "Güncel profilini görmek için bir yazılım kimliği seçin.",
            ["Clear"] = "Temizle",
            ["Clear History"] = "Geçmişi temizle",
            ["Clear Search"] = "Aramayı temizle",
            ["Close"] = "Kapat",
            ["Collecting diagnostics..."] = "Tanılama bilgileri toplanıyor...",
            ["Command Log"] = "Komut günlüğü",
            ["Commit"] = "Kaydet",
            ["Condition"] = "Koşul",
            ["Configuration"] = "Yapılandırma",
            ["Confirm before install"] = "Yüklemeden önce onay iste",
            ["Confirm before uninstall"] = "Kaldırmadan önce onay iste",
            ["Confirm before update"] = "Güncellemeden önce onay iste",
            ["Confirmations"] = "Onaylar",
            ["Couldn't load automations"] = "Otomasyonlar yüklenemedi",
            ["Couldn't load diagnostics"] = "Tanılama bilgileri yüklenemedi",
            ["Couldn't load favorites"] = "Favoriler yüklenemedi",
            ["Couldn't load game mode"] = "Oyun modu yüklenemedi",
            ["Couldn't load history"] = "Geçmiş yüklenemedi",
            ["Couldn't load installed packages"] = "Yüklü paketler yüklenemedi",
            ["Couldn't load sources"] = "Kaynaklar yüklenemedi",
            ["Couldn't load the fleet"] = "Cihaz filosu yüklenemedi",
            ["Couldn't load the marketplace"] = "Mağaza yüklenemedi",
            ["Couldn't load updates"] = "Güncellemeler yüklenemedi",
            ["Couldn't load workspaces"] = "Çalışma alanları yüklenemedi",
            ["Create automatic package workflows triggered by events."] = "Olaylarla tetiklenen otomatik paket iş akışları oluşturun.",
            ["Create your first automation to get started."] = "Başlamak için ilk otomasyonunuzu oluşturun.",
            ["Create your first blueprint to get started."] = "Başlamak için ilk planınızı oluşturun.",
            ["Create your first company policy."] = "İlk şirket ilkenizi oluşturun.",
            ["Create your first workspace to group package installs."] = "Paket yüklemelerini gruplamak için ilk çalışma alanınızı oluşturun.",
            ["Critical"] = "Kritik",
            ["Ctrl + K"] = "Ctrl + K",
            ["Debugging"] = "Hata ayıklama",
            ["Delete"] = "Sil",
            ["Delete Identity"] = "Kimliği sil",
            ["Deselect All"] = "Tümünün seçimini kaldır",
            ["Developer tools and diagnostics."] = "Geliştirici araçları ve tanılama.",
            ["Developer tools will appear here."] = "Geliştirici araçları burada görünür.",
            ["diagnostics Reports"] = "Tanılama raporları",
            ["Display name (optional)"] = "Görünen ad (isteğe bağlı)",
            ["Downloads"] = "İndirmeler",
            ["e.g. No Steam"] = "Ör. Steam yok",
            ["e.g. On update available, On startup"] = "Ör. Güncelleme olduğunda, başlangıçta",
            ["e.g. Steam"] = "Ör. Steam",
            ["Effects"] = "Efektler",
            ["Enable Acrylic"] = "Akrilik efekti etkinleştir",
            ["Enable animations"] = "Animasyonları etkinleştir",
            ["Enable Mica"] = "Mica efektini etkinleştir",
            ["Enabled"] = "Etkin",
            ["enterprise Dashboard"] = "Kurumsal pano",
            ["Export"] = "Dışa aktar",
            ["Export History"] = "Geçmişi dışa aktar",
            ["Export JSON"] = "JSON olarak dışa aktar",
            ["Export Settings"] = "Ayarları dışa aktar",
            ["Export the current state of your fleet in multiple formats."] = "Cihaz filonuzun mevcut durumunu farklı biçimlerde dışa aktarın.",
            ["Failed Operations (history)"] = "Başarısız işlemler (geçmiş)",
            ["Failed to load identities"] = "Yazılım kimlikleri yüklenemedi",
            ["Favorite Sources"] = "Favori kaynaklar",
            ["Featured"] = "Öne çıkanlar",
            ["Find packages to install"] = "Yüklenecek paketleri bulun",
            ["Founder"] = "Founder",
            ["Gaming essentials for streaming and play."] = "Yayın ve oyun için temel uygulamalar.",
            ["Hand-picked by the community"] = "Topluluk tarafından seçildi",
            ["Hardware acceleration"] = "Donanım hızlandırma",
            ["Health"] = "Sağlık",
            ["Health Score"] = "Sağlık puanı",
            ["history Policies"] = "Geçmiş ilkeleri",
            ["HTML, Excel and PDF exports are planned for an upcoming release."] = "HTML, Excel ve PDF dışa aktarma desteği sonraki sürümlerde planlanıyor.",
            ["Import"] = "İçe aktar",
            ["Import Settings"] = "Ayarları içe aktar",
            ["Install"] = "Yükle",
            ["Install a package to begin."] = "Başlamak için bir paket yükleyin.",
            ["Install All"] = "Tümünü yükle",
            ["Install games to see them here."] = "Burada görmek için oyun yükleyin.",
            ["Install software or record an install to start tracking identities."] = "Yazılım kimliklerini izlemek için yazılım yükleyin veya yükleme kaydı ekleyin.",
            ["Installed Packages"] = "Yüklü paketler",
            ["Last actions"] = "Son işlemler",
            ["Links"] = "Bağlantılar",
            ["Live Operations"] = "Devam eden işlemler",
            ["Living profiles for every piece of software on your machine."] = "Bilgisayarınızdaki her yazılım için güncel profiller.",
            ["Local machine only"] = "Yalnızca bu bilgisayar",
            ["Machine"] = "Bilgisayar",
            ["Manage all installed applications."] = "Yüklü tüm uygulamaları yönetin.",
            ["Manage available package updates."] = "Mevcut paket güncellemelerini yönetin.",
            ["Manage packages"] = "Paketleri yönetin",
            ["Manage packages and monitor your system."] = "Paketleri yönetin ve sisteminizi izleyin.",
            ["Manage WinGet sources."] = "WinGet kaynaklarını yönetin.",
            ["Manage your company fleet — remotely."] = "Şirket cihazlarınızı uzaktan yönetin.",
            ["Manual"] = "El ile",
            ["New Policy"] = "Yeni ilke",
            ["No automations"] = "Otomasyon yok",
            ["No blueprints"] = "Plan yok",
            ["No computers in the fleet"] = "Cihaz filosunda bilgisayar yok",
            ["No developer tools"] = "Geliştirici aracı yok",
            ["No diagnostics available"] = "Tanılama bilgisi yok",
            ["No favorites yet"] = "Henüz favori yok",
            ["No gaming packages found"] = "Oyun paketi bulunamadı",
            ["No history available"] = "Geçmiş kaydı yok",
            ["No installed packages"] = "Yüklü paket yok",
            ["No marketplace items match this search. Try a different keyword."] = "Aramanızla eşleşen mağaza öğesi yok. Başka bir sözcük deneyin.",
            ["No notifications"] = "Bildirim yok",
            ["No pinned sources"] = "Sabitlenmiş kaynak yok",
            ["No policies"] = "İlke yok",
            ["No recent activity"] = "Yakın zamanda etkinlik yok",
            ["No software profiles yet"] = "Henüz yazılım profili yok",
            ["No sources configured"] = "Yapılandırılmış kaynak yok",
            ["No updates available"] = "Kullanılabilir güncelleme yok",
            ["No violations"] = "İhlal yok",
            ["No workspaces"] = "Çalışma alanı yok",
            ["Nothing here yet"] = "Burada henüz bir şey yok",
            ["One action per line — e.g. install 7zip.7zip"] = "Her satıra bir işlem yazın — ör. install 7zip.7zip",
            ["Operations you run will appear here."] = "Çalıştırdığınız işlemler burada görünür.",
            ["Package ID / name"] = "Paket kimliği / adı",
            ["Pending Updates"] = "Bekleyen güncellemeler",
            ["Total Computers"] = "Toplam bilgisayar",
            ["Pending Updates (winget)"] = "Bekleyen güncellemeler (winget)",
            ["Pin frequently used sources here."] = "Sık kullandığınız kaynakları buraya sabitleyin.",
            ["Policies are enforced across the fleet."] = "İlkeler cihaz filosunda uygulanır.",
            ["Quick Actions"] = "Hızlı işlemler",
            ["Real system and environment information."] = "Gerçek sistem ve ortam bilgileri.",
            ["Recent Activity"] = "Son etkinlikler",
            ["Recent operations"] = "Son işlemler",
            ["Reusable installation templates with step-by-step actions."] = "Adım adım işlemler içeren, yeniden kullanılabilir yükleme şablonları.",
            ["Run a diagnostic scan to see results."] = "Sonuçları görmek için tanılama taraması başlatın.",
            ["Run Check Violations to scan the fleet."] = "Cihaz filosunu taramak için ihlal denetimini çalıştırın.",
            ["Search automations..."] = "Otomasyonlarda ara...",
            ["Search blueprints..."] = "Planlarda ara...",
            ["Search by name, or pick a category to get started."] = "Başlamak için ada göre arayın veya bir kategori seçin.",
            ["Search computers..."] = "Bilgisayarlarda ara...",
            ["Search history..."] = "Geçmişte ara...",
            ["Search packages..."] = "Paketlerde ara...",
            ["Search software..."] = "Yazılımlarda ara...",
            ["Search the marketplace..."] = "Mağazada ara...",
            ["Search to find packages"] = "Paket bulmak için arayın",
            ["Select a computer"] = "Bir bilgisayar seçin",
            ["Select a package to view details."] = "Ayrıntıları görmek için bir paket seçin.",
            ["Select a software"] = "Bir yazılım seçin",
            ["Select a workspace"] = "Bir çalışma alanı seçin",
            ["Settings are stored locally in settings.json. Export a copy before reinstalling Windows."] = "Ayarlar settings.json dosyasında yerel olarak saklanır. Windows'u yeniden kurmadan önce bir kopyasını dışa aktarın.",
            ["Sound notifications"] = "Sesli bildirimler",
            ["Statistics"] = "İstatistikler",
            ["System Details"] = "Sistem ayrıntıları",
            ["System Health"] = "Sistem sağlığı",
            ["Toast notifications"] = "Masaüstü bildirimleri",
            ["Trust Score"] = "Güven puanı",
            ["Trusted"] = "Güvenilir",
            ["UI virtualization"] = "Arayüz sanallaştırması",
            ["Unable to load packages"] = "Paketler yüklenemedi",
            ["Update All"] = "Tümünü güncelle",
            ["Upgrade packages"] = "Paketleri yükseltin",
            ["View all →"] = "Tümünü gör →",
            ["Violations"] = "İhlaller",
            ["Warnings"] = "Uyarılar",
            ["Weekly Fleet Report"] = "Haftalık cihaz filosu raporu",
            ["What does this blueprint do?"] = "Bu plan ne yapar?",
            ["What does this policy do?"] = "Bu ilke ne yapar?",
            ["What to run, e.g. upgrade all packages"] = "Çalıştırılacak işlem; ör. upgrade all",
            ["When this condition is met"] = "Bu koşul sağlandığında",
            ["Window opacity"] = "Pencere opaklığı",
            ["Workspace This Computer"] = "Çalışma alanı: Bu bilgisayar",
            ["All categories"] = "Tüm kategoriler",
            ["Are you sure?\n\nThis action cannot be undone."] = "Emin misiniz?\n\nBu işlem geri alınamaz.",
            ["Blueprint payload is empty."] = "Plan verisi boş.",
            ["Browse initialization failed"] = "Keşfet sayfası başlatılamadı",
            ["Browse suggestions"] = "Arama önerileri",
            ["Calculating health score…"] = "Sağlık puanı hesaplanıyor…",
            ["Check the spelling, try a shorter name, or browse a category instead."] = "Yazımı denetleyin, daha kısa bir ad deneyin veya bir kategoriye göz atın.",
            ["Command palette action failed"] = "Komut paleti işlemi başarısız",
            ["Create a new workspace"] = "Yeni çalışma alanı oluştur",
            ["Create a reusable installation template."] = "Yeniden kullanılabilir yükleme şablonu oluşturun.",
            ["Create an automatic package workflow."] = "Otomatik paket iş akışı oluşturun.",
            ["Curated package collections"] = "Seçilmiş paket koleksiyonları",
            ["Delete Automation"] = "Otomasyonu sil",
            ["Delete Blueprint"] = "Planı sil",
            ["Delete Failed"] = "Silme işlemi başarısız",
            ["Delete Policy"] = "İlkeyi sil",
            ["Delete Workspace"] = "Çalışma alanını sil",
            ["Discover / Discover"] = "Keşfet / Keşfet",
            ["Edit Policy"] = "İlkeyi düzenle",
            ["Editing a saved automation — changes apply when you save."] = "Kayıtlı otomasyonu düzenliyorsunuz; değişiklikler kaydettiğinizde uygulanır.",
            ["Editing a saved blueprint — changes apply when you save."] = "Kayıtlı planı düzenliyorsunuz; değişiklikler kaydettiğinizde uygulanır.",
            ["Enterprise fleet load failed"] = "Kurumsal cihaz filosu yüklenemedi",
            ["Export Fleet Report"] = "Cihaz filosu raporunu dışa aktar",
            ["Failed to load featured catalog"] = "Öne çıkanlar kataloğu yüklenemedi",
            ["Failed to load featured packages."] = "Öne çıkan paketler yüklenemedi.",
            ["Failed to load recent packages"] = "Son kullanılan paketler yüklenemedi",
            ["Failed to load updates."] = "Güncellemeler yüklenemedi.",
            ["History unavailable"] = "Geçmiş kullanılamıyor",
            ["Import failed: invalid file."] = "İçe aktarma başarısız: dosya geçersiz.",
            ["Install a package by name"] = "Ada göre paket yükle",
            ["Install every available update"] = "Kullanılabilir tüm güncellemeleri yükle",
            ["Load Failed"] = "Yükleme başarısız",
            ["Loading history failed"] = "Geçmiş yüklenemedi",
            ["Loading notifications failed"] = "Bildirimler yüklenemedi",
            ["Loading package statistics…"] = "Paket istatistikleri yükleniyor…",
            ["Loading Winget information failed"] = "Winget bilgileri yüklenemedi",
            ["Loading Winget sources failed"] = "Winget kaynakları yüklenemedi",
            ["Marketplace catalog load failed"] = "Mağaza kataloğu yüklenemedi",
            ["Marketplace install failed for '{item.Name}'"] = "'{item.Name}' mağazadan yüklenemedi",
            ["No installed packages to evaluate."] = "Değerlendirilecek yüklü paket yok.",
            ["No operations yet"] = "Henüz işlem yok",
            ["No packages found or Winget unavailable"] = "Paket bulunamadı veya Winget kullanılamıyor",
            ["No policy violations found."] = "İlke ihlali bulunamadı.",
            ["No results for "] = "Şunun için sonuç yok: ",
            ["No updates available."] = "Kullanılabilir güncelleme yok.",
            ["No updates were applied."] = "Hiçbir güncelleme uygulanmadı.",
            ["No verification data"] = "Doğrulama verisi yok",
            ["Not available"] = "Kullanılamıyor",
            ["Open a page"] = "Bir sayfa aç",
            ["Policy Check"] = "İlke denetimi",
            ["Policy Check Failed"] = "İlke denetimi başarısız",
            ["Policy evaluation failed"] = "İlke değerlendirilemedi",
            ["Record a rollback for '{identity.Name}'?"] = "'{identity.Name}' için geri alma kaydı oluşturulsun mu?",
            ["Remote management is not configured."] = "Uzaktan yönetim yapılandırılmamış.",
            ["Remove a package by name"] = "Ada göre paket kaldır",
            ["Remove all operation history"] = "Tüm işlem geçmişini kaldır",
            ["Remove the software profile for '{identity.Name}'?"] = "'{identity.Name}' yazılım profili kaldırılsın mı?",
            ["Restart Computer"] = "Bilgisayarı yeniden başlat",
            ["Reusable installation templates"] = "Yeniden kullanılabilir yükleme şablonları",
            ["Search and install '{argument}'"] = "'{argument}' ifadesini ara ve yükle",
            ["Search and install '{query}'"] = "'{query}' ifadesini ara ve yükle",
            ["Search and uninstall '{argument}'"] = "'{argument}' ifadesini ara ve kaldır",
            ["Search and uninstall '{query}'"] = "'{query}' ifadesini ara ve kaldır",
            ["Search and update '{argument}'"] = "'{argument}' ifadesini ara ve güncelle",
            ["Search and update '{query}'"] = "'{query}' ifadesini ara ve güncelle",
            ["Search packages or type '>' for commands..."] = "Paket arayın veya komutlar için '>' yazın...",
            ["Select a package to view details"] = "Ayrıntıları görmek için bir paket seçin",
            ["Settings exported."] = "Ayarlar dışa aktarıldı.",
            ["Settings imported."] = "Ayarlar içe aktarıldı.",
            ["Settings reset to defaults."] = "Ayarlar varsayılan değerlerine döndürüldü.",
            ["Sources unavailable"] = "Kaynaklar kullanılamıyor",
            ["System panel closed"] = "Sistem paneli kapatıldı",
            ["Theme payload is empty."] = "Tema verisi boş.",
            ["Type a command..."] = "Bir komut yazın...",
            ["Unable to update packages."] = "Paketler güncellenemedi.",
            ["Unable to update packages. Reason: WinGet operation failed."] = "Paketler güncellenemedi. Neden: WinGet işlemi başarısız oldu.",
            ["Update All cancellation requested"] = "Tümünü güncelleme işlemi için iptal istendi",
            ["Update All failed"] = "Tümünü güncelleme başarısız",
            ["Update all packages?"] = "Tüm paketler güncellensin mi?",
            ["Update cancelled."] = "Güncelleme iptal edildi.",
            ["Update complete — {result.Succeeded} successful, {result.Failed} failed, {result.CancelledCount} cancelled."] = "Güncelleme tamamlandı — {result.Succeeded} başarılı, {result.Failed} başarısız, {result.CancelledCount} iptal edildi.",
            ["Update complete — {result.Succeeded} updated."] = "Güncelleme tamamlandı — {result.Succeeded} paket güncellendi.",
            ["Update Failed"] = "Güncelleme başarısız",
            ["Updating packages…"] = "Paketler güncelleniyor…",
            ["Upgrade all packages"] = "Tüm paketleri yükselt",
            ["Values stay N/A until a verification service is available."] = "Doğrulama hizmeti sağlanana kadar değerler kullanılamaz olarak gösterilir.",
            ["WinGetX does not currently have a real package-signature/security scanner connected."] = "WinGetX'e henüz gerçek bir paket imza ve güvenlik tarayıcısı bağlanmadı.",
            ["Workspace payload is empty."] = "Çalışma alanı verisi boş.",
            ["Your package setup needs attention."] = "Paket yapılandırmanız ilgi gerektiriyor.",
            ["Save"] = "Kaydet",
            ["Steps"] = "Adımlar",
            ["{0} Installed • {1} Updates"] = "{0} yüklü • {1} güncelleme",
            ["{0} of {1} online"] = "{0}/{1} çevrimiçi",
            ["{0} packages found"] = "{0} paket bulundu",
            ["{0} operations"] = "{0} işlem",
            ["{0} items"] = "{0} öğe",
            ["{0} workspaces"] = "{0} çalışma alanı",
            ["{0} healthy"] = "{0} sağlıklı",
            ["{0} outdated"] = "{0} güncel değil",
            ["{0} deprecated"] = "{0} desteği sona ermiş",
            ["{0} failed operations"] = "{0} başarısız işlem",
            ["{0} unknown source"] = "{0} bilinmeyen kaynak",
            ["Based on {0} installed packages: "] = "{0} yüklü paket temel alınarak: ",
            ["Found {0} updates"] = "{0} güncelleme bulundu",
            ["{0} packages have updates available."] = "{0} paketin güncellemesi var.",
            ["{0} unread"] = "{0} okunmamış",
            ["{0} Running • {1} Pending • {2} Completed"] = "{0} çalışıyor • {1} bekliyor • {2} tamamlandı",
            ["{0} score"] = "{0} puan",
            ["{0} threads"] = "{0} iş parçacığı",
            ["{0} items found"] = "{0} öğe bulundu",
            ["{0} tools"] = "{0} araç",
            ["{0} operations found"] = "{0} işlem bulundu",
            ["{0} packages installed"] = "{0} paket yüklü",
            ["{0} updates available"] = "{0} güncelleme var",
            ["Some packages need attention."] = "Bazı paketler ilgi gerektiriyor.",
            ["Loading Winget information"] = "Winget bilgileri yükleniyor",
            ["Loading history"] = "Geçmiş yükleniyor",
            ["Loading notifications"] = "Bildirimler yükleniyor",
            ["Command palette operation failed for '{id}'"] = "Komut paleti işlemi başarısız: '{id}'",
            ["Command palette package not found"] = "Komut paletinde paket bulunamadı",
            ["Command palette resolve failed for '{query}'"] = "'{query}' komut paletinde çözümlenemedi",
            ["Command palette search failed for '{query}'"] = "'{query}' komut paletinde aranamadı",
            ["Update complete — {0} successful, {1} failed, {2} cancelled."] = "Güncelleme tamamlandı — {0} başarılı, {1} başarısız, {2} iptal edildi.",
            ["Update complete — {0} updated."] = "Güncelleme tamamlandı — {0} paket güncellendi.",
            ["Updating packages… {0} / {1}"] = "Paketler güncelleniyor… {0} / {1}",

        };

    private static readonly IReadOnlyDictionary<string, string> Translations =
        CreateTranslations();

    private static readonly (Regex Pattern, string Format, int Count)[] FormatTranslations =
        Translations
            .Where(pair => pair.Key.Contains('{'))
            .OrderByDescending(pair =>
                Regex.Replace(pair.Key, @"\{[^{}]+\}", string.Empty).Length)
            .Select(pair =>
            {
                var index = 0;
                var pattern = Regex.Replace(
                    Regex.Escape(pair.Key),
                    @"\\\{([^}]+)\}",
                    _ => $"(?<p{index++}>.+?)");
                var formatIndex = 0;
                var format = Regex.Replace(
                    pair.Value,
                    @"\{[^{}]+\}",
                    _ => $"{{{formatIndex++}}}");

                return (
                    new Regex($"^{pattern}$", RegexOptions.CultureInvariant),
                    format,
                    index);
            })
            .ToArray();

    private readonly Dictionary<string, LocalizedText> _texts = new(StringComparer.Ordinal);

    public static LocalizationManager? Current { get; private set; }

    private string _language = "English";

    public string Language => _language;

    public event EventHandler? LanguageChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public LocalizationManager(ISettingsService settings)
    {
        Current = this;
        _language = NormalizeLanguage(settings.Settings.Language);
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

        if (Translations.TryGetValue(value, out var translation))
            return translation;

        foreach (var (pattern, translatedFormat, count) in FormatTranslations)
        {
            var match = pattern.Match(value);
            if (!match.Success)
                continue;

            var arguments = new object[count];
            for (var index = 0; index < count; index++)
            {
                arguments[index] = match.Groups[$"p{index}"].Value;
            }

            return string.Format(CultureInfo.CurrentCulture, translatedFormat, arguments);
        }

        return value;
    }

    private static IReadOnlyDictionary<string, string> CreateTranslations()
    {
        var translations = Turkish.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);
        foreach (var (key, value) in AdditionalTurkish)
            translations[key] = value;

        return translations;
    }

    public void SetLanguage(string? language)
    {
        var normalized = NormalizeLanguage(language);
        if (Language == normalized)
            return;

        _language = normalized;

        foreach (var (key, text) in _texts)
            text.Value = TranslateValue(key);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
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
