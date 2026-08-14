# Yönetici raporu — 2026-08-13

Alipay özellik envanteri (SPEC 8 durur): [`YONETICI-CALISMA.md`](YONETICI-CALISMA.md).

**Kritik yol:** **TASK-05** — canlı özet (`SqlWalletReader` / `LedgerPair.NetOf`). TASK-04 **Done** (`a4755a1`). Havale API yok.

**T-028:** DI `EmptyWalletReader` → `SqlWalletReader`. SQL yoksa 0,00 ₺ (500 yok). Identity SQLite.

Lokal: http://localhost:5153 `/giris`

---

## RAG

| İz | Renk | Gerçek |
|----|------|--------|
| **Coder TASK-03** | **Yeşil** | `4fa4648` |
| **Coder TASK-04** | **Yeşil** | `a4755a1` EF ledger SQL |
| **Coder TASK-05** | **Sarı (Doing)** | Reader yazılmış; DI hâlâ Empty |
| **Payments** | **Yeşil** | Domain durur |
| **Tester** | **Yeşil** | TASK-03/04 suite |

---

## Karar

1. Doing = TASK-05. `POST /api/transfers` yok.
2. Coder `AddClearPay` kaydı + test. SQL: `docker compose up -d` veya belgele.
3. TASK-06 şimdi değil.
