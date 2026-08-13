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

## Tipografi

- Sayfa kicker: 0.75rem, 600, letter-spacing `0.08em`, uppercase, muted
- `h1.page-title`: 1.75rem / 700 / tracking `-0.02em` — tek h1 = ekran adı (wordmark h1 değil)
- Lede: muted, title’dan sonra tek satır
- Para: `font-variant-numeric: tabular-nums`; özet bakiyesi ~2.35rem
- Buton: 600, min-height 2.75rem, navy dolgu veya ghost (şeffaf + navy çerçeve)

## Wordmark

Kare marka: 2rem, 1px çerçeve, içinde **C** — dolgu yok, gölge yok.  
İsim: **ClearPay**, tracking `0.04em`, 700. Sidebar’da beyaz; auth’ta navy.  
Tagline (yalnız auth): `Demo cüzdan` — “dijital banka” yazılmaz.

## Layout

```
[ sidebar 248px navy ] [ topbar | içerik max 920px | footer ]
```

Sol menü sırası sabit: Özet, Havale, Yükle/Çek, Hareketler; Admin role.  
Footer her yerde: **Demo — sahte banka gateway**

## Mobil (≤800px)

Sidebar drawer: kapalı `translateX(-100%)`, açık `is-open`. Hamburger topbar’da. Backdrop navy %35. Stat grid tek kolon. Auth kart tam genişlik, yatay padding 1.25rem. Form satırları (`field-row`, `split-2`) tek kolon ≤900px.

## Kompozisyonlar

### Giriş / kayıt (`_AuthLayout`)

Dikey ortalı kart, max 420px, 1px çizgi, gölge yok. Sıra: wordmark → tagline → `h1` (Giriş / Hesap oluştur) → lede → form → switch link. Birincil buton tam genişlik. Auth footer kartın dışında, aynı demo cümlesi.

### Özet (`/`)

Kicker **Cüzdan** → **Özet** → (isteğe selamlama).  
1. `balance-hero`: kullanılabilir bakiye (xl) + durum satırı (Aktif / Dondurulmuş)  
2. İki `stat-card`: bu ay giden | bu ay gelen  
3. CTA: **Havale gönder** (dolu), **Yükle** / **Çek** (ghost)  
4. Panel **Son hareketler** + “Tümünü gör”

Boş hareket: tablo hücresi değil, `empty-block` — başlık + bir satır ne yapılacağı. “Henüz hareket yok.” yetmez.

### Havale (`/havale`)

Kicker **Transfer**. Panel içinde: kalan bakiye üst şerit (label sol, tutar sağ, tabular) → alıcı e-posta → tutar | açıklama → ipucu → **Gönder** + **İptal**. Gönder, bakiye 0 iken disabled (mevcut). Tutar alanı görsel olarak para; süs ikon yok.

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
