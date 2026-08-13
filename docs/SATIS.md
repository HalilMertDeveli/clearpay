# SATIS — ClearPay nasıl anlatılır

**Tercih sebebi: her kuruşun +/− satırı ve correlation id’si sizin defterinizde kalır — Papara, iyzico, FAST ve kart “bakiye güncellendi” der.**

Lisanslı cüzdan değiliz. Papara’nın tüketici yerini almayız. Sahte banka uygulaması değiliz (WePay benzeri cüzdan sitesi). Hedef: hiring manager, GitHub, demo copy. Kaynak: [`docs/FARK.md`](FARK.md). Footer: **Demo — yükleme için sahte gateway.** TASK-06/11 bitmeden “kanıtladım” yok.

---

## 15 saniye

> ClearPay, ASP.NET Core 8 **WePay benzeri cüzdan demosu** (mutabakat-öncelikli defter). Papara/iyzico/FAST değil: her kuruş +/− satır ve correlation id, aynı istek **409**, timeout **outbox**’ta kaybolmaz. Lisans yok; yükle/çek için sahte **BankGateway** REST+SOAP. Sahte banka uygulaması değil. Intertech/Softtech mülakatında anlatılacak repo.

Daha kısa: *Defter sizin. Bakiye güncellendi demiyoruz. Cüzdan sitesi, sahte banka değil. Demo.*

---

## 2 dakika

1. **Wedge:** Kim parayı açıklamak zorundaysa (finans, iç kontrol, banka .NET) kara kutu istemez. Papara defteri onlarda; iyzico settlement; FAST banka dekontu; CRUD `Balance -=`.
2. **Ne:** Razor + API, .NET 8, SQL Server. WePay benzeri cüzdan: havale, yükle/çek (sahte **BankGateway**), admin dondur + audit. Şube / BankaX yok.
3. **Motor:** Çift kayıt; `Idempotency-Key` → 409; outbox aynı transaction; REST ve SOAP aynı sözleşme; correlation id dekontta.
4. **Dürüstlük:** “Papara’yı geçtik” yok. “Sahte bankayız” yok. Tüketici e-para lisans ister. Biz kapalı devre **açıklanabilir para** gösteriyoruz. FAST/POS yok.
5. **Kanıt:** xUnit 409 + invariant, Swagger, Compose, Azure (TASK-16).

Ezber: **409 / transaction / outbox.**

---

## CV

- Built **ClearPay**, an ASP.NET Core 8 **WePay-like wallet** demo: double-entry ledger (balance = net, not `UPDATE Balance`), idempotent P2P (**409**), outbox in the same SQL transaction. **Not a fake bank app.**
- Mock **BankGateway** (REST + SOAP) for top-up/withdraw timeout/retry only; freeze, audit, Serilog correlation. **Not a licensed wallet.**

Türkçe: ClearPay — WePay benzeri cüzdan demosu; çift kayıt, 409, outbox. Sahte banka uygulaması değil; lisanslı kuruluş değil.

Kullanma: Papara klonu, sahte banka uygulaması, FAST entegrasyonu, BDDK, üretim cüzdanı.

---

## README / site

README ilk 5 satır: disclaimer + 15s. Coder uydurmaz.

| Yer | Metin |
|-----|--------|
| Footer | **Demo — yükleme için sahte gateway** |
| Sonra (Coder) | **Neden ClearPay** — her kuruş defterde |
| Ads | `docs/ADS.md` — harcama yok; “Papara alternatif” yasak |

---

## Q&A

| Soru | Cevap |
|------|--------|
| Neden seni Papara yerine seçeyim? | Tüketici parasında seçme. İç defter + izlenebilir kuruş istiyorsan Papara senin SP’n değil. |
| iyzico? | Onlar kart çeker. Ledger yok. |
| FAST? | Bağlı değiliz. Outbox + 409 timeout hikâyesi. |
| 409 neden 200 replay değil? | İkinci kesinti yok; niyet işlendi. |
