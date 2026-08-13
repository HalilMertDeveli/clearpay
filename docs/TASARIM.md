# TASARIM — ClearPay

Kaynak: `docs/SPEC.md` ekran listesi. Piksel Figma yok; Razor mevcut sınıflara yakın. Coder `site.css` sahibi; bu belge + `wwwroot/css/brand.css` (ek token). Layout linki Coder HANDOFF’ta.

## Görsel bar

| Kural | Değer |
|-------|--------|
| Navy | `#1B2A4A` |
| Zemin | `#FFFFFF` (kart), wash `#F4F6F9` (sayfa) |
| Mürekkep | navy; muted `#5C6B86` |
| Çizgi | `#E6E9EF` |
| Tip | Inter 400/500/600/700 |
| Gölge | yok |
| Gradient | yok |
| Emoji | yok |
| Bootstrap | yok |

Yaratıcılık: hiyerarşi, boş durum, wordmark, form ritmi. Landing / kampanya sayfası yok.

**One-liner:** ClearPay — demo dijital cüzdan (WePay benzeri).

**Ürün çerçevesi (T-015):** WePay benzeri **dijital cüzdan / pay**. Kicker **Cüzdan**, tagline **Demo dijital cüzdan**. Şube / internet-bankacılığı kromu yok. `IBankGateway` yalnızca yükle/çek stub.

## Tipografi

- Sayfa kicker: 0.75rem, 600, letter-spacing `0.08em`, uppercase, muted
- `h1.page-title`: 1.75rem / 700 / tracking `-0.02em` — tek h1 = ekran adı (wordmark h1 değil)
- Lede: muted, title’dan sonra tek satır
- Para: `font-variant-numeric: tabular-nums`; özet bakiyesi ~2.35rem
- Buton: 600, min-height 2.75rem, navy dolgu veya ghost (şeffaf + navy çerçeve)

## Wordmark

Kare marka: 2rem, 1px çerçeve, içinde **C** — dolgu yok, gölge yok.  
İsim: **ClearPay**, tracking `0.04em`, 700. Sidebar’da beyaz; auth’ta navy.  
Tagline (yalnız auth): `Demo dijital cüzdan`.

## Layout

```
[ sidebar 248px navy ] [ topbar | içerik max 920px | footer ]
```

Sol menü sırası sabit: Özet, Havale, Yükle/Çek, Hareketler; Admin role.  
Footer her yerde: **Demo — yükleme için sahte gateway**

## Mobil (≤800px)

Sidebar drawer: kapalı `translateX(-100%)`, açık `is-open`. Hamburger topbar’da. Backdrop navy %35. Stat grid tek kolon. Auth kart tam genişlik, yatay padding 1.25rem. Form satırları (`field-row`, `split-2`) tek kolon ≤900px.

## Kompozisyonlar

### Giriş / kayıt (`_AuthLayout`)

Dikey ortalı kart, max 420px, 1px çizgi, gölge yok. Sıra: wordmark → tagline → `h1` (Giriş / Hesap oluştur) → lede → form → switch link. Birincil buton tam genişlik. Auth footer kartın dışında: **Demo — yükleme için sahte gateway**.

### Özet (`/`)

Kicker **Cüzdan** → **Özet** → (isteğe selamlama).  
1. `balance-hero`: kullanılabilir bakiye (xl) + durum satırı (Aktif / Dondurulmuş)  
2. İki `stat-card`: bu ay giden | bu ay gelen  
3. CTA: **Havale gönder** (dolu), **Yükle** / **Çek** (ghost)  
4. Panel **Son hareketler** + “Tümünü gör”

Boş hareket: tablo hücresi değil, `empty-block` — başlık + bir satır ne yapılacağı. “Henüz hareket yok.” yetmez.

### Havale (`/havale`)

Kicker **Havale**. Panel içinde: kalan bakiye üst şerit (label sol, tutar sağ, tabular) → alıcı e-posta → tutar | açıklama → ipucu → **Gönder** + **İptal**. Gönder, bakiye 0 iken disabled (mevcut). Tutar alanı görsel olarak para; süs ikon yok.

### Yükle / çek, hareketler, dekont, admin

SPEC sırası. İki kolon Yükle | Çek (`split-2`); hareketler filtre şeridi + tablo; boş tablo aynı `empty-block`. Dekont tek işlem, correlation id monospace. Admin: tablo + dondur / kuyruk; süs dashboard yok.

## Boş durum (zorunlu)

```
.empty-block
  .empty-title   — ne yok
  .empty-hint    — sonraki eylem (Havale / Yükle)
```

Renk muted; ortalama hizalı; padding ~2.25rem. İllüstrasyon yok.

## Form

Label 0.875rem / 500. Input 2.75rem, 1px `--line`, focus: 2px navy outline, offset 0. Hata `--danger` `#8A1F1F`, özet kutu 1px danger çerçeve. Disabled opacity 0.45.

## brand.css

Yalnız ek: kicker tracking, tabular-nums, wordmark, `empty-block`, hero sol 3px navy çizgi, para input. `site.css` kopyalanmaz. Coder her iki layout’ta `site.css` **sonrasına** linkler.

## Motion (canlı site)

Süre **150–250ms**, eğri `ease` (`transition-timing-function: ease`). Gölge kalkışı, bounce, blur, gradient sweep yok.

| Hedef | Ne | Süre |
|--------|----|------|
| `.sidebar` | `transform` (drawer `translateX`) | 200ms |
| `.nav-backdrop` | `opacity` | 180ms |
| `.btn` / `.btn-ghost` | `background-color`, `border-color`, `color` | 160ms |
| `.stat-card`, `.panel`, `.balance-hero`, `.auth-card` | `border-color` only (hover `#1B2A4A` %40) | 180ms |
| `.nav-link` | `background-color`, `color` | 150ms |
| `.field-input` | `border-color` | 150ms |

Yasak: `box-shadow` animasyonu, `scale(1.05)`, sayfa-içi confetti, kart “lift”.

```css
@media (prefers-reduced-motion: reduce) {
  .sidebar, .nav-backdrop, .btn, .stat-card, .panel,
  .balance-hero, .auth-card, .nav-link, .field-input {
    transition: none;
  }
}
```

Token önerisi (`brand.css`): `--motion: 180ms ease;` — 150–250ms bandı dışına çıkma.

## Tarifler (Coder — Razor ezme, CSS/sınıf)

Mevcut sınıflar: `auth-card`, `balance-hero`, `stack-form`, `data-table`, `empty-title`, `empty-hint`. Yeni sayfa yok. Tip ölçeği 16px kök.

### Tip ölçeği (sabit)

| Rol | Boyut | Ağırlık | Satır / ekstra |
|-----|--------|---------|----------------|
| Kicker `.page-kicker` | 0.75rem | 600 | tracking 0.08em, uppercase |
| `h1.page-title` | 1.75rem | 700 | tracking −0.02em; margin 0 0 0.5rem |
| `.lede` | 1rem | 400 | muted; margin 0 0 1.5rem |
| `.stat-label` | 0.8rem | 400 | muted; margin 0 0 0.4rem |
| `.stat-value-xl` | 2.35rem (≤800px: 1.9rem) | 700 | tabular-nums; tracking −0.02em |
| `.stat-value` | 1.35rem | 700 | tabular-nums |
| `.field-label` | 0.875rem | 500 | margin-bottom 0.35rem |
| `.field-input` | 1rem | 400 | height 2.75rem |
| `.field-hint` / `.empty-hint` | 0.875rem | 400 | muted |
| `.empty-title` | 1rem | 600 | navy; margin 0 0 0.35rem |
| `th` | 0.75rem | 600 | muted |
| `td` | 0.925rem | 400 | |
| `.btn` | 1rem | 600 | min-height 2.75rem; padding 0.65rem 1.15rem |

### 1) Giriş (`_AuthLayout` + Login)

Dikey ortalı dijital cüzdan girişi (WePay benzeri).

```
auth-shell  padding 2.5rem 1.25rem
auth-card   max-width 420px; padding 2.5rem 2.15rem 2.15rem
            border 1px --line; gölge yok
wordmark    mark 2rem + isim; tagline 0.85rem muted, margin −1.25rem 0 1.5rem
            metin: Demo dijital cüzdan
h1          Giriş — 1.75rem; lede 1rem, margin-bottom 0
auth-form   margin-top 1.5rem
.field      margin-bottom 1.1rem  (son field + btn arası ekstra yok; btn form akışında)
.btn-block  width 100%; margin-top 0.25rem
auth-switch margin 1.35rem 0 0; 0.9rem
auth-footer kart DIŞI, 0.75rem muted, margin-top 1.35rem
            metin: Demo — yükleme için sahte gateway
```

Kayıt aynı kart ritmi; dört field, hint şifre altında 0.35rem. Boş durum yok (form). Hata: `.validation-summary` 0.75rem 0.85rem padding, 1px danger.

### 2) Özet hero (`/` `.balance-hero`)

Cüzdan bakiyesi (WePay benzeri özet). Sol 3px navy (`brand.css`).

```
content        padding 2.25rem 2rem; max-width 920px
kicker→title   kicker 0 0 0.4rem; title 0 0 0.5rem; lede (ad) 0 0 1.75rem
balance-hero   padding 1.75rem 1.5rem; margin-bottom 1rem
               border 1px --line + border-left 3px #1B2A4A
stat-label     0.8rem muted → xl tutar 2.35rem tabular
status-line    margin 0.75rem 0 0; 0.85rem muted  (“Aktif” / “Dondurulmuş”)
stat-row       2 kolon, gap 1rem, margin-bottom 1.5rem
stat-card      padding 1.25rem 1.35rem
actions        gap 0.6rem; margin 0 0 1.75rem
               dolu: Havale gönder; ghost: Yükle, Çek
panel          padding 1.25rem 1.35rem; panel-head space-between, margin-bottom 0.75rem
```

**Boş (son hareketler):** `td.empty-cell` colspan 4; padding 2.25rem 1rem; ortalama.

- `.empty-title`: Henüz hareket yok
- `.empty-hint`: İlk havaleniz veya yüklemeniz burada görünür.

İllüstrasyon yok. Mobil: hero + stat-row tek kolon; xl 1.9rem.

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
                 Gönder dolu (bakiye 0 → disabled); İptal ghost
```

Boş alıcı/tutar: native validation / kırmızı özet; ayrı empty-block yok. Bakiye 0 ipucu hint’te kalır.

### 4) Hareket tablosu (`/hareketler` `.data-table`)

Cüzdan geçmişi. TASK-09 doldurur; iskelet bu ritim.

```
kicker Geçmiş; h1 Hareketler; lede 0 0 1.75rem
filtre paneli     padding 1.25rem 1.35rem; margin-bottom 1rem
.filter-row       3 field + Filtrele; gap 1rem; align end
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

Dekont satırı (TASK-09): correlation id monospace, 0.8rem muted.

