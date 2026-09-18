using UnityEngine;

public class RandomBrickMaterial : MonoBehaviour
{
    [Tooltip("Bu tuğlaya spawn anında rastgele atanacak materyaller. Aynı modül her seferinde farklı renk kombinasyonuyla görünür.")]
    [SerializeField] private Material[] materials;

    [SerializeField] private MeshRenderer targetRenderer;

    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<MeshRenderer>();
        if (materials == null || materials.Length == 0) return;

        targetRenderer.sharedMaterial = materials[Random.Range(0, materials.Length)];
    }
}
