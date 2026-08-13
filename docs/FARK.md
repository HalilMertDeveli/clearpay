# FARK — neden ClearPay (mutabakat, pazar değil)

**Bizi tercih sebebi: her kuruşun +/− satırı ve correlation id’si sizin defterinizde kalır — Papara, iyzico, FAST ve kart “bakiye güncellendi” der.**

Tüketici parasında Papara’nın yerini **almayız** (lisans yok). Wedge gerçek: parayı **açıklamak** zorunda olan taraf kara kutu istemez. Demo: **sahte banka gateway.** FAST katılımcısı, POS, BDDK iddiası yok.

---

## Tercih sürücüsü (tek)

Üç aday vardı. **Mutabakat-öncelikli defter** kaldı.

| Aday | Neden değil / neden evet |
|------|--------------------------|
| Kapalı devre kampüs/yemek cüzdanı | Gerçek iş. Ekran yok (pazaryeri/yemek Q2). Papara GTM’e kayar. *Sahne:* iç TL, freeze, audit — ürün iddiası değil. |
| Developer-first ledger API | Mülakat paketi (409, outbox, SOAP). FAST/kart’ın *yerine* ödeme yöntemi değil. |
| **Mutabakat-öncelikli defter** | **Seçildi.** Kodda kilitli (ledger, 409, outbox, correlation, freeze). Papara/iyzico/FAST/3DS/CRUD’un vermediği şey: **sizin** +/− satırınız. |

### Kimin sorunu

Finans, iç kontrol, kampüs/şirket operasyonu, banka/.NET ekibi — “bu 17,40 ₺ nereye gitti, kim kesti, tekrar kesildi mi?”

### Bugün ne kullanıyorlar

- **Papara / Tosla / Paycell / ininal** — lisanslı e-para. Para *onların* defterinde. Siz donduramaz, satır satır muteber kılmazsınız.
- **iyzico / PayTR / Stripe** — satıcı kart çeker (3DS, taksit, settlement). Ledger gizli; mutabakat dosyası ≠ sizin cüzdan invariant’ınız.
- **Banka FAST / EFT** — TCMB rayı, saniyeler, 7/24 FAST. Dekont bankada. Sizin `LedgerEntry` çiftiniz yok; freeze yok; timeout sizin outbox’ınız değil.
- **Kart 3DS POS** — ACS, OTP, PCI. Callback çorbası. İç avans/yemek için yanlış araç.
- **Öğrenci CRUD** — `Balance -= amount`. Çift tıklama iki keser. Timeout yutar.

### Neden ClearPay

Kapalı devre **açıklanabilir para**: her harekette + ve −; bakiye = ledger net; aynı `Idempotency-Key` → **409**; outbox ledger ile **aynı SQL transaction**; dekontta correlation id; admin freeze; sahte banka REST **ve** SOAP (aynı sözleşme).

### Ne değiliz

Lisanslı tüketici cüzdanı. Papara rakibi. FAST/BOA. Sanal POS / 3DS. Satıcı pazaryeri. “Onlardan ucuzuz.”

---

## Onlar ne / biz ne / mülakatta cümle

| Onlar | Onlar ne | Biz ne | Mülakatta |
|-------|----------|--------|-----------|
| Papara, Tosla, Paycell, ininal | Lisanslı cüzdan, gerçek bakiye, kart/IBAN/P2P | Demo kapalı devre; defter **bizde** (çift kayıt) | Onlar ürün; ben izlenebilir kuruş gösteriyorum. Yerlerini almıyorum. |
| iyzico, PayTR, Stripe | Checkout PSP: 3DS, taksit, settlement | PSP değiliz; kart çekmiyoruz | Ledger’ı gizlerler. Ben her satırı gösterebilirim. |
| Banka FAST / EFT | TCMB anlık/EFT; IBAN; banka dekontu | Sahte gateway; FAST yok | Timeout’ta outbox + 409. FAST lisansı yok. |
| Kart 3DS POS | Acquiring, ACS, PCI | Yok | İç bakiye kart rayına çıkmaz. |
| ASP.NET CRUD | `UPDATE Balance` | `LedgerEntry` +/−, unique idempotency | Bakiye güncellendi demiyoruz; defter net. |

---

## Kategori (kamu, kısa)

**Cüzdan:** Papara bağımsız “mini banka”; Tosla AkÖde/Akbank, genç P2P; Paycell Turkcell (fatura/QR); ininal TCMB/BDDK e-para, ön ödemeli kart + cüzdan. Hepsi gerçek para.

**Checkout:** iyzico (PayU) developer/pazaryeri; PayTR iframe/KOBİ; Stripe TR (2024, TRY) — taksit zayıf, kart PSP. ClearPay sanal POS değil.

**FAST:** TCMB, 7/24, saniyeler. Ekran hissi (alıcı, tutar, dekont) SPEC’ten; mesaj protokolü yok.

**CRUD:** portföy kırılma noktası = para. Biz orayı kasten gösteriyoruz.

---

## Motor (SPEC; TASK-04…11 dolar)

```
Idempotency-Key → aynıysa 409
tek SQL tx: −/+ LedgerEntry, Transfer, Idempotency, Audit, Outbox
commit → Hangfire outbox → BankGateway REST|SOAP
timeout: kayıt durur, worker dener; ikinci debit yok
```

1. Çift kayıt (`PairId`); iade = ters kayıt; `UPDATE Balance` yasak
2. 409 = niyet işlendi
3. Outbox dual-write’ı keser
4. REST + SOAP, aynı timeout sözleşmesi
5. Audit + freeze + correlation
6. Dürüst demo etiketi (`SoftwareApplication`, banka schema yok)

---

## One-liner (Designer / SEO)

1. Her kuruş defterde — bakiye güncellendi yok.
2. Aynı havale 409; çift kesinti yok.
3. Timeout ödemeyi silmez (outbox).
4. Sahte banka REST ve SOAP.
5. Dondur, audit, correlation id.
6. Demo — sahte banka gateway.

Footer sonra: *Her kuruş defterde. Demo — sahte banka gateway.*

---

## Söyleme

FAST entegrasyonu · gerçek POS/3DS · pazaryeri · Kafka · “Papara kadar güvenli / Papara alternatif”
