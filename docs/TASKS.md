# TASKS

Orchestrator her seferinde **sıradaki Todo** işini alır, bitirince Done’a taşır.

## Todo
- [ ] TASK-16: Azure App Service + Azure SQL (açık URL) — **Halil:** `az login` + `.\infra\deploy.ps1` (ajan hesap/URL uydurmaz)

## Doing
- (boş)

## Done
- [x] TASK-01: Repo + MD sistemi + ajan rolleri
- [x] TASK-02: Solution iskeleti (.NET 8 Clean Arch, Docker SQL, sol menü layout)
- [x] TASK-03: Giriş + Kayıt + boş cüzdan özeti
- [x] TASK-04: SQL model + ledger iskeleti (Wallet, LedgerEntry, indeks)
- [x] TASK-05: Cüzdan özeti canlı (bakiye, ay giden/gelen, son 5 hareket)
- [x] TASK-06: Havale (idempotency, transaction, 409)
- [x] TASK-07: Yükle / çek + sahte BankGateway REST
- [x] TASK-08: SOAP gateway + timeout aynı sözleşme
- [x] TASK-09: Hareketler + filtre + dekont (correlation id)
- [x] TASK-10: Admin (dondur, başarısız kuyruk, audit ara)
- [x] TASK-11: Outbox + Hangfire (timeout’ta kaybolmama)
- [x] TASK-12: Redis cache + RabbitMQ (lokal Compose)
- [x] TASK-13: xUnit sertleştirme (ledger, 409, API)
- [x] TASK-14: İngilizce README + Swagger + CV maddesi
- [x] TASK-15: GitHub remote + GitHub Actions
- [x] UI: mobil bankacılık düzeni — alt sekme çubuğu, ortalanmış kolon, ölçülü derinlik (T-053; TASK numarası değil, 8 ekran aynı)
- [x] UI: demo kayıtlı kart — Yükle/Çek paneli, PAN yok (T-055; 9. ekran yok)
- [x] UI: kamu cüzdan örnekleri — havale onay, unfreeze, tarih-bitiş, fiş corr/son4 (T-057; TASK numarası değil, 8 ekran aynı)
- [x] UI: web+mobil+JWT parite — GET transfer, 401 ProblemDetails, Yükle İptal, Admin rol hapı, Flutter tarih/sayfa/secure JWT/freeze (T-073; TASK numarası değil, 8 ekran aynı)
- [x] UI: Flutter TC demo giriş + FAST/QR kısayol + QR kanal (T-067; TASK numarası değil, 8 ekran aynı)
- [x] UI: splash + Bireysel/Kurumsal Identity AccountKind (T-068; Firebase kasa yok; 8 ekran aynı)
- [x] UI: web internet-şube kromu — masthead + hızlı işlemler (T-072; TASK numarası değil, 8 ekran aynı; YK kopyası değil)
- [x] UI: web dekont PDF — mevcut ledger correlation (T-079; 9. ekran yok)
- [x] UI: Flutter dekont borusu + PDF aynı fiş (T-069; T-079 web PDF durur; 9. ekran yok)
- [x] UI: Flutter launcher icon — navy C + teal halka (T-084; TASK numarası değil, 8 ekran aynı)
- [x] UI: Flutter web yok — yalnız mobil JWT; site Razor (T-087; 9. ekran / PWA yok)
- [x] UI: web+Flutter kimlik paritesi — kayıt telefon+tür, giriş TC demo, Flutter 4 dil (T-088; TASK numarası değil, 8 ekran aynı)
- [x] UI: TR/EN/DE/FR gerçekten UI değiştirir — web cookie `c=` + Flutter `L()` 8 işlem (T-090; TASK numarası değil, 8 ekran aynı)
- [x] UI: Kartlarım `/kartlar` — canlı CSS kart önizleme, son 4 kayıt, yükle mevcut gateway (T-097; ekran 9 kullanıcı isteği)

## Notlar
- Kaynak: `docs/CALISMA-PLANI.md`. Yönetici: `docs/YONETICI-RAPORU.md`. Fark: `docs/FARK.md`.
- Kullanıcı kontrol eder; komut: «sıradaki işi yap» / «devam»
- **Ürün sırası TASK-16.** Yol: [`YOL.md`](YOL.md) (T-059). Infra + T-095 Production web ayarı hazır; açık URL **Halil tıklar**. Ads harcaması yok. Cursor plan = YOL; TASK şişmez.
- Q2 Flutter JWT istemci (T-061) landed: `GET /api/wallet` + `mobile/clearpay`. TASK-16 Todo durur. 9. ekran yok.
- T-062: kayıt/kart/admin JWT; Flutter’da site işlemleri; Coder OWN `mobile/**/*.dart`. TASK-16 Todo.
- T-063: Flutter aynı git repo (`mobile/clearpay` + `ClearPay.code-workspace`). İç içe git yok. TASK-16 Todo.
- T-065: Flutter `firebase_core` (istemci). Firestore/Auth ikinci kasa yok. Firebase projesi Halil. TASK-16 Todo.
- T-091: Flutter `cloud_firestore` yalnız `app_meta/ping` (tutar yok). Kasa JWT → SQL. TASK-16 Todo.
- T-066: Flutter sol `NavigationDrawer` + özet bakiye kartı / kısayol (8 işlem). YK/Papara marka yok. TASK-16 Todo.
- T-067: Flutter TC demo giriş + ana ızgara (FAST→Havale, QR al/öde kanal, Daha fazla, Piyasalar park). YK/World/Jet QR yok. TASK-16 Todo.
- T-068: Splash + Bireysel/Kurumsal. SQL Identity `AccountKind` + JWT `account_kind`. Firebase’e mod yazılmaz. TASK-16 Todo.
- T-071: SignalR `/hubs/wallet` — mobil yazınca web yenilenir; bakiye hub’da yok. Halil tıkları: [`API-ESZAMAN.md`](API-ESZAMAN.md). TASK-16 Todo.
- T-072: girişli Razor internet-şube kromu (YK *düzeni*, marka yok). 560px kolon 1120px. Tabbar ≤800px durur. TASK-16 Todo.
- T-079: dekont PDF (aynı SQL fiş). Razor `handler=Pdf` + `GET /api/receipts/{id}/pdf`. TASK-16 Todo.
- T-069: Flutter başarı → Dekont; PDF byte API’den. Örnek `aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeee0001`. TASK-16 Todo.
- T-087: Flutter `web/` yok. Site Razor. `flutter run -d chrome` yok. TASK-16 Todo.
- T-088: Razor kayıt telefon+AccountKind; giriş TC demo; Flutter TR/EN/DE/FR chrome. Web forgot / QR web / Azure durur. TASK-16 Todo.
- T-090: aynı 4 dil iki yüzeyde UI’yı sürer (Flutter 8 işlem `L()`; web picker/`c=`). 5. dil / 9. ekran / Flutter Chrome yok. TASK-16 Todo.
- T-084: Flutter launcher navy C + teal halka (mipmap/AppIcon). TASK-16 Todo.
- T-086: Flutter şifre kurtarma + telefon + Firebase Auth (web Razor forgot **park**). TASK-16 Todo.
- TASK-12: `ConnectionStrings:RabbitMq` yok/düşer → log publisher; Hangfire yedek. Health `rabbit` up/down/off. CloudAMQP hesabı açılmaz.
- Hosting / DNS / CloudAMQP kullanıcı hesabı gerektirir
- Para kuralları: `docs/SPEC.md` — 409, transaction, outbox bozulmaz
- Satıcı ödemesi ve canlı Redis/Rabbit bağlama Q2; uygulama bind TASK-12 Done
- T-019: kod mimarisi Onion/Clean; n-tier aynı dört projenin adı (ikinci BLL/DAL yok). TASK-04 Done (T-024).
- **Operasyon (T-025):** `halilmertdeveliii@gmail.com`. GitHub `HalilMertDeveli`. TASK-16 **blok Halil** (`az login` + `.\infra\deploy.ps1`); ajan URL uydurmaz. Ads harcama yok.
- D: MSSQL/MySQL/Oracle bind (T-021) Deploy OWN; `docker-compose.yml` / `docker-compose.databases.yml` ezilmez.
