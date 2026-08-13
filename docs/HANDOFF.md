# HANDOFF — ajan konuşma defteri

Kardeş ajanlar buraya **append** eder. SPEC/PLAN/TASKS yerine geçmez. Kullanıcı checklist’i: `docs/SENIN-ISLERIN.md`. Bölüm silme / üzerine yazma.

## 2026-08-13 — Architect

- `docs/ARCHITECTURE.md` origin/main’de (`62bbddd`): katmanlar, SPEC ekran haritası, tek host, cookie sonra JWT, ledger Web dışında, Q1 Hangfire/outbox vs Q2 Redis/Rabbit.
- TASK-04 Coder/Payments: 1 user = 1 wallet; Identity SQLite ayrı; ledger SQL Server; `IdempotencyRecord.Key` unique; `LedgerEntry(WalletId, CreatedAt)` indeks.
- Çatışma: pull temizdi; Architect commit push edildi. Sonra Deploy `3df4c57` HANDOFF’u overwrite etti (Architect bölümü silindi — burada geri eklendi). `stash pop` README/DEPLOY/compose yüzünden durdu; Coder TASK-03 WIP `stash@{0}` (`wip-other-agents-before-architect-pull`). Architect ikinci push yapmadı (kirli ağaç + Deploy commit).

## 2026-08-13 — docs-ogrenme

- **OWN:** `docs/OGRENME.md`, `docs/SENIN-ISLERIN.md`; `docs/AGENTS.md` ve `README.md` Docs’ta tek satır link.
- `src/`, `tests/`, `docker-compose`, `ARCHITECTURE.md` dokunulmadı.
- Öğrenme defteri: ev → kilit → para motoru; 409/tx/outbox henüz yok (bilinçli). HANDOFF burada ajan defteri olarak geçiyor.
- README Docs satırları OGRENME + SENIN-ISLERIN; Coder README’yi TASK-03 ile commit ediyorsa bu iki maddeyi koru.
- Dosyalar yazıldı: `docs/OGRENME.md` (Senin yapman gerekenler önde), `docs/SENIN-ISLERIN.md` (VS checklist). src/tests/compose’a dokunulmadı.

## 2026-08-13 — Deploy

- **OWN:** `docker-compose.yml`, `.dockerignore`, `docs/DEPLOY.md` (lokal netleştirme). Azure hesabı açılmadı. Razor/CSS/Domain dokunulmadı.
- `.dockerignore`: `bin/`, `obj/` (ve git/IDE/secret).
- Compose: SQL Server aynı (1433, volume, SA env). Web Compose servisi değil — host’ta `dotnet run --project src/ClearPay.Web --launch-profile http` (5153). Redis/Rabbit TASK-12.
- Bu makinede `docker` PATH’te yok; YAML SQL-only bırakıldı.
- Follow-up: `docker-compose.yml` + `docs/DEPLOY.md` bu commit’te. HANDOFF’ta Architect / Payments / Tester bölümleri korundu.

## 2026-08-13 — Payments (TASK-04 Domain)

- **OWN:** `src/ClearPay.Domain/Ledger/**` only. Web/Razor/CSS, tests, compose, TASKS, EF/Infrastructure dokunulmadı. `DomainAssembly.cs` ve `Identity/AppRoles.cs` Coder’da — bekletildi, yeni dosya eklendi.
- POCOs: `Wallet` (1 user = 1 wallet, `IsFrozen`, bakiye kolonu yok), `LedgerEntry` (signed Amount, `PairId`), `Transfer`, `IdempotencyRecord` (`Key` unique → 409), `AuditLog`, `OutboxMessage`.
- Invariant: `LedgerPair.Create` +/− çift; `CreateRefund` ters kayıt. `MoneyTransaction.RequiredInserts` = debit, credit, Transfer, Idempotency, Audit, Outbox — tek SQL transaction. `UPDATE Balance` helper yok.
- Coder EF (SQL Server, Identity SQLite değil): unique `Wallet.UserId`, unique `IdempotencyRecord.Key`, index `LedgerEntry(WalletId, CreatedAt)`. İsimler `LedgerSchema`. Duplicate key → 409. Havale API TASK-06.
- net8.0 Domain derlenmeli. Commit: Domain Ledger + bu HANDOFF bölümü.

## 2026-08-13 — Tester

- **OWN:** `tests/ClearPay.Tests/AuthOrUiTests.cs` only. `PlaceholderPagesTests.cs` dokunulmadı. `src/**` yok.
- Identity/login henüz yok. Mevcut menü + `/api/health` 200. `/Account/Login` 404 olunca giriş/kayıt/cüzdan assert no-op; Coder login basınca aynı testler 200 + korumalı rota redirect + kayıt sonrası `0,00 ₺` bekler.
- TASK-06 409 testi `[Fact(Skip=...)]`. Identity `AuthorizeFolder` gelince `PlaceholderPagesTests` anonim 200 kırılır — Coder o dosyayı silsin veya Tester’a bırakın.
- `dotnet test` sonucu bir sonraki nota.

## 2026-08-13 — Payments (Domain files)

- `src/ClearPay.Domain/Ledger/**` yazıldı; `dotnet build` ClearPay.Domain net8.0 yeşil. EF/Web yok. Coder `AppRoles` / `DomainAssembly` dokunulmadı.
