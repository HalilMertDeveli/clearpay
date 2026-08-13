# TASKS

Orchestrator her seferinde **sıradaki Todo** işini alır, bitirince Done’a taşır.

## Todo
- [ ] TASK-05: Cüzdan özeti canlı (bakiye, ay giden/gelen, son 5 hareket)
- [ ] TASK-06: Havale (idempotency, transaction, 409)
- [ ] TASK-07: Yükle / çek + sahte BankGateway REST
- [ ] TASK-08: SOAP gateway + timeout aynı sözleşme
- [ ] TASK-09: Hareketler + filtre + dekont (correlation id)
- [ ] TASK-10: Admin (dondur, başarısız kuyruk, audit ara)
- [ ] TASK-11: Outbox + Hangfire (timeout’ta kaybolmama)
- [ ] TASK-12: Redis cache + RabbitMQ (lokal Compose)
- [ ] TASK-13: xUnit sertleştirme (ledger, 409, API)
- [ ] TASK-14: İngilizce README + Swagger + CV maddesi
- [ ] TASK-16: Azure App Service + Azure SQL (açık URL)

## Doing
- [ ] TASK-04: SQL model + ledger iskeleti (Wallet, LedgerEntry, indeks)

## Done
- [x] TASK-01: Repo + MD sistemi + ajan rolleri
- [x] TASK-02: Solution iskeleti (.NET 8 Clean Arch, Docker SQL, sol menü layout)
- [x] TASK-03: Giriş + Kayıt + boş cüzdan özeti
- [x] TASK-15: GitHub remote + GitHub Actions

## Notlar
- Kaynak: `docs/CALISMA-PLANI.md`. Yönetici: `docs/YONETICI-RAPORU.md`. Fark: `docs/FARK.md`.
- Kullanıcı kontrol eder; komut: «sıradaki işi yap» / «devam»
- **Ürün sırası TASK-04** (EF SQL Server ledger). TASK-03 Done (`4fa4648`). Havale API (TASK-06) yok. Identity SQLite (T-009); ledger SQL.
- TASK-12: Compose Redis/Rabbit var; uygulama bağlı değil. Ads harcaması yok.
- Hosting / DNS / CloudAMQP kullanıcı hesabı gerektirir
- Para kuralları: `docs/SPEC.md` — 409, transaction, outbox bozulmaz
- Satıcı ödemesi ve canlı Redis/Rabbit bağlama Q2; Todo’da TASK-12
- T-019: kod mimarisi Onion/Clean; n-tier aynı dört projenin adı (ikinci BLL/DAL yok). TASK-04 sırası değişmez.
