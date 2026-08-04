# WinGetX Founder Roadmap

> Founder Edition modülleri: FounderCenter, ThemeStudio, GameMode, FounderLab, PerformanceOverlay, DeveloperConsole.
> Modül vizyonu: `docs/founder-vision.md`. Mimari: `docs/architecture.md`.
> Kural: Founder özellikleri ana projeye karışmaz (ARTICLE XIII).

## M0 — Kurulum

- `WinGetX.Founder` proje yapısı (`WinGetX.Founder/` + modül klasörleri)
- Modül arayüzü / modül shell
- Lisans, imza ve dağıtım kararları

## M1 — Founder Center

- Modül çerçevesi: modülleri yükle, listele, yönet
- Modül durumları (yüklü / deneme / kilitli)
- Studio Dashboard temeli (backlog Issue 230)

## M2 — Theme Studio

- Tema düzenleyici: renk, font, radius, spacing, shadow, animation
- Canlı önizleme (ARTICLE XII tema sistemine uyumlu)
- Tema dışa aktarma / içe aktarma

## M3 — Game Mode

- Oyun modu: bildirimlerde DND, kaynak kilitleme
- Otomatik kurulum profilleri
- Oyundayken performans önceliği

## M4 — Performance Overlay

- Canlı CPU / RAM / Disk / GPU göstergesi
- Oyun içi overlay
- Kayıt / geçmiş (backlog Issue 225 temeli)

## M5 — Founder Lab

- Deneysel özellikler için izole test alanı
- Özellik bayrakları (feature flags)
- Founder dışı kullanıcıya kapalı

## M6 — Developer Console

- Geliştirici komutları ve teşhis
- Scripting / otomasyon
- Log görüntüleyici

## Paylaşılan Çekirdek Kararı

Founder modülleri ortak iş mantığına ihtiyaç duyarsa:

- Ana projeye bağımlılık oluşturulmaz.
- Gerekirse ayrı paylaşılan çekirdek (paylaşılan domain) M0–M1 arasında değerlendirilir ve buraya not düşülür.
