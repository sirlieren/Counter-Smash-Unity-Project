# Procedural Level Generation — Araştırma Raporu

> Bailey'in önerdiği 5 adımlı yaklaşım (modül kütüphanesi → yerleşim kuralı → parametre
> randomizasyonu → kararlılık kontrolü → seed) akademik/endüstri pratiğiyle birebir örtüşüyor —
> bu Angry Birds tarzı fizik-yıkım oyunlarında kanıtlanmış standart yöntem. Aşağıda her adım için
> somut Unity uygulama detayı var. Kaynak: Stephenson & Renz (AAAI/AIIDE 2016), Ferreira & Toledo
> (CIG 2014), Unity forum tartışmaları, `unity-claude-skills` seed pattern referansı.

## 1. Modül kütüphanesi

Akademik literatürdeki en basit ve en çok kanıtlanmış yöntem: **sütun tabanlı temsil** (Ferreira &
Toledo 2014) — bir yapı, dikey sütunların dizisi olarak temsil edilir, her sütun elemanter bloklar
veya önceden tanımlı küçük "composed block" (Bailey'in "modül"ü) içerir. Bizim durumumuzda:

- Her modül bir prefab grubu: taban genişliği (kaç tuğla birim), yüksekliği, tuğla sayısı tanımlı.
- Modül tipleri: düz duvar dilimi, kule dilimi, piramit katmanı, köşe parçası.
- Önemli kısıtlama (literatürden): bloklar neredeyse hep **90 derece açılarla** yerleştirilir —
  eğik/düzensiz açılar stabilite doğrulamasını çok zorlaştırıyor, kaçınılmalı.

## 2. Yerleşim kuralı

Basit ve kanıtlanmış yaklaşım: level alanını yatayda **N dilime böl** (grid), her dilime bir modül
ata. Modül taban genişliği dilime sığmalı, dilimin tabanından (zeminden) başlayıp yukarı inşa
edilmeli. Akademik generatörler bunu "ground structures" olarak adlandırıyor — dilim genişlikleri
de rastgele ama minimum bir eşikle sınırlanıyor (çok dar dilim = anlamsız/aşırı basit modül).
Bizim kapsamımızda (8-10 seviye, tek ekran) bu kadarı yeterli — platform/asma yapı katmanına
(Angry Birds'teki "floating platforms") gerek yok.

## 3. Parametre randomizasyonu — Unity'de DOĞRU seed kullanımı

**Kritik uyarı, doğrudan sorunla karşılaşmadan önce bilinmeli:** `UnityEngine.Random` KULLANMA.
Global state taşır, çağrı sırasına bağımlıdır, `Awake`/`Start` gibi sırası garanti olmayan
yerlerden çağrılırsa aynı seed bile farklı sonuç üretebilir (Unity forum'da tam bu hata defalarca
raporlanmış — prosedürel dünya/dekor üretiminde "aynı seed farklı sonuç veriyor" şikayetlerinin
neredeyse tamamının kök nedeni bu).

**Doğru yöntem:** `System.Random`'ı saran bir `SeededRandom` sınıfı kullan, master seed'den
alt-sistemler (yerleşim, tuğla tipi seçimi, hedef sayısı) için **ayrı child seed'ler türet**
(hash ile). Böylece bir alt-sistemdeki değişiklik diğerinin sırasını kaydırmaz.

```csharp
public class SeededRandom
{
    private readonly System.Random rng;
    public int Seed { get; }

    public SeededRandom(int seed) { Seed = seed; rng = new System.Random(seed); }

    public int Range(int min, int max) => rng.Next(min, max);
    public float Float01() => (float)rng.NextDouble();
    public float Range(float min, float max) => min + Float01() * (max - min);

    public static int DeriveSeed(int masterSeed, string subsystem)
        => System.HashCode.Combine(masterSeed, subsystem.GetHashCode(System.StringComparison.Ordinal));
}
```

**İkinci gotcha (Unity forum'dan doğrulanmış, sık yapılan hata):** yerleşim sırasında "geçersiz
pozisyon → yeniden dene (reroll)" yapma. Reroll, RNG sırasını sadece o an kaydırır — aynı seed
farklı bir çalıştırmada farklı reroll sayısına denk gelirse tüm sonraki sonuçlar kayar. Bunun
yerine: önce N aday pozisyon/modülü sırayla üret, SONRA geçerliliğini kontrol et — üretim ve
doğrulama adımlarını RNG çağrısı açısından birbirinden ayır.

## 4. Kararlılık kontrolü — bu adım opsiyonel değil, kritik

Akademik makalelerin ortak bulgusu (Stephenson & Renz 2016, Tanager/Ferreira 2017): **teorik
stabilite hesabı (kütle/sürtünme/pozisyondan matematiksel analiz) tek başına yetmiyor** — gerçek
fizik motorunda (Unity dahil) simülasyon hataları/toleransları yüzünden teorik olarak stabil bir
yapı bile zamanla kendi kendine çöküyor. Pratikte kullanılan tek güvenilir yöntem: **yapıyı kur,
kısa bir süre gerçekten simüle et, hareketi ölç.**

**Unity'de uygulaması:**
1. Modülleri yerleştir, tüm Rigidbody'leri aktif et.
2. Birkaç saniye (gerçek zamanda değil, `Physics.Simulate()` ile manuel/hızlandırılmış adımlarla)
   fizik ilerlet — top atılmadan, sadece yerçekimi altında.
3. Her bloğun başlangıç pozisyonundan **toplam yer değiştirmesini** ölç ("Block Velocity/Displacement
   measure" — literatürdeki standart metrik).
4. Eşik üstü hareket eden blok varsa → yapı unstable → bu seed'i reddet → yeni seed dene.
5. Bir üst sınır koy (örn. 10 deneme) — sürekli unstable çıkıyorsa modül/parametre aralığı gözden
   geçirilmeli, sonsuz döngüye girme.

Bu adım aynı zamanda **CV'de gösterilecek en güçlü teknik detay** — "seviyelerin otomatik
üretildiği VE otomatik doğrulandığı" bir sistem, sadece "rastgele blok koydum" değil.

## 5. Seed sistemi — reproducibility

Her seviye bir `int seed` ile üretiliyor. Aynı seed = aynı level (modül seçimi + yerleşim +
parametreler bire bir tekrar üretilebilir). Pratik faydaları:
- Bug/crash raporlarken "seed 4471'de çöküyor" diye net bir referans.
- Kararlılık testi geçen seed'leri bir listede tutup "onaylı seed havuzu" oluşturabilirsin —
  8-10 seviye için bu havuzdan seçmek, tamamen runtime-random'dan daha güvenli bir orta yol.
- Portföy sunumunda ("neden bu kararları verdim" notunda) bahsedilecek somut bir teknik detay.

## Zaman/kapsam notu (dürüst gözlem)

Bu, GDD'deki "8-10 el yapımı seviye" yerine geçen bir sistem — ilk kurulumu (modül kütüphanesi +
yerleşim mantığı + stabilite kontrolü) muhtemelen 1-1.5 gün ekstra iş ister, ama bir kez kurulunca
"onaylı seed havuzundan 8-10 seviye seç" adımı elle tasarlamaktan hızlı. Net risk: 1 haftalık
hedefe göre bu Gün 3'ün kapsamını büyütüyor — diğer günlerden (özellikle Gün 5/6 polish-test) zaman
çalınmaması için stabilite kontrolünü basit tutmak (tek metrik: toplam deplasman) ve modül sayısını
başta 3-4 ile sınırlamak öneriyorum.

## Kaynaklar
- Stephenson & Renz, "Procedural Generation of Levels for Angry Birds Style Physics Games" (AIIDE 2016)
- Ferreira & Toledo, "A Search-based Approach for Generating Angry Birds Levels" (CIG 2014)
- Unity Discussions: "System.Random with same seed not 100% deterministic?"
- `unity-claude-skills` — Seed & Reproducibility pattern (SeededRandom)
