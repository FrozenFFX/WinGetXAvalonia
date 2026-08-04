# Future Ideas

> GitHub'da açık Issue kalabalığı yaratmadan, sohbetlerde üretilen fikirlerin arşivi.
> Kural: bu liste GitHub Issue'ya dönüştürülmez. Sadece ICEBOX olarak burada durur.

## v5+ Vizyon

WinGetX Platform:
- Package Engine, Update Engine, Search Engine, Notification Engine, Theme Engine, Plugin Engine, Analytics Engine, Diagnostics Engine, Automation Engine

Applications:
- Dashboard, Installed, Browse, Updates, History, Collections, Workspaces, Reports, Studio, Enterprise

Founder:
- Founder Center, Founder Lab, Theme Studio, Performance Overlay, Game Mode, Developer Console

## Issue 221 — Package Sandbox Mode

Kurulum öncesi sanal test: Package → Virtual Environment → Test Install → Success → Real Install.
Gerçek sisteme dokunmadan ön kontrol. (Windows Sandbox API altyapısı gerekir — 🧪 Experimental)

## Issue 222 — Install Cost Analyzer

Kurulumdan önce analiz: Download (425 MB), Disk (1.2 GB), Estimated Time (2m 14s), Restart (No), Dependencies (3).
`winget show` manifest ayrıştırması altyapısı hazır (WingetParser.ParseDetails).

## Issue 223 — Smart Duplicate Detection

Sistem taranır, örtüşen paketler bulunur (Git / Portable Git / Git for Windows) ve "These packages may overlap" önerisi gösterilir. Yerel analiz.

## Issue 224 — Application Timeline

WinGetX kullanım zaman çizelgesi: Installed, Updated, Removed, Settings Changed, Theme Changed, Collection Imported. IHistoryService + EventBus altyapısı hazır.

## Issue 225 — Resource Usage History

CPU/RAM/Disk geçmişi → Operation → History → Graph. ISystemMonitorService altyapısı hazır (ring buffer + grafik gerekir).

## Issue 226 — Package Dependencies Map

Interaktif bağımlılık görünümü: Zoom, Pan, Export PNG. (Grafik kontrolü + manifest Dependencies verisi zayıf — 🧪 Experimental)

## Issue 227 — Package Risk Analyzer

Her işlem öncesi Risk: Low/Medium/High. Sebepler: Restart Required, Preview Version, Unknown Publisher, Large Download.
PackageHealthCalculator altyapısı hazır — tamamen yerel hesaplama, domain'e %100 uyumlu.

## Issue 228 — Queue Scheduler

Queue → Run After (20:00) → Weekend → When Idle → After Restart. QueuePanelViewModel/IOperationService üzerine zamanlama + idle servisi.

## Issue 229 — Package Dashboard Cards

Her paket için mini kart: Installed, Latest, Favorite, Pinned, Publisher. Detaya gitmeden özet.

## Issue 230 — WinGetX Studio Dashboard

Diagnostics, Queue, Logs, Plugins, Themes, Collections, Automation, Reports, Founder — tüm gelişmiş araçlar tek merkezde. (Tek özellik değil, büyük UI yeniden yapılanma — 🧪 Experimental)

## Issue 231 — Smart Recommendations

Kullanıcının yüklü uygulamalarına göre öneriler. Örn: VS Code yüklüyse Git, Docker Desktop ve PowerShell 7 öner.

## Issue 232 — Startup Impact Analyzer

Başlangıçta çalışan uygulamaları analiz eder. Etki: Low / Medium / High. Kapatma önerileri sunar.

## Issue 233 — Package Collections

Birden fazla uygulamayı tek koleksiyon halinde kaydet (Developer, Gaming, Streaming, Office). Tek tıkla kur.

## Issue 234 — Workspace Profiles

Farklı bilgisayar profilleri (Work, Gaming, School, Portable). Her profil farklı koleksiyon kullanır.

## Issue 235 — Auto Update Rules

Paket bazlı kurallar: Chrome → Auto Update, Discord → Ask First, Steam → Ignore.

## Issue 236 — Ignore Updates

Belirli sürümleri veya paketleri kalıcı olarak yok say.

## Issue 237 — Package Compare

İki paketi karşılaştır: Version, Publisher, Installer, Dependencies, License, Source.

## Issue 238 — Portable Applications Manager

Portable uygulamaları ayrı sekmede yönet.

## Issue 239 — Package Health Monitor

Kurulu paketlerin genel sağlık durumu: Outdated, Broken, Deprecated, Unknown. (PackageHealthSummaryCalculator altyapısı hazır)

## Issue 240 — Smart Cleanup

Eski installer, cache, unused packages, temporary files tek tıkla temizle.

## Issue 241 — Package Notes

Her uygulamaya kişisel not eklenebilir.

## Issue 242 — Tags

Package etiketleri: Work, Gaming, Development, Important, Pinned.

## Issue 243 — Package Groups

Uygulamaları klasör mantığında grupla.

## Issue 244 — Restore Missing Packages

Export edilen listedeki eksik uygulamaları bulup geri yükle.

## Issue 245 — Winget Source Manager

Winget kaynaklarını ekle/sil/güncelle.

## Issue 246 — Package Integrity Check

Manifest, Hash, Signature, Publisher kontrolü.

## Issue 247 — Advanced Search

Regex, Publisher, Version, Source, Architecture, Scope filtreleri. (SearchEngine v2 altyapısı hazır)

## Issue 248 — Scheduled Updates

Belirli gün ve saatte otomatik güncelle.

## Issue 249 — Install History Details

Kurulum süresi, indirilen veri, çıkış kodu, log.

## Issue 250 — Package Benchmarks

En çok kullanılan, en hızlı güncellenen, en büyük, en eski istatistikleri.

## Issue 251 — Package Screenshots

Manifest veya internetten ekran görüntüleri. 🧪 Experimental

## Issue 252 — Package Reviews

Topluluk yorumları. 🧪 Experimental

## Issue 253 — Release Notes Viewer

Sürüm notlarını uygulama içinde göster.

## Issue 254 — Offline Repository

İndirilen manifestleri çevrimdışı kullan.

## Issue 255 — Queue Priorities

High / Normal / Low öncelik sistemi. (QueuePanelViewModel altyapısı hazır)

## Issue 256 — Queue Pause & Resume

İşlem kuyruğunu durdur/devam ettir.

## Issue 257 — Queue Export

Bekleyen işlemleri JSON olarak dışa aktar.

## Issue 258 — System Snapshot

Yüklü paketler, Winget, Windows, Ayarlar tek dosyada kaydedilir.

## Issue 259 — Diagnostics Report

Tek tıkla detaylı sistem raporu oluştur.

## Issue 260 — Plugin Marketplace

WinGetX eklentilerini indir. 🧪 Experimental

## Issue 261 — Theme Marketplace

Tema indir/paylaş.

## Issue 262 — Icon Cache Manager

İkon önbelleğini yönet.

## Issue 263 — Download Manager

Tüm indirmeleri tek panelden yönet.

## Issue 264 — Mirror Selection

Alternatif indirme sunucuları.

## Issue 265 — Proxy Manager

Proxy ayarlarını yönet.

## Issue 266 — Bandwidth Limiter

İndirme hız sınırı.

## Issue 267 — Network Diagnostics

DNS, Latency, Download, Upload testleri.

## Issue 268 — Package Security Score

0–100 güven puanı. (PackageHealthCalculator skor altyapısı hazır)

## Issue 269 — CVE Checker

Kurulu uygulamaların bilinen güvenlik açıklarını kontrol eder. 🧪 Experimental

## Issue 270 — Digital Signature Viewer

İmzaları görüntüler.

## Issue 271 — Installer Preview

Kurulum komutunu çalıştırmadan önce göster.

## Issue 272 — Silent Install Profiles

Sessiz kurulum profilleri oluştur.

## Issue 273 — Multi-PC Sync

Birden fazla cihaz arasında ayar senkronizasyonu. 🧪 Experimental

## Issue 274 — Cloud Backup

GitHub / OneDrive yedekleme. 🧪 Experimental

## Issue 275 — Package Export Templates

JSON, YAML, TXT, CSV.

## Issue 276 — Automation Rules

"Discord güncellenince bildirim gönder" gibi kurallar.

## Issue 277 — Event Triggers

Install, Update, Remove, Startup, Shutdown tetikleyicileri.

## Issue 278 — Operation Scheduler

Belirli saatte kurulum.

## Issue 279 — Performance Dashboard

CPU, RAM, Disk, GPU, Network canlı grafikler.

## Issue 280 — Resource Alerts

CPU veya RAM belirli seviyeyi geçerse uyar.

## Issue 281 — Accessibility Mode

Büyük yazılar, yüksek kontrast, klavye navigasyonu, Screen Reader desteği.

## Issue 282 — Keyboard Shortcut Manager

Kısayolları özelleştir.

## Issue 283 — Command Palette

Ctrl+K ile tüm komutlara erişim.

## Issue 284 — AI Assistant

WinGetX içinde paket önerileri ve hata açıklamaları. 🧪 Experimental

## Issue 285 — Enterprise Policies

Kurumsal kullanım için merkezi politika yönetimi.

## Issue 286 — WinGetX Cloud

Settings, Collections, History, Favorites, Themes, Plugins cihazlar arasında senkronize edilir. 🧪 Long-term Vision

## Issue 287 — Undo / Rollback

Son işlemi geri alma, önceki sürüme dönme. Install/Update/Remove işlemleri geri alınabilir.

## Issue 288 — Windows Update Integration

Backlog tamamen winget paketlerine odaklı; Windows güncellemeleri (Windows Update) için görünüm ve kontrol eklenir.

## Issue 289 — Notification Rules / Do Not Disturb

Bildirim altyapısı mevcut ama "hangi olayda bildirim" yönetimi yok: per-olay bildirim seçimi, sessiz saatler.

## Issue 290 — Localization Manager

Anayasa ARTICLE XI dil desteğini zorunlu kılıyor; dil paketi yönetimi: dil seçimi, dil paketi indirme, topluluk çevirileri.

## Issue 291 — Disk Usage by App

Smart Cleanup (#240) var ama "hangi uygulama ne kadar yer kaplıyor" görünümü yok: paket bazlı disk boyutu raporu.

## Issue 292 — Updates: Cancellable Refresh

`UpdatesViewModel.Refresh()` uzun süren `GetUpdatesAsync()` çağrısında iptal edilemez. `CancellationTokenSource` ile iptal edilebilir Refresh + `OperationCanceledException` handle'ı eklenir. (v1.0 kapsamı dışı)
