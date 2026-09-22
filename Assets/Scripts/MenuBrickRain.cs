using System.Collections;
using UnityEngine;

public class MenuBrickRain : MonoBehaviour
{
    [Tooltip("Arka planda düşecek dekoratif tuğla prefabları. Gameplay tuğlalarını değil, ClearableBrick içermeyen hafif bir kopya set kullan — menü sahnesi seviye/skor mantığına karışmasın.")]
    [SerializeField] private GameObject[] brickPrefabs;

    [Tooltip("İki spawn arasındaki süre aralığı (saniye). Organik görünmesi için bu aralıkta rastgele seçilir.")]
    [SerializeField] private Vector2 spawnIntervalRange = new Vector2(0.4f, 1.2f);

    [Tooltip("Spawn noktasının etrafında yatayda ne kadar rastgele dağılacağı (dünya birimi, toplam genişlik).")]
    [SerializeField] private float spawnWidth = 6f;

    [Tooltip("Tuğla kaç saniye sonra yok edilecek. Ekranın altına düşüp gözden kaybolduktan sonra sahnede birikmesin diye.")]
    [SerializeField] private float lifeTime = 6f;

    [Tooltip("Düşerken tumbling hissi için rastgele başlangıç açısal hızı (derece/saniye).")]
    [SerializeField] private float maxAngularSpeed = 180f;

    private void OnEnable()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnIntervalRange.x, spawnIntervalRange.y));
            SpawnBrick();
        }
    }

    private void SpawnBrick()
    {
        if (brickPrefabs == null || brickPrefabs.Length == 0) return;

        GameObject prefab = brickPrefabs[Random.Range(0, brickPrefabs.Length)];
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-spawnWidth * 0.5f, spawnWidth * 0.5f), 0f, 0f);

        GameObject brick = Instantiate(prefab, spawnPos, Random.rotation);

        Rigidbody rb = brick.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.angularVelocity = Random.insideUnitSphere * maxAngularSpeed * Mathf.Deg2Rad;
        }

        Destroy(brick, lifeTime);
    }
}
