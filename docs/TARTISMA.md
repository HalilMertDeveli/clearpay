# Tartışma — fikir alışverişi, sonra işlem

Ekipler **önce burada** yazar, **sonra** kod/docs işler. `docs/HANDOFF.md` yalnızca durum (landed / blok / sıradaki). Karar buradadır.

**Kural:** `src/` değişikliği = bu dosyada bir madde (kim / seçenekler / karar). Maddesiz `src/` yok.

Şablon:

```
## T-NNN — başlık
- **Kim:** birimler / ajanlar
- **Seçenekler:** A … / B …
- **Karar:** ne + neden
- **Sonra:** kim ne yazar (owned glob)
```

---

## T-001 — Identity deposu (TASK-03)

- **Kim:** Yazılım (Coder, Architect), Yönetim
- **Seçenekler:**
  - **A — SQLite** (`App_Data/identity.db`): login/kayıt bugün Docker SQL olmadan ayağa kalkar.
  - **B — SQL Server şimdi:** Compose şart; TASK-03’ü SQL’e kilitler; ledger ile aynı DB erken karışır.
- **Karar:** **A.** TASK-03 cookie Identity SQLite. Ledger / wallet SQL Server TASK-04+ (EF, login yeşil olunca). Canlıda Identity da Azure SQL’e taşınır (`docs/CANLI.md`); SQLite prod değil.
- **Sonra:** Coder TASK-03’ü bitirir (sayfalar + cookie). `App_Data/*.db` commit yok. Payments Domain durur. Havale API yok.

---

## Açık (henüz karar yok)

| Konu | Kim beklenir | Not |
|------|----------------|-----|
| Application portları (`IBankGateway`, `IWalletReader`, `ITransferExecutor`) | Architect + Payments | DIP kapısı; PageModel’de ledger yok. `src/` Application portu yazılacaksa buraya T-00x. |
| Identity + ledger tek SQL (canlı) | Coder + Deploy | TASK-16; şimdi değil. |
