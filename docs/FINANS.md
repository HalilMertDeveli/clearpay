# FINANS — çift kayıt, mutabakat, demo

ClearPay lisanslı ödeme kuruluşu **değildir**. Ekrandaki tutarlar **demo**; sahte `BankGateway`. Bu belge muhasebe / iç kontrol gözüyle **neden defter**. Kod yazılmaz burada.

- Kurallar: [`SPEC.md`](SPEC.md) § Para kuralları
- Motor (kod): Payments — `LedgerPair`, `LedgerEntry`, `Wallet` (bakiye kolonu yok)
- Mülakat pitch: [`SATIS.md`](SATIS.md) / [`FARK.md`](FARK.md)
- 409 / tx / outbox (mühendis): [`OGRENME.md`](OGRENME.md)
- İnsan ödeme checklist: [`ODEME-SENIN.md`](ODEME-SENIN.md)

Kilit cümle: **Bakiye bir kolon değil; imzalı satırların net’i.** Footer: **Demo — sahte banka gateway.**

---

## 1. Çift kayıt (double-entry)

Klasik muhasebe: her işlemde en az bir **borç (debit)** ve bir **alacak (credit)**; yevmiye dengelenir; mizan toplamı sıfırdır. ClearPay bunu cüzdan defterine indirger — tam hesap planı (GL, Kasa, Banka, 120/320) **yok**; dürüst sınır.

Burada satır **imzalı TRY** (ölçek 2, kuruş):

| İşaret | Anlam (cüzdan) | Muhasebe yakını |
|--------|----------------|-----------------|
| `Amount < 0` | Çıkış (debit) | Bu cüzdan borçlanır |
| `Amount > 0` | Giriş (credit) | Bu cüzdan alacaklanır |

Bir hareket = **iki** `LedgerEntry`, aynı `PairId`, aynı `CorrelationId`, `Amount` toplamı **tam 0**. Domain: `LedgerPair.Create` / `EnsureBalanced`. `Wallet`’ta `Balance` kolonu yok; bakiye = `LedgerPair.NetOf(satırlar, walletId)`.

### Örnek: Ali → Veli 40,00 ₺

```
PairId = P1          CorrelationId = C1
Ali    LedgerEntry   Amount = −40,00   Kind = Transfer
Veli   LedgerEntry   Amount = +40,00   Kind = Transfer
Toplam = 0
Ali bakiyesi  = önceki net − 40
Veli bakiyesi = önceki net + 40
İki cüzdanın bu çifte net’i = 0
```

Tester kanıtı (TASK-04): havale 40 ₺ + ters 15 ₺ → gönderen `NetOf = −25`, alıcı `+25`, iki net toplamı 0.

### Türler (`LedgerEntryKind`)

| Kind | Ne | Karşı taraf |
|------|-----|-------------|
| `Transfer` | P2P havale | İki müşteri cüzdanı |
| `TopUp` | Yükle | Müşteri + sistem/float (TASK-07; çift yine zorunlu) |
| `Withdraw` | Çek | Tersi: müşteri debit, float credit |
| `Refund` | İade | `CreateRefund`: eski credit cüzdanı debit olur — **yeni çift**, eski satır silinmez |

İade “bakiye düzelt” değildir. Eski yevmiye durur; ters yevmiye eklenir. Denetçi iki çifti de görür.

### Ne değil

Tam **chart of accounts**, TCMB mizanı, KDV, gelir tablosu yok. Finans mülakatında söyle: *kapalı devre cüzdan defteri; her kuruşun +/−’si ve izi var; genel muhasebe paketi değil.*

---

## 2. Neden `UPDATE Balance` yok

Öğrenci CRUD: `wallet.Balance -= amount; SaveChanges();` — tek kolon, tek yazma. ClearPay’de bu yol **yasak** (SPEC madde 5; Domain yorumu; PLAN).

| Sorun | Ne olur |
|-------|---------|
| **İz yok** | “Bakiye 100 ₺” — kim kesti, ne zaman, neden? Dekont üretilemez. |
| **Yarış** | İki eşzamanlı UPDATE; last-write-wins; para kaybolur veya şişer. Unique `Idempotency-Key` kolona yapışmaz. |
| **Mutabakat yok** | Banka / gateway / log “C1 ile 40 ₺” der; sende sadece yeni sayı. Eşleştirme anahtarı yok. |
| **İade** | `Balance +=` = ikinci hatayı üstüne yazmak. Ters kayıt = açıklanabilir düzeltme. |
| **Kısmi yazma** | Gönderen UPDATE oldu, alıcı fail. Biri zengin, biri fakir. Çift kayıt + **tek SQL transaction** bunu keser. |
| **Freeze** | Kolon hâlâ UPDATE edilebilir. Kural `CanDebit` + debit satırı yazmamaktır. |

Mülakat cümlesi: *Bakiye türetilmiş görünüm. Kaynak yevmiye. Denormalize kolon (PLAN TASK-05) olsa bile invariant test: kolon = `NetOf`; audit’siz `UPDATE Balance` yok.*

`MoneyTransaction.RequiredInserts` (tek commit): debit satırı, credit satırı, `Transfer`, `IdempotencyRecord`, `AuditLog`, `OutboxMessage`. Domain `SaveChanges` çağırmaz.

---

## 3. Correlation id — mutabakat anahtarı

Finans / operasyon “bu 17,40 ₺ nereye gitti?” diye sorar. Cevap bir **Guid** ile toplanır, tahminle değil.

Aynı `CorrelationId` şunlarda durur (Domain + SPEC dekont):

- Her iki `LedgerEntry` (çift)
- `Transfer`
- `AuditLog` (kim, ne, ne zaman)
- `OutboxMessage`
- Sahte `BankGateway` isteği
- Ekran **dekont** (TASK-09)
- Serilog (PLAN)

### `PairId` ≠ `CorrelationId`

| Alan | Soru | Cevap |
|------|------|--------|
| `PairId` | Bu +/− **çift** hangisi? | Bir yevmiyenin iki satırı. Muhasebe bağı. |
| `CorrelationId` | Bu **iş** uçtan uca hangisi? | HTTP → defter → audit → outbox → gateway → dekont. Mutabakat / E2E iz. |

İade yeni `PairId` + yeni `CorrelationId` alır; orijinal çift silinmez. “C1’in iadesi C2” audit/dekontta bağlanır.

### Gün sonu (demo ölçeği)

1. Dönem satırlarını çek (`WalletId, CreatedAt` indeks).
2. Cüzdan net’i = `NetOf` = ekran bakiyesi.
3. Bir `CorrelationId` için iki satır toplamı 0; gateway payload aynı id.
4. Outbox: commit olduysa mesaj kaybolmaz (timeout hikâyesi); işlenmemiş satır “banka gitmedi” değil, “henüz yayınlanmadı”.
5. Admin audit arama: kullanıcı + correlation id + tarih (TASK-10).

Gerçek bankada bu, dekont referansı / end-to-end id / statement matching’tir. Bizde Guid; FAST UETR yok — **iddia etme**.

---

## 4. Demo — gerçek para değil

Ekranda `40,00 ₺` **şekil**; TCMB, muhabir, müşteri fonu yok.

| Bu | Değil |
|----|--------|
| Sahte `BankGateway` REST + SOAP | Gerçek banka / POS / 3DS / kart |
| Kapalı devre demo cüzdan | Papara / Tosla / e-para lisansı |
| TRY ölçek 2 | Canlı settlement |
| Footer: Demo — sahte banka gateway | BDDK / ödeme kuruluşu |

Yükle/çek timeout’ta ledger **kesinleşmez** (TASK-07); kuyruk kaydı kalır. Gerçek IBAN’dan para yatırma yok. Ads: “ucuz havale” / “Papara alternatif” yok (`ADS.md`).

---

## 5. Finans mülakatçısı ne sorar

Hedef kapı: Intertech, Softtech, banka/.NET, iç kontrolün yanındaki ekip. Satış pitch’i `SATIS.md`; burası **kontrol / mutabakat** soruları. TASK-06/09/11 bitmeden “kanıtladım” deme; cümle ve Domain hazır.

| Soru | Cevap |
|------|--------|
| Çift kayıt nedir? Burada nasıl? | Her harekette − ve + satır, aynı `PairId`, toplam 0. Bakiye = net. |
| Neden bakiye kolonu yok? | Kolon kaynak olursa iz ve yarış kaybolur. `UPDATE Balance` audit’siz düzeltmedir. |
| 40 ₺ havale göster. | Ali −40, Veli +40, aynı correlation. İki net’in bu çifte toplamı 0. |
| İade? | Ters çift (`Refund`); eski satırlar durur. `Balance +=` yok. |
| Çift tıklama? | Aynı `Idempotency-Key` → **409**; ikinci kesinti yok. |
| HTTP timeout, debit yazıldı mı? | Tek transaction: ya hepsi ya hiç. Outbox aynı commit; worker sonra. |
| Gün sonu nasıl kapatırsın? | Correlation id ile satır + gateway + audit. Cüzdan net = `NetOf`. Sistem çapı: her çift 0. |
| Dondurulmuş hesap? | `IsFrozen` → `CanDebit` false; gönderemez/çekemez. Gelen credit (TASK sonrası) ayrı kural. |
| Yetersiz bakiye? | `WouldGoNegative`; 4xx, satır yok. |
| Hesap planı / mizan? | Yok. Cüzdan defteri + tür enum. Genel muhasebe değil. |
| Settlement / PSP? | iyzico dosyası değiliz. Defter bizde; kart çekmiyoruz. |
| FAST / IBAN? | Yok. Sahte gateway. UETR uydurma. |
| Kuruş? | `decimal` precision 18, scale 2. `float` yok. |
| PairId vs correlation? | Çift vs uçtan uca iş. Mutabakat ikincisi. |
| Redis bakiye? | Q2 cache. Kaynak ledger. Cache ≠ kasa. |
| Bunu prod cüzdan sanayım mı? | Hayır. Demo, sahte banka, lisans yok. |

Kısa kapanış (finansçıya): *Her kuruşun +/− satırı ve correlation id’si defterde. “Bakiye güncellendi” demiyoruz.*

---

## Yasak (bu rol)

- `src/` ve `docs/AGENTS.md` yazmak
- `UPDATE Balance` “düzeltme” önermek
- Lisans / Papara / gerçek para iddiası
- SPEC ekran listesini şişirmek
