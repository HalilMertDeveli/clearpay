# TASKS

Orchestrator her seferinde **sıradaki Todo** işini alır, bitirince Done’a taşır.

## Todo
- [ ] TASK-03: Giriş + Kayıt + boş cüzdan özeti
- [ ] TASK-04: SQL model + ledger iskeleti (Wallet, LedgerEntry, indeks)
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
- [ ] TASK-15: GitHub remote + GitHub Actions
- [ ] TASK-16: Azure App Service + Azure SQL (açık URL)

## Doing
- (boş)

## Done
- [x] TASK-01: Repo + MD sistemi + ajan rolleri
- [x] TASK-02: Solution iskeleti (.NET 8 Clean Arch, Docker SQL, sol menü layout)

## Notlar
- Kaynak: `docs/CALISMA-PLANI.md`. Yönetici: `docs/YONETICI-RAPORU.md`. Fark: `docs/FARK.md`.
- Kullanıcı kontrol eder; komut: «sıradaki işi yap» / «devam»
- **Öncelik TASK-03** (Coder). Architect Application portları (DIP) para özelliği **kapısı**. PageModel’de ledger/havale yok.
- TASK-16 Azure **şimdi değil** (`docs/CANLI.md`). Ads harcaması yok.
- Hosting / DNS kullanıcı hesabı gerektirir
- Para kuralları: `docs/SPEC.md` — 409, transaction, outbox bozulmaz
- Satıcı ödemesi ve canlı Redis/Rabbit Q2; Todo’da yok
