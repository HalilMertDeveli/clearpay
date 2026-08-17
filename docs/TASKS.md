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

## Notlar
- Kaynak: `docs/CALISMA-PLANI.md`. Yönetici: `docs/YONETICI-RAPORU.md`. Fark: `docs/FARK.md`.
- Kullanıcı kontrol eder; komut: «sıradaki işi yap» / «devam»
- **Ürün sırası TASK-16.** Infra/docs hazır; açık URL **Halil tıklar**. Ads harcaması yok.
- Q2 Flutter JWT istemci (T-061) landed: `GET /api/wallet` + `mobile/clearpay`. TASK-16 Todo durur. 9. ekran yok.
- T-062: kayıt/kart/admin JWT; Flutter’da site işlemleri; Coder OWN `mobile/**/*.dart`. TASK-16 Todo.
- T-063: Flutter aynı git repo (`mobile/clearpay` + `ClearPay.code-workspace`). İç içe git yok. TASK-16 Todo.
- T-065: Flutter `firebase_core` (istemci). Firestore/Auth ikinci kasa yok. Firebase projesi Halil. TASK-16 Todo.
- TASK-12: `ConnectionStrings:RabbitMq` yok/düşer → log publisher; Hangfire yedek. Health `rabbit` up/down/off. CloudAMQP hesabı açılmaz.
- Hosting / DNS / CloudAMQP kullanıcı hesabı gerektirir
- Para kuralları: `docs/SPEC.md` — 409, transaction, outbox bozulmaz
- Satıcı ödemesi ve canlı Redis/Rabbit bağlama Q2; uygulama bind TASK-12 Done
- T-019: kod mimarisi Onion/Clean; n-tier aynı dört projenin adı (ikinci BLL/DAL yok). TASK-04 Done (T-024).
- **Operasyon (T-025):** `halilmertdeveliii@gmail.com`. GitHub `HalilMertDeveli`. TASK-16 **blok Halil** (`az login` + `.\infra\deploy.ps1`); ajan URL uydurmaz. Ads harcama yok.
- D: MSSQL/MySQL/Oracle bind (T-021) Deploy OWN; `docker-compose.yml` / `docker-compose.databases.yml` ezilmez.
