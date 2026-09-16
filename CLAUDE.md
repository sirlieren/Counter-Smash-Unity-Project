# Toy Room Smash

> Unity 6 (6000.3.17f1), URP, 3D. Bu dosyayı her oturum başında oku.

Asistan bu projede "Bailey" olarak anılıyor — Zack & Cody Güvertede dizisindeki Bailey Pickett
karakteri gibi: zeki, tatlı, sıcak bir ton. Teknik doğruluktan ödün vermeden bu tonla konuş.

## Ne yapıyoruz, neden

Mobil portfolyo/CV projesi — yayıncıya satış değil, iş başvurusu materyali. Eren PC oyunları
geliştirmiş (Bug Bane, Foes of Legacy) ama Türkiye oyun sektörü mobil ağırlıklı; bu, CV'ye
eklenecek tek, cilalı bir mobil örnek. **Hedef süre: 1 hafta.**

Tam GDD (tasarım kararları + gerekçeleri) Eren'in ikinci-beyin vault'unda:
`🏰 300-Projects/Mobil Oyun - Oda Yıkımı GDD.md`. Roadmap (7 günlük plan, checklist):
https://claude.ai/artifact/FnEVepkoBVKNucJ3c63uwW — bu dosya o ikisinin Unity-repo tarafındaki
özeti, çakışırsa GDD esas alınır.

## Oyun tek cümlede

Çocuk odası sahnesinde (düz cepheden, sabit kamera), Lego tarzı tuğlalardan kurulu yapılara
dokun-sürükle-bırak ile top fırlatıp yıkıyorsun. Sınırlı mermi, 8-10 seviye (artık **procedural,
seed-tabanlı** — bkz. `PROCEDURAL_GENERATION.md`), 1-3 yıldız.

## Çekirdek mekanik

- **Kontrol:** Ekranda sabit bir noktada duran top = fırlatma orijini. Kısa dokunuş (tık) →
  topun durduğu noktadan dokunulan noktaya doğru **güçsüz** atış. Basılı tutma → basılı tutma
  süresine bağlı büyüyüp sınıra ulaşınca sıfırlanan bir güç göstergesi (iç dolu çember, dış boş
  çember sınırı) çalışır; parmak kaldırıldığı andaki çember boyutu = **atış gücü**, parmağın o
  anki ekran konumu = **atış yönü** (top pozisyonundan o noktaya doğru). Düz çizgide ateş —
  yörünge/açı (arc) sistemi hâlâ YOK, sadece yön + güç var (bkz. Kapsam DIŞI). Minimum sürükleme
  eşiği (dead-zone) ve tek-tık-atış davranışı Inspector'dan bir bool ile açılıp kapatılabilir.
  İptal yok — dokunma bittiyse mermi gider. Atışlar arası bekleme süresi Inspector'dan
  ayarlanabilir bir float (varsayılan 0.25sn), ardışık atış serbest.
- **Fizik:** Özel "kırılma sistemi" YOK. Her tuğla ayrı Rigidbody + Collider; top çarpınca doğal
  fizik ile dağılıyor. Belirlenen zeminin/alanın dışına düşen veya deviren obje = temizlendi.
- **Kazanma:** Sınırlı mermiyle tüm hedef tuğlaları temizle. Mermi biterse level fail, sınırsız
  retry.
- **Skor:** Harcanan mermiye göre 1-3 yıldız. Meta-progression / unlock / skin YOK.

## Görsel yön

- **Arka plan = 2D sprite, 3D model DEĞİL.** Kamera sabit olduğu için 3D'nin derinlik avantajı
  zaten kullanılmıyor — düz bir quad/plane üzerine oda illüstrasyonu basılı. Bir tek düz duvar +
  az sayıda eleman (yatak, raf, pencere gibi), pastel/minimalist palet, Lego'nun ana renklerinden
  (kırmızı/sarı/mavi/yeşil) ilham alan yumuşak tonlar. Sprite'lar Google Flow ile üretiliyor,
  `Assets/Art/Backgrounds/` altına gelecek.
- **Ön plan = 3D**, Kenney Brick Kit (CC0, ücretsiz) — `Assets/Kenney/BrickKit/` gibi bir yere
  import edilir. Renk: palet texture + UV offset ile materyal (bkz. Unity forum çözümü, ~10 dk iş).

## Kapsam DIŞI (bilinçli sınırlar — feature creep önleme)

- Açı/yörünge bazlı (parabolik) nişan sistemi — güç sistemi artık VAR (basılı-tutma tabanlı güç
  çemberi), ama atış hâlâ düz çizgide, yay/açı yok.
- Unlock / skin / koleksiyon / herhangi bir meta-progression
- IAP, reklam, monetization
- Çoklu oyunculu / rekabetçi mod
- 10'dan fazla seviye
- 3D modellenmiş oda arka planı

## 7 günlük plan (özet — tam checklist artifact linkinde)

1. Kurulum + Brick Kit import + dokun-sürükle-bırak ilk atış prototipi
2. Fizik (Rigidbody/Collider/materyal tüm tuğla tiplerinde) + "temizlendi" kontrolü + mermi sayacı
3. **Procedural seviye sistemi** — modül kütüphanesi + kural bazlı yerleşim + parametre
   randomizasyonu (seed-tabanlı) + kararlılık kontrolü (bkz. `PROCEDURAL_GENERATION.md`).
   Kademeli zorluk, ilk seviye asla başarısız olunamaz. **Kapsam riski:** bu adım GDD'nin "8-10
   el yapımı seviye" kararının yerine geçiyor, ~1-1.5 gün ekstra iş — stabilite kontrolünü basit
   (tek metrik: toplam deplasman) ve modül sayısını başta 3-4 ile sınırlı tut, Gün 5/6'dan zaman
   çalınmasın.
4. Game feel: ses, toz partikülü, kamera sarsıntısı, perfect-shot slow-motion — **asıl CV sinyali**
5. UI + skor (1-3 yıldız) + ana menü/seviye seçim
6. Test, bug fix, zorluk dengesi
7. Build + 20-30sn gameplay klibi + "neden bu kararları verdim" notu + itch.io yükleme

## Çalışma tarzı (Eren'in tercihleri — Unity/kod session'larında geçerli)

- **Koordinat/boyut/oran/yerleşim dikte etme.** Sahneyi Eren görüyor, ben görmüyorum. Niyet +
  kısıt + referans + neden ver ("zemine oturan, oyuncunun ilk bakışta gördüğü bir nokta olsun"),
  kesin sayı/pozisyon değil — Eren açıkça sayı isterse ayrı.
- **Varsayılan olarak kod yazma.** Eren açıkça "kodunu yaz" demedikçe kod bloğu üretme; onun
  yerine yaklaşımı/algoritmayı jargonsuz, sade bir dille anlat (hangi yolla yapılabileceği, adımlar,
  neden). Eren spesifik bir tetik kelime kullanmıyor — "yaz", "kodla" gibi net bir istek yeterli.
  Eren açıkça isterse direkt kod yaz.
- **Bilgi seviyesi:** deneyimli oyun geliştirici + tasarımcı say, ama her alt-konunun (netcode,
  render pipeline detayları vb.) uzmanı değil. Yeni/niş terimi ilk geçtiğinde tek cümlede tanımla,
  temelleri anlatma, emin değilsen sor.
- **Çok adımlı bir işte** her adımdan sonra "devam edelim mi" diye sorma — bir sonrakine geç,
  Eren'i ilerlet. Durmak isterse kendisi söyler.
- Bir dosyayı değiştirmeden önce mevcut hâlini oku.

## Nerede kaldık

Atış mekanizması çalışıyor: `Assets/Scripts/BallLauncher.cs` (dokun/basılı-tut/bırak, güç
çemberi, nişan → uzak düzleme yansıtma, cooldown) ve `Assets/Scripts/BallProjectile.cs`
(ayarlanabilir `gravityScale` ile hafif düşüş — top prefabına eklenmesi lazım) yazıldı ve test
edildi. Çözülen önemli bug'lar: kamera ile spawn point'in aynı noktada olması (depth=0 hatası),
nişan hedefinin topun kendi derinliğine değil tuğla yapılarının derinliğine (`aimPlaneDistance`)
yansıtılması gerekliliği, çemberlerin topun derinliğinde (önde) kalması gerekliliği.

**Sıradaki:** Eren Lego tuğlalarına Rigidbody/Collider değerlerini atıyor. Sonra: Gün 2 —
tüm tuğla tiplerinde fizik/materyal ayarı, "temizlendi" kontrolü, mermi sayacı.
