# TASARIM — ClearPay

Kaynak: `docs/SPEC.md` ekran listesi. Piksel Figma yok; Razor mevcut sınıflara yakın. Coder `site.css` sahibi; bu belge + `wwwroot/css/brand.css` (ek token). Layout linki Coder HANDOFF’ta.

## Görsel bar (T-040 + T-053 — mobil bankacılık, ölçülü derinlik)

| Kural | Değer |
|-------|--------|
| Navy | `#1B2A4A` (mürekkep, sidebar, hero bant, birincil buton) |
| Teal | metin/vurgu `#0F766E`; parlak `#14B8A6` (aktif nav/sekme, gelen tutar `.amount-in`) |
| Ilık | `#C2782A` (dondurulmuş rozet; birincil CTA değil) |
| Zemin | kart `#FFFFFF`; sayfa wash `#E8EEF5` (düz; radial yok) |
| Muted | `#5C6B86` |
| Çizgi | `#D8DEE8` |
| Tip | Inter 400/500/600/700/800 |
| Radius | kart 12px; cüzdan beyaz tabaka `--radius-lg` 20px; sekme hapı `--radius-pill`; buton/input 8px |
| Elevation | **ölçülü var (T-053):** `--elev-1` kart/panel, `--elev-2` alt sekme çubuğu, `--elev-3` bakiye kartı + beyaz tabaka + auth kart. 1px `--line` durur; parlama/neon yok |
| Gradient | **bakiye kartı** + T-055 `.demo-card` (Yükle/Çek sahte kart yüzü) `--hero-grad` (`#24365C` → `#1B2A4A`). Buton, sekme, zemin düz |
| Cam | yok; auth kart opak `#fff` |
| Emoji | yok |
| Bootstrap | yok |

Yaratıcılık: hiyerarşi, boş durum + CTA, wordmark, form ritmi, kısa hareket. Landing / kampanya / şube yok.

**One-liner:** ClearPay — demo dijital cüzdan (WePay benzeri).

**Ürün çerçevesi (T-015 + T-072):** WePay benzeri **dijital cüzdan / pay**. Kicker **Cüzdan**, tagline **Demo dijital cüzdan**. Girişli kabuk internet-şube *düzeni* (masthead + sol menü); YK/Worldcard marka kopyası değil. `IBankGateway` yalnızca yükle/çek stub.

## Tipografi

- Sayfa kicker: 0.75rem, 600, letter-spacing `0.08em`, uppercase, muted
- `h1.page-title`: 1.85rem / 700 / tracking `-0.03em` — tek h1 = ekran adı (wordmark h1 değil)
- Lede: muted, title’dan sonra tek satır
- Para: `font-variant-numeric: tabular-nums`; özet bakiyesi ~2.6rem / 800 (hero’da beyaz)
- Buton: 600, min-height 2.75rem, navy dolgu (hover teal çerçeve + dolgu) veya ghost (şeffaf + navy; hover teal çerçeve)

## Wordmark

Kare marka: 2rem, 8px radius, düz teal `#0F766E`, içinde beyaz **C**.  
Launcher (T-084): navy `#1B2A4A` kare, beyaz geometrik **C**, ince teal halka; YK/Papara/World yok.  
İsim: **ClearPay**, tracking `0.04em`, 700. Masthead’de beyaz; auth’ta navy.  
Tagline (yalnız auth): `Demo dijital cüzdan`.

## Layout

Alipay tüketici ev *yapısı* (T-038): renk MARKA navy, ürün **ClearPay**. Alipay logo/QR yok.

```
[ sidebar 248px navy, ikon+metin ] [ topbar ince | içerik max 560px ortalı | footer ]
```

İçerik masaüstünde **ortalanmış dar kolon** (`.content` 560px) — uygulama hissi. Hareketler ve Admin `.content--wide` (1040px) ile geniş kalır; sayfa `ViewData["Wide"] = true` verir.

Sol menü sırası sabit: Özet, Havale, Yükle/Çek, Hareketler; Admin role.  
Footer her yerde: **Demo — yükleme için sahte gateway**

## Mobil (≤800px) — T-053 alt sekme çubuğu

Sidebar **gizlenir**; hamburger ve backdrop yok (iki gezinme aynı anda olmaz).

```
[ masthead: wordmark | kullanıcı | çıkış ]
[ içerik, tek kolon kart yığını       ]
[ footer: demo notu + dil seçici      ]
[ tabbar sabit: Özet Havale Yükle Hareketler (Admin) ]
```

- `.tabbar`: `position: fixed`, alt sıfır, `--tabbar-h` 64px + `--safe-bottom`, üstte 1px çizgi + `--elev-2`. Aktif sekme teal.
- İkonlar sidebar glifleridir; beyaz zeminde `currentColor` alır (`.tabbar .nav-ico--*`).
- Dil seçici mobilde **footer**’a taşınır (`.footer-lang`); topbar kalabalıklaşmaz.
- `.page-footer` alt boşluğu tabbar yüksekliği kadar artar.
- İşlem tabloları `.data-cards` ile karta döner (`--mv` özet 4 hücre, `--tx` hareketler 7 hücre); masaüstünde tablo kalır. Admin tabloları `.table-scroll` ile yatay kaydırılır.
- Yükle | Çek `:target` sekmesi (`.tabset` + `.tab-switch`); `#yukle` / `#cek` çapaları korunur, JS yok.
- Birincil form eylemi `.form-actions--sticky` ile tabbar üstünde yapışır.
- Auth kart tam genişlik, yatay padding 1.25rem. Form satırları (`field-row`, `split-2`) tek kolon ≤900px.

## Kompozisyonlar

### Giriş / kayıt (`_AuthLayout`)

T-054: Yapı Kredi *düzeni* (sol hero + sağ panel), **marka kopyası değil**. Üst navy şerit (wordmark + dil). `.auth-stage` iki kolon: sol `.auth-hero` (`--hero-grad`, geometrik `.auth-orb`, kicker ClearPay + `Demo cüzdan` + footer cümlesi); sağ `.auth-shell` mevcut kart. Kart max 420px, 12px radius, 1px `--line` + `--elev-3`. Sıra kartta: `.auth-mark` → tagline → `h1` → lede → form → sosyal → switch. Auth footer kartın dışında: **Demo — yükleme için sahte gateway**. Google/Apple silinmez.

Motion: vendored `anime.min.js` (3.2.2) + `auth-hero.js` yalnız bu layout. Tek sefer stagger 180–240ms. `prefers-reduced-motion` → JS no-op. npm/GSAP yok. ≤800px hero üstte kısa, kart altta.

### Özet (`/`)

Internet-şube ev (T-072): hesap kartı + hızlı işlem karoları + beyaz tabaka. Sparkline / Worldcard yok.

1. `dash-grid`: `balance-hero.wallet-band` (`--hero-grad` + `--elev-3`) kicker **Cüzdan** + `h1` Özet + selamlama; kullanılabilir bakiye **xl / 800**; durum (Aktif teal / Dondurulmuş ılık). Yanında `.ops-rail` **Hızlı işlemler**: `.action-grid` 2×2 beyaz karo — **Gönder** `/havale`, **Yükle** `/yukle-cek#yukle`, **Çek** `#cek`, **Hareketler** `/hareketler`. İkonlar CSS geometri (emoji / YK / Alipay QR yok).
2. `.wallet-sheet` (beyaz, radius `--radius-lg` 20px, 1px çizgi + `--elev-3`): iki ay özeti (giden | gelen) + **Son hareketler** + “Tümünü gör”.

Boş hareket: `empty-block` — `empty-mark` + başlık + hint + CTA (Havale / Yükle).

### Havale (`/havale`)

Kicker **Havale**. Panel içinde: kalan bakiye üst şerit (label sol, tutar sağ, tabular) → alıcı e-posta → tutar | açıklama → ipucu (409) → **Gönder** (inceleme) + **İptal**. Gönder, bakiye 0 iken disabled. Onay: `.confirm-sheet` alıcı/tutar/sonraki bakiye + **Onayla ve gönder**. Tutar alanı görsel olarak para; süs ikon yok.

### Yükle / çek, hareketler, dekont, admin

SPEC sırası. İki kolon Yükle | Çek (`split-2`); hareketler Başlangıç|Bitiş|Tür + tablo; boş tablo `empty-block` + CTA. Dekont tek işlem, correlation id monospace + kopyala/yazdır (T-056); ****son4 satırı T-057. Başarılı para hareketi fişe gider; havale replay de fişe. Admin: tablo + dondur/**çöz** / kuyruk; süs dashboard yok.

T-055: kayıtlı kart **ekran 5 paneli** (`#kart`), 9. ekran yok. `.demo-card` navy `--hero-grad` + `--elev-3` + `--radius-lg`; son 4 hane tabular (`•••• •••• •••• 1234`); `.card-chip` hap. Tam PAN/CVV yok; Visa/Mastercard logosu yok.

## Boş durum (zorunlu)

```
.empty-block
  .empty-mark    — CSS kare (emoji/illüstrasyon yok)
  .empty-title   — ne yok
  .empty-hint    — sonraki eylem
  .empty-actions — Havale gönder + Yükle (özet); hareketler sayfasında isteğe link
```

Teal-tint wash, 12px radius, ortalama; padding ~2.25rem. Raster illüstrasyon yok.

## Form

Label 0.875rem / 500. Input 2.75rem, 1px `--line`, focus: 2px navy outline, offset 0. Hata `--danger` `#8A1F1F`, özet kutu 1px danger çerçeve. Disabled opacity 0.45.

## brand.css

Ek katman (site.css sonrası): teal/ılık token, derinlik token’ları (`--elev-1..3`, `--hero-grad`, `--radius-lg`, `--radius-pill`, `--tabbar-h`, `--safe-bottom`), wallet-band + action-grid, `empty-mark`, tabbar glif renkleri, `.amount-in`, sosyal kancalar (`.auth-divider`, `.btn-social`, `.btn-google`, `.btn-apple`). `site.css` iskelet durur; kopyalanmaz. Her iki layout: `site.css` → `brand.css` → `motion.css`.

## Motion (T-040 — CSS, kütüphane yok)

OWN: `wwwroot/css/motion.css` (+ kısa `site.js` count-up). npm / GSAP / Framer yok. Animasyon **gradient değil**.

Süre **150–250ms**. Bounce, confetti, `scale(1.05)`, rainbow, sonsuz orb/shimmer/pulse **yok**.

| Hedef | Ne | Süre |
|--------|----|------|
| `.auth-card`, `.balance-hero`, `.panel`, `.wallet-sheet` | giriş: opacity + `translateY(8px→0)` | 180–220ms |
| `.btn`, `.nav-link`, `.lang-btn` | color / border / background | 160ms |
| `.stat-card`, `.data-table tbody tr` | hover: zemin + çerçeve (gölge yok) | 160ms |
| `.tab-link`, `.tab-switch a` | renk / zemin | 160ms |
| `.tabbar`, `.data-cards tbody tr` | giriş: opacity + `translateY` | 160–180ms |
| count-up | bakiye / ay tutar (bir kez) | ~240ms |

```css
@media (prefers-reduced-motion: reduce) {
  .sidebar, .nav-backdrop, .btn, .stat-card, .panel,
  .balance-hero, .auth-card, .nav-link, .field-input, .lang-btn {
    transition: none;
    animation: none;
    transform: none;
  }
}
```

Token: `--motion: 180ms ease;`

## Tarifler (Coder — Razor ezme, CSS/sınıf)

Mevcut sınıflar: `auth-card`, `balance-hero`, `stack-form`, `data-table`, `empty-title`, `empty-hint`. Yeni sayfa yok. Tip ölçeği 16px kök.

### Tip ölçeği (sabit)

| Rol | Boyut | Ağırlık | Satır / ekstra |
|-----|--------|---------|----------------|
| Kicker `.page-kicker` | 0.75rem | 600 | tracking 0.08em, uppercase |
| `h1.page-title` | 1.85rem | 700 | tracking −0.03em; margin 0 0 0.5rem |
| `.lede` | 1rem | 400 | muted; margin 0 0 1.5rem |
| `.stat-label` | 0.8rem | 400 | muted; margin 0 0 0.4rem |
| `.stat-value-xl` | 2.85rem (≤800px: 2.15rem) | 800 | tabular-nums; tracking −0.03em |
| `.stat-value` | 1.35rem | 700 | tabular-nums |
| `.field-label` | 0.875rem | 500 | margin-bottom 0.35rem |
| `.field-input` | 1rem | 400 | height 2.75rem |
| `.field-hint` / `.empty-hint` | 0.875rem | 400 | muted |
| `.empty-title` | 1rem | 600 | navy; margin 0 0 0.35rem |
| `th` | 0.75rem | 600 | muted |
| `td` | 0.925rem | 400 | |
| `.btn` | 1rem | 600 | min-height 2.75rem; padding 0.65rem 1.15rem |

### 1) Giriş (`_AuthLayout` + Login)

Alipay-benzeri hızlı giriş (ClearPay; banka portalı değil).

```
auth-top    navy şerit; wordmark sol, dil sağ; padding 0.85rem 1.5rem
auth-shell  padding 2.5rem 1.25rem; zemin wash (radial carnival yok)
auth-card   max-width 420px; padding 2.5rem 2.15rem 2.15rem
            border 1px --line; radius 12px; opak #fff + --elev-3 (cam yok)
auth-mark   2.5rem teal kare, beyaz C; margin 0 0 0.85rem
tagline     0.85rem muted, margin 0 0 1.25rem; metin: Demo cüzdan
h1          Giriş — 1.75rem; lede 1rem, margin-bottom 0
auth-form   margin-top 1.5rem
.auth-tabs  her zaman görünür (Yükle `.tab-switch` değil — o masaüstünde gizli)
            2 kolon hap; E-posta | TC (demo)
.field      margin-bottom 1.1rem
.btn-block  width 100%; margin-top 0.25rem
auth-switch margin 1.35rem 0 0; 0.9rem
auth-footer kart DIŞI, 0.75rem muted, margin-top 1.35rem
            metin: Demo — yükleme için sahte gateway
```

Kayıt aynı kart ritmi; ad, e-posta, **telefon**, Bireysel/Kurumsal (`.choice-row` iki radio), şifre + tekrar. Hint şifre altında 0.35rem. Boş durum yok (form). Hata: `.validation-summary` 0.75rem 0.85rem padding, 1px danger.

T-088: TC sekmesi Mernis değil (seed `10000000146`). Flutter dil şeridi auth + çekmece; YK/Papara yok.

### 2) Özet hero (`/` `.wallet-home`)

Alipay ev yapısı: bant + 4 daire + örtüşen beyaz tabaka. Sparkline yok.

```
content        padding 1.5rem 2rem 2.5rem; max-width 560px; margin-inline auto
wallet-band    padding 1.75rem 1.6rem 2.25rem; radius 12px 12px 0 0
               --hero-grad; renk beyaz; --elev-3
kicker/title   bant içinde; kicker rgba white .72; h1 beyaz 1.35rem
stat-value-xl  2.85rem / 800 tabular (≤800px: 2.15rem)
action-grid    4 kolon; margin-top 1.35rem
action-ico     3rem daire, beyaz; CSS glyph navy (emoji yok)
wallet-sheet   margin-top −1.25rem; padding 1.5rem 1.35rem
               radius --radius-lg 20px; beyaz; 1px --line + --elev-3
stat-row       2 kolon, gap 0.75rem; iç hücre wash, radius 12px, --elev-1
son hareketler .data-table.data-cards.data-cards--mv
               gelen tutar .amount-in (teal); ≤800px kart satırı
panel/activity sheet içinde; ayrı dış çerçeve yok
```

**Boş (son hareketler):** `td.empty-cell` colspan 4; padding 2.25rem 1rem; ortalama.

- `.empty-mark`: CSS kare
- `.empty-title`: Henüz hareket yok
- `.empty-hint`: İlk havaleniz veya yüklemeniz burada görünür.
- `.empty-actions`: Havale gönder + Yükle

Raster yok. Mobil: 4 ızgara durur (küçük etiket); xl 2.15rem.

### 3) Havale formu (`/havale` `.stack-form`)

P2P cüzdan gönderimi.

```
kicker Transfer; h1 Havale; lede 0 0 1.75rem
panel            padding 1.25rem 1.35rem
.remain          flex; label sol, tutar sağ tabular
                 padding-bottom 1rem; margin-bottom 1.25rem
                 border-bottom 1px --line
.field           alıcı full width; margin-bottom 1.1rem
.field-row       tutar | açıklama; gap 1rem; ≤900px tek kolon
#tutar           inputmode decimal; tabular-nums (brand.css)
.field-hint      margin 0.35rem 0 0; 0.875rem
.form-actions    margin 1.25rem 0 0; gap 0.6rem
                 Gönder dolu → onay adımı (bakiye 0 → disabled); İptal ghost
```

Boş alıcı/tutar: native validation / kırmızı özet; ayrı empty-block yok. Bakiye 0 ipucu hint’te kalır.

### 4) Hareket tablosu (`/hareketler` `.data-table`)

Cüzdan geçmişi. TASK-09 doldurur; iskelet bu ritim.

```
kicker Geçmiş; h1 Hareketler; lede 0 0 1.75rem
filtre paneli     padding 1.25rem 1.35rem; margin-bottom 1rem
.filter-row       Başlangıç | Bitiş | Tür + Filtrele; gap 1rem; align end
                  ≤900px tek kolon
tablo paneli      padding 1.25rem 1.35rem (veya tablo kenarsız, th/td yatay 0.6rem)
th                padding 0.5rem 0.6rem 0.75rem; border-bottom 1px
td                padding 0.85rem 0.6rem; border-bottom 1px
tutar kolonu      tabular-nums; sağa hizalı (Coder: th/td.num)
işlem no / corr.  0.85rem; isteğe font-family ui-monospace
```

**Boş dönem:** colspan 6 (veya kolon sayısı); aynı empty çifti:

- `.empty-title`: Bu dönemde hareket bulunmuyor
- `.empty-hint`: Tarih veya tür filtresini genişletin; ya da ilk havaleyi Özet’ten gönderin.
- `.empty-actions`: Havale gönder + Yükle

Dekont satırı (TASK-09): correlation id monospace, 0.8rem muted.

Hareketler sayfası `ViewData["Wide"] = true` (geniş kolon). ≤800px tablo `.data-cards--tx` ile karta döner: tür (kalın) + karşı taraf + tarih solda; tutar (kalın, gelen teal) + durum + Dekont sağda; işlem no gizlenir (dekontta durur).

### 5) Alt sekme çubuğu (`_Layout` `.tabbar`, ≤800px)

```
tabbar      fixed alt; grid, kolonlar eşit; min-height --tabbar-h 64px
            padding-bottom --safe-bottom; beyaz; 1px üst çizgi + --elev-2
tab-link    dikey: ikon üstte, etiket 0.68rem/600 altta; muted
            aktif → teal (rota eşleşmesi, sidebar ile aynı mantık)
nav-ico     sidebar glifi; beyaz zeminde currentColor
sıra        Özet, Havale, Yükle/Çek, Hareketler (+ Admin role)
```

### 6) Dekont fişi (`/dekont/{id}`)

```
receipt-amount  ortalı; stat-label + stat-value-xl 2.3rem + durum rozeti
                alt 1px --line
receipt-row     dt sol muted 0.85rem | dd sağ 600; 1px dashed ayraç
                son satırlar ayraçsız; correlation id .mono + Kopyala; ****son4 varsa Hesap/kart
form-actions    PDF indir (dolu) + Yazdır (ghost) + Geri; yazdırınca chrome gizlenir
```

Başarılı havale / yükle / çek TempData flash ile **bu fişe** yönlenir (Index değil). `@media print`: sidebar, tabbar, topbar, footer, flash, form-actions yok; yalnız fiş.

T-056: `data-copy` / `data-print` (`site.js`); clipboard + `window.print`. T-079: `handler=Pdf` aynı `correlationId` (yönetilen PDF 1.4); 9. ekran yok.

### 7) Giriş — Beni hatırla (T-056)

IdentityCourse / TaskManagement `RememberMe`. Ekran 1, checkbox `.field-check` şifre altında. `PasswordSignInAsync(isPersistent)`. Google/Apple aynı. Cookie SameSite **Lax** (Strict OAuth’u kırar — reddedildi).

### 8) Erişim yok (hata kromu, SPEC ekran değil)

IdentityCourse AccessDenied. `/erisim-yok` — `empty-block` + Özet CTA. Admin’e yetkisiz Musteri. 9. ekran / satıcı paneli değil.

### 9) Form busy (T-056)

BankApp çift POST yoktu ve `UPDATE Balance` yaptı; burada tersi: POST form `aria-busy` + `.is-busy` (pointer-events). Submit butonu `disabled` **yapılmaz** (Razor handler adı kaybolmasın). Dil seçici / çıkış hariç. 409 sunucuda durur.

Havale **Gönder** bakiye 0 veya dondurulmuşken `disabled` (TASARIM maddesi; T-056 tamamlar). Gönder = inceleme (`handler=Review`); ikinci adım **Onayla ve gönder** (aynı `/havale`, `.confirm-sheet`). Düzenle yeni idempotency key üretir. Replay (aynı key) mevcut dekonta gider. `aria-busy` iken `.remain strong` / input düz `#E8EEF5` iskelet (sonsuz shimmer yok).

### 10) Havale onay (T-057 — 9. ekran değil)

WePayUI / Papara P2P review. `.confirm-sheet` panel içinde: alıcı, tutar, açıklama, gönderim sonrası bakiye, idempotency kırıntısı. `h2#confirm-title` `data-autofocus` + 2px navy outline. Skip-link durur.

### 11) Hareketler tarih aralığı + fiş referansı (T-057)

Papara `listLedgers` start+end. `.filter-row`: Başlangıç | Bitiş | Tür | Filtrele. Boş dönem `empty-actions` Havale + Yükle. İşlem no `.mono` + Kopyala (masaüstü; mobilde col 2 gizli, dekontta durur). Özet son 5: tür dekont linki + 8 hane corr.

Dekont: correlation tam + kopyala; yükle/çek `****son4` satırı (`AccountHintLabel`) Description `****` içerince. Admin: Dondur + **Çöz** (aynı e-posta alanı); boş kuyruk `empty-block`.

### 12) Parite kromu (T-073)

Yükle formunda **İptal** (Çek ile aynı, Özet’e). Topbar rol hapı: Musteri / **Admin** (`.pill-admin` ılık çerçeve). 9. ekran değil.

Flutter: sol çekmece + alt sekme durur; dondurulmuş cüzdanda Havale/Yükle/Çek kapalı. JWT 401 ProblemDetails.

### 13) Kartlarım — canlı önizleme (T-097, ekran 9)

Kullanıcı isteği: kart bağla, yazarken kart yüzü. Rota `/kartlar`. 3D CSS kart (ön: numara gruplu / ad / SKT; CVV odakta arka yüz `rotateY(180deg)`, 220ms). Şema **ISO BIN** (T-103): Visa `4…` mavi yüz + VISA yazısı; Mastercard `51–55` / `2221–2720` koyu yüz + iki örtüşen daire (geometrik; resmi SVG yok); Troy `9792` teal. Yapı Kredi **kart adı**, şema değil. CVV input `name` yok. Kayıt son 4 + ad + şema. «Bu karttan cüzdana yükle» → `/yukle-cek?kart=`. Flutter Kartlarım aynı BIN yüzü (T-097 park kalktı). Footer demo one-liner.

