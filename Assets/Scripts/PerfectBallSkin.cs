using UnityEngine;

/// <summary>
/// Perfect atışta topa "yıldız modu" görünümü verir: renk ve emission renk çemberinde döner.
/// Ball prefabına eklenir; başta kapalıdır, BallLauncher perfect atışta Activate() çağırır.
/// Materyalin URP Lit (veya _BaseColor/_EmissionColor alan bir shader) olması gerekir.
/// </summary>
public class PerfectBallSkin : MonoBehaviour
{
    [Tooltip("Boş bırakılırsa çocuklardaki tüm MeshRenderer'lar kullanılır.")]
    [SerializeField] private Renderer[] targets;

    [Header("Renk döngüsü")]
    [Tooltip("Saniyede kaç tam renk turu atılacağı.")]
    [SerializeField] private float cyclesPerSecond = 1.5f;
    [SerializeField, Range(0f, 1f)] private float saturation = 0.8f;
    [Tooltip("Emission parlaklık çarpanı (HDR) — bloom varsa parlamayı bu belirler.")]
    [SerializeField] private float emissionIntensity = 2f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private Material[] materials;

    private void Awake()
    {
        enabled = false;
    }

    public void Activate()
    {
        if (targets == null || targets.Length == 0)
        {
            targets = GetComponentsInChildren<MeshRenderer>();
        }

        // renderer.materials paylaşılan asset'i değil, bu topa özel kopyaları döndürür —
        // böylece diğer toplar ve tuğlalar etkilenmez.
        var list = new System.Collections.Generic.List<Material>();
        foreach (Renderer target in targets)
        {
            if (target == null) continue;
            foreach (Material material in target.materials)
            {
                material.EnableKeyword("_EMISSION");
                list.Add(material);
            }
        }
        materials = list.ToArray();
        enabled = true;
    }

    private void Update()
    {
        float hue = Mathf.Repeat(Time.time * cyclesPerSecond, 1f);
        Color color = Color.HSVToRGB(hue, saturation, 1f);

        foreach (Material material in materials)
        {
            if (material.HasProperty(BaseColorId)) material.SetColor(BaseColorId, color);
            if (material.HasProperty(EmissionColorId)) material.SetColor(EmissionColorId, color * emissionIntensity);
        }
    }

    private void OnDestroy()
    {
        // Kopya materyaller top yok olunca kendiliğinden temizlenmez.
        if (materials == null) return;
        foreach (Material material in materials)
        {
            if (material != null) Destroy(material);
        }
    }
}
