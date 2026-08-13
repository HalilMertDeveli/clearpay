# GELIR — ClearPay nasıl para getirir (araştırma)

**Kilit:** ClearPay lisanslı ödeme / e-para kuruluşu **değildir**. Ekrandaki tutarlar demo. Bu belge banka ve iş yeri sunumu içindir; hukuki tavsiye değildir. 6493 / TCMB işi avukata.

Karar: [`TARTISMA.md`](TARTISMA.md) **T-013**. Wedge: [`FARK.md`](FARK.md). Mülakat pitch: [`SATIS.md`](SATIS.md). Kapı listesi: [`IK.md`](IK.md). Para kuralları: [`SPEC.md`](SPEC.md).

Footer cümlesi sunumda da: **Demo — yükleme için sahte gateway.**

---

## 1. Bugün ne satılabilir

| Durum | Gerçek |
|-------|--------|
| Ürün | ASP.NET Core 8 mutabakat-öncelikli **cüzdan demosu** |
| Para | Sahte gateway. Müşteri fonu, IBAN, FAST, POS **yok** |
| Kanıt | TASK-03 giriş/kayıt. Ledger HTTP = TASK-06. Outbox worker = TASK-11. URL = TASK-16 |
| Satış iddiası | “Papara alternatifi / BDDK lisanslı / FAST entegre” **yasak** |

Banka veya iş yerine **şimdi** götürülecek şey: repo + 15 saniye wedge + “şu motoru kapalı devre / iç defter olarak lisanslarız.” Canlı tahsilat sözleşmesi **yok**.

---

## 2. Dört yol (biri kapalı)

TCMB, 31 Ocak 2026 / RG 33154 tebliği. Asgari özkaynak **30 Haziran 2026** itibarıyla:

| Kuruluş | Asgari özkaynak |
|---------|-----------------|
| Fatura ödemesine aracılık eden ödeme kuruluşu | **20 milyon TL** |
| Diğer ödeme kuruluşu | **40 milyon TL** |
| Elektronik para kuruluşu | **105 milyon TL** |

Buna MASAK, bilgi sistemleri denetimi, sigorta, 12–24 ay faaliyet izni eklenir. **Yol A (kendi lisansı) bu projede kapalı.**

Kalan üç yol:

| # | Yol | Kim öder | Ne satılır | Ne zaman nakit |
|---|-----|----------|------------|----------------|
| **B** | Kariyer | Intertech, Softtech, Bileşim, TAV, İGA, uni BT, sanayi .NET | Aday + repo | TASK-06/11 + LinkedIn; en hızlı |
| **C** | White-label yazılım | Lisanslı ÖK / e-para / banka inovasyon | Defter + 409 + outbox + audit (onların lisansı) | Pilot sözleşme; rakip: Firisbe, PayKit, PayStore |
| **D** | Kapalı devre (sınırlı ağ) | Üniversite, fabrika yemekhane, OSB, otel, havalimanı içi | Ön ödemeli bakiye **yalnızca o ağda** | İlk ticari konuşma; 6493 m.12/2(h) |

6493 **sınırlı ağ / ticari temsilci** istisnası: belirli mal-hizmet ağında ön ödemeli araç. Son 12 ay işlem tutarı **50 milyon TL**’yi aşınca her Ocak TCMB bildirimi. İstisna ≠ “istediğin gibi cüzdan.” IBAN’a serbest çekim e-para lisansı ister.

---

## 3. Gelir sistemleri (teklif değil, mertebe)

Rakamlar 2026 TR B2B mertebesidir; sözleşme fiyatı değildir.

### B — Kariyer

Maaş. Ürün geliri değil; SPEC’in asıl hedefi. Sunum: “satın alın” değil, “bu defteri .NET ekibinizde kurarım.” Firma filtresi [`IK.md`](IK.md).

### C — Yazılım lisansı (lisans onlarda)

| Kalem | Mertebe |
|-------|---------|
| Kurulum / entegrasyon | 80.000–250.000 TL |
| Yıllık lisans + bakım | 150.000–600.000 TL |
| veya aylık SaaS | 20.000–80.000 TL |
| İşlem payı | genelde yok (onların komisyonu kendi lisansı) |

Alıcı cümlesi: *Lisans ve müşteri fonu sizde; çift kayıt, 409, outbox, correlation id bizde.*

### D — Kapalı devre işletme

| Kalem | Mertebe |
|-------|---------|
| Kurulum (kiosk/QR sonra; Q1’de web) | 50.000–200.000 TL |
| Aylık | 12.000–50.000 TL (2.000–15.000 kullanıcı) |
| İsteğe işlem | ciro **%0,3–1,0** (küçük hacimde SaaS daha mantıklı) |
| Yıllık bakım | kurulumun %15–20 |

Örnek (hesap, vaat değil): 2.000 kişilik fabrika, kişi başı 150 ₺/ay yemek = 300.000 ₺/ay ciro. %0,5 = 1.500 ₺/ay — yetmez. Bu ölçekte **sabit 25.000–40.000 ₺/ay + kurulum** konuşulur. Yıl 1 tek tesis ~400–600 bin ₺ ciro **eğer** kapanırsa.

Komisyon modelleri (rakipler; kopyalama):

| Model | Kim kullanır | ClearPay’de |
|-------|----------------|-------------|
| % ciro (MDR) | Kart/PSP, yemek kartı üye iş yeri | Lisanssız **yapılmaz** |
| Sabit SaaS | Kampüs kart, fabrika | **Uygun** (D) |
| Kurulum + bakım | Banka ISV, white-label | **Uygun** (C) |
| Float faizi | E-para bakiyesi | Lisans ister; **yok** |
| Ön ödemeli kırık bakiye | Kapalı devre | Etik/hukuk; vaat etme |

---

## 4. Kime ne söylenir

### Banka / banka yazılım evi (Intertech, Softtech, Bileşim)

**15 saniye**

> ClearPay lisanslı cüzdan değil. FAST ve Papara’nın yerini almıyoruz. Personel yemek, kampüs ortaklığı, havalimanı içi gibi **kapalı devrede** her kuruşun +/− satırı ve correlation id’si sizin defterinizde kalır. Aynı niyet **409**; timeout **outbox**.

**İsteme:** çekirdek banking, FAST katılımcılığı, kart acquiring.

**İste:** inovasyon / iç kontrol / .NET ekibi; “iç defter PoC” veya işe alım.

### İş yeri (üniversite, OSB, yemekhane, otel)

**15 saniye**

> Yemekhane ve kampüs bakiyeniz bugün kara kutu. 17,40 ₺ kim kesti, ikinci kez kesildi mi, timeout yuttu mu — satır satır. Para sizin ağınızda kalır; IBAN’a çekim yok. Demo; canlı tahsilat avukat + sınırlı ağ teyidinden sonra.

**İsteme:** “Papara gibi her yerde geçer.”

**İste:** tek tesis pilot (yemekhane veya kantin), muhasebe ile gün sonu mutabakat.

---

## 5. Lokasyonlar (sunum rotası)

Kapıya soğuk girme: LinkedIn / İK / satın alma e-postası. Adres = toplantı yeri, “içeri yürüyün” değil.

### İstanbul — banka yazılım (Gün 1 Avrupa)

| Yer | Adres (kamuya açık) | Ne konuşulur |
|-----|---------------------|--------------|
| Softtech Levent | Levent Mah. Meltem Sk. İş Kuleleri Kule 3 No:14 Kat:12, Beşiktaş | .NET cüzdan / defter; işe alım veya PoC |
| Softtech Maslak | Reşitpaşa Mah. Katar Cad. İTÜ Teknokent ARI-3 Kat:4–5, Sarıyer | Aynı; Ar-Ge |
| Intertech Esentepe | Büyükdere Cad. No:141 Kat:6, Şişli | Banka .NET; iç defter |
| Intertech Vadi İstanbul | Ayazağa Mah. Kemerburgaz Cad. Vadi İstanbul Park 7A Blok Kat:14, Sarıyer | Aynı |

### İstanbul — banka yazılım (Gün 2 Anadolu + Ataşehir)

| Yer | Adres | Ne konuşulur |
|-----|-------|--------------|
| Intertech Ar-Ge Pendik | Sanayi Mah. Teknopark Blv. No:1/3C, Kurtköy–Pendik 34906 | Ana kampüs; +90 216 664 20 00 |
| Bileşim ADK | Yenişehir Mah. Çağlayan Sk. No:16, Ataşehir | Dağıtım kanalları / ödeme altyapısı .NET |

### İstanbul — işletme / havalimanı

| Yer | Adres / bölge | Ne konuşulur |
|-----|----------------|--------------|
| İTÜ Ayazağa | Maslak / Sarıyer kampüs | Öğrenci işleri + BT: yemekhane cüzdanı |
| Boğaziçi, Yıldız, Marmara | ilgili kampüs idari | Aynı; rakipler Kampüs Kart, SoliClub, Etisan |
| Gebze GOSB / TOSB | Gebze–Kocaeli OSB | Fabrika yemekhane, servis kartı |
| İGA | Tayakadın Mah. Ulubatlı Hasan Cad. No:255, Arnavutköy (ticari adres); GM ofis terminal içinde (2026) | Havalimanı içi kapalı devre; İK .NET |
| TAV | Sabiha Gökçen işletme / TAV ofisleri | Aynı; SPEC kapısı |
| İkitelli / Dudullu OSB | Başakşehir / Ümraniye | KOBİ yemekhane |

### Ankara / İzmir

| Yer | Adres | Ne konuşulur |
|-----|-------|--------------|
| Intertech Bilkent Cyberpark | Üniversiteler Mah. 1606. Cad. Blok A Kat:8 No:801, Çankaya | Banka .NET |
| Softtech Hacettepe Teknokent | Üniversiteler Mah. 1596. Cad. Safir C Blok Kat:10, Ankara | Aynı |
| ODTÜ / Hacettepe / Ankara Üni. | kampüs BT | Kapalı devre; rakip Kampüs Kart (Çankaya) |
| Intertech İzmir | Adatepe Mah. Doğuş Cad. No:207 Z/1, Buca | Banka .NET |
| İzmir Atatürk OSB | Çiğli | Fabrika yemekhane |

Düzenleyici (müşteri değil): TCMB ödeme sistemleri mevzuatı, TÖDEB üyeleri, FinTech İstanbul etkinlikleri.

---

## 6. Rakipler (dürüst)

| Oyuncu | Ne satar | ClearPay farkı |
|--------|----------|----------------|
| Papara, Tosla, Paycell, ininal | Lisanslı e-para | Yerlerini **almıyoruz** |
| iyzico, PayTR | Kart PSP | Kart çekmiyoruz |
| Firisbe, PayKit, PayStore, Kobaküs | White-label ödeme / cüzdan | Onlar tam yığın; biz **mutabakat kaması** |
| Multinet, Pluxee, Edenred, Setcard | Yemek kartı ağı | Üye iş yeri ağımız yok |
| Kampüs Kart, SoliClub, Etisan, Bink, inventiv | Kampüs / kapalı devre | Geçiş+POS onlar; defter izi biz |

İlk satış “onların yerine geçmek” değil: **iç defter + 409 + outbox** katmanı veya tek tesis web cüzdanı (Q1 ekranları). Satıcı paneli SPEC’te yok (Q2).

---

## 7. Yapılacaklar (sıra)

Kod TASK sırası değişmez (`TASKS.md`). Bu liste **ticari**.

1. **Şimdi:** Bankaya “lisanslı cüzdan” deme. 15s metni ezberle. Kapı listesi `IK.md`.
2. **TASK-06 + TASK-11:** 409 ve outbox kanıtı olmadan “satın alın” yok.
3. **TASK-16:** HTTPS demo URL; sunumda localhost yok.
4. **İlk ticari konuşma:** bir üniversite BT veya bir OSB yemekhane — sınırlı ağ. Avukat: 6493 m.12/2(h), 50 milyon TL bildirim.
5. **Şirket:** fatura keseceksen Ltd/şahıs; ajan şirket açmaz.
6. **Q2 (ayrı karar):** satıcı ekranı, QR kasa — SPEC’e 9. ekran **kullanıcı onayı** ile.
7. **Asla şimdi:** ödeme kuruluşu başvurusu, Ads “ucuz havale”, gerçek IBAN.

---

## 8. Kaynaklar (Ağustos 2026)

- TCMB asgari özkaynak tebliği: RG 31.01.2026 / 33154; yürürlük 30.06.2026 (20 / 40 / 105 milyon TL).
- 6493 m.12/2(b) ticari temsilci, (h) sınırlı ağ; 50 milyon TL / 12 ay → Ocak TCMB bildirimi (FinTech İstanbul, TCMB rehber).
- Softtech / Intertech / Bileşim iletişim sayfaları (adresler kamuya açık; teyit et).
- Kapalı devre oyuncular: Multinet Up, inventiv, Kampüs Kart, SoliClub, Bink, Etisan.
- White-label: Firisbe, PayKit, PayStore.

---

## Yasak

- `src/` ve SPEC ekran listesini bu belgeyle şişirmek
- BDDK / TCMB lisansı, FAST, Papara rakibi
- Maaş bandı uydurma (`IK.md`)
- Hukuki tavsiye gibi yazmak
