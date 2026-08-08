# WinGetX Guardrails (Sakın Yapma Listesi ve Omurga Sözleşmesi)

> WinGetX, Windows'taki her yazılımı, her değişikliği ve her otomasyonu
> tek bir ortak veri modeli üzerinde temsil edebilmelidir.

Bu belge, ürünün "ne" olduğunu değil "ne olmadığını" tanımlar.
Yeni özellik fikirleri önce buradan geçer.

---

## 1. Modül Testi

Bir özellik/modül ancak şu soruya cevap veriyorsa var olabilir:

> **Bu, yazılım yaşam döngüsünü yönetiyor mu?**

Hayırsa eklenmez. Örnekler (ASLA eklenmez):

- Dosya yöneticisi
- Registry Editor
- Task Manager
- CMD / Terminal emülatörü
- Explorer / Browser / Text Editor
- Antivirüs (Security bunu yapmaz)

---

## 2. Sakın Yapma Listesi

- ❌ Her şeyi WinGetX'in içine koyma.
- ❌ AI'ı her yere koyma. AI yalnızca: Log analizi, Workspace oluşturma,
  Blueprint üretme, Hata açıklama, Paket önerme. Chat kutusu yok.
- ❌ Çok fazla ayar ekleme. Mevcut Settings donduruldu.
- ❌ Modülleri kopuk bırakma. Bir olay tek modülü değil, zinciri etkiler.
- ❌ Aynı bilgiyi birden fazla yerde gösterme. Her bilginin tek sahibi var.
- ❌ Yalnızca geliştiriciye hitap etme. Teknik bilgi zorunlu olmasın.
- ❌ Önemli işlemi 3 tıklamadan uzun yapma.
- ❌ Modül enflasyonu yaratma (20 → 40 değil; bazı ekranlar birleşir).
- ❌ Her şeyi gerçek zamanlı hesaplama. Health gibi değerler yalnızca
  gerektiğinde hesaplanır.
- ❌ Arka planda çok servis çalıştırma. Hedef ~50 MB RAM, 300 MB değil.
- ❌ Plugin sistemini erken getirme (v3/v4'ten önce asla).
- ❌ Marketplace'i erken açma (içeriksiz ölür; önce 100 Blueprint / 100
  Environment / 100 Theme, sonra pazar).
- ❌ Cloud bağımlılığı oluşturma. Program internetsiz de çalışır; cloud opsiyonel.
- ❌ Dashboard'a her şeyi koyma. Dashboard rapor değil, kontrol panelidir.
- ❌ Fazla grafik koyma. Yalnızca kullanışlı olan.
- ❌ Environment'ı yalnızca program listesi yapma. Environment şunları içerir:
  Programs, Settings, Registry, PowerShell, Environment Variables, Extensions,
  Plugins, Automation, Theme, Terminal, Fonts, Certificates.
- ❌ Blueprint ile Environment'ı karıştırma:
  - **Environment** = bilgisayarın yaşam alanı (yaşayan durum).
  - **Blueprint** = tek seferlik dağıtım paketi (recipe).
- ❌ History'yi log sanma. History hikâyedir: commit, rollback, snapshot,
  restore, compare, diff.
- ❌ Security'yi antivirüs yapma. Security: Package Trust, Signature, Hash,
  Publisher, Supply Chain. Bitti.
- ❌ Diagnostics'i hata ekranı yapma. Hata kodu değil, insan dili:
  "Winget kaynağına ulaşılamıyor. İnternet bağlantınızı kontrol edin."
- ❌ Modüller arasında farklı tasarım kullanma: Kart, Toolbar, Search, Badge,
  Context Menu her yerde aynı.
- ❌ Gereksiz animasyon. 200 ms yeter.
- ❌ Çok pencere/dialog/popup. Tek pencere (Studio gibi).
- ❌ CLI'ı unutma. Her işlem CLI üretir.
- ❌ Kötü log formatı. Her log JSON; yıllar sonra bile okunabilir.

---

## 3. Modül Hiyerarşisi (Tier)

Her ekranın "ana ekran" olmaya çalıştığı proje kaybolur. Roller net:

| Tier | Modüller |
|------|----------|
| **Tier 1 — Ana** | Packages, Environment, Automation |
| **Tier 2 — Destek** | History, Discover, Security, Diagnostics |
| **Tier 3 — Araçlar** | Studio, Developer, Sources |

---

## 4. Ekran İmzaları

Her ekran aynı "liste + sağ panel + toolbar" hissini vermez. Her ekranın
kimliği vardır:

| Ekran | İmza |
|-------|------|
| Packages | Explorer |
| History | Timeline |
| Environment | Canvas |
| Automation | Flow Designer |
| Studio | IDE (Explorer, Properties, Console, Designer, Preview, Logs, Validation) |
| Security | Trust Center |
| Diagnostics | Health Monitor |
| Dashboard | Mission Control |
| Discover | Steam hissi (Bugün Popüler, Yeni Gelenler, Topluluk, Önerilenler) |

---

## 5. Omurga (Spine)

Yeni ekran eklemeden önce tamamlanacak katmanlar, bağımlılık sırasıyla:

```
Core Domain (tek Package modeli, entity-merkezli)
    ↓
Event Engine (tek olay → çok modül dinler)
    ↓
Task Engine (her işlem Task; queue, progress, cancel) [var, güçlendirilir]
    ↓
State Engine (Global App State; UI tek kaynaktan beslenir)
    ↓
History Engine (tüm değişiklikler tek formatta; hikâye)
    ↓
Snapshot Engine (geri alma / karşılaştırma)
    ↓
SQLite (kalıcılık; ayar dosyası değil, gerçek veri tabanı)
    ↓
UI (bu katman yalnızca yukarıdakini gösterir)
```

Plugin API bu sıralamanın **sonunda** gelir (v3/v4).

---

## 6. Veri Modeli (Entity-Merkezli Düşünce)

- Proje "ekran" olarak değil, **veri modeli** olarak geliştirilir.
- `Package` tek sınıftır, tek kaynaktır. Aynı gerçeği temsil eden 7+ sınıf
  asla bir arada yaşamaz.
- Birinci sınıf nesneler: Package, Environment, Blueprint, Automation, Task,
  History, Snapshot, Source, Plugin, User, Notification.
- Paket bilgisi (sürüm vb.) tek sahiplidir; UI onu yalnızca gösterir.
- CLI, API, mobil/web paneli veri modelinin doğal türevleridir.

---

## 7. Mühendislik Sözleşmeleri (Engineering Contracts)

- ✅ **North Star ölçümdür.** Environment Success Rate + Minutes Saved yerelde
  hesaplanır; özellik önceliği bu değerleri yükseltmeye bağlanır (vision.md).
- ✅ **Feature Freeze** — sürüm döngüsü ~6 aydır. Freeze sonrası yalnızca hata
  düzeltme ve RC stabilizasyonu yapılır; süreç değişmez (ARTICLE XV).
- ✅ **Erişilebilirlik** — klavye ile tam gezinme, focus state, yüksek kontrast
  ve screen reader (AutomationProperties) her ekranın kabul koşuludur.
- ✅ **Yerelleştirme** — string yok, kaynak yoksa kod merge edilmez (ARTICLE XI).
  Yeni ekran RTL'e hazır tasarlanır.
- ✅ **Analytics** — yalnızca toplam ve anonim veri; onay olmadan ham veri veya
  PII gönderilmez. Veri toplama kapalıyken program aynı çalışır.
- ✅ **Offline** — dokümantasyon, manifest ve ikon önbelleği yereldir; program
  internetsiz tam çalışır (cloud opsiyoneldir).
- ✅ **Modül sürümleme** — her modül kendi sürümüne sahiptir; dosya/DB formatı
  geriye dönük uyumludur, eski veri yüklemede migration otomatik çalışır.
- ✅ **Güven Şeffaflığı** — Registry / PATH / sürücü / servis değişikliğinden
  önce kullanıcıya tam diff önizlemesi ve geri alma noktası sunulur.
- ✅ **Veri modeli** — DB "olan her şeydir": append-only olay kaydı. Background
  indexer SQLite önbelleğini besler; Environment kök varlıktır.
- ✅ **Platform-first** — Core, WinGet değil Platform sözleşmelerini tanımlar;
  WinGet, motorlardan yalnızca biridir.
