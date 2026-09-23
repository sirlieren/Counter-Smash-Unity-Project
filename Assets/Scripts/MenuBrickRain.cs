using System.Collections;
using System.Collections.Generic;
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

    private struct ActiveBrick
    {
        public GameObject prefab;
        public GameObject instance;
        public float returnAt;
    }

    private readonly Dictionary<GameObject, Stack<GameObject>> idle = new Dictionary<GameObject, Stack<GameObject>>();
    private readonly List<ActiveBrick> activeBricks = new List<ActiveBrick>();
    private Coroutine spawnLoop;

    private void OnEnable()
    {
        spawnLoop = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnLoop != null) StopCoroutine(spawnLoop);
        spawnLoop = null;
        for (int i = activeBricks.Count - 1; i >= 0; i--)
            ReturnBrick(activeBricks[i]);
        activeBricks.Clear();
    }

    private void Update()
    {
        float now = Time.time;
        for (int i = activeBricks.Count - 1; i >= 0; i--)
        {
            if (now < activeBricks[i].returnAt) continue;
            ReturnBrick(activeBricks[i]);
            activeBricks.RemoveAt(i);
        }
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
        if (prefab == null) return;
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-spawnWidth * 0.5f, spawnWidth * 0.5f), 0f, 0f);

        Stack<GameObject> stack = GetStack(prefab);
        GameObject brick = stack.Count > 0 ? stack.Pop() : Instantiate(prefab, transform);
        brick.transform.SetPositionAndRotation(spawnPos, Random.rotation);
        brick.SetActive(true);

        Rigidbody rb = brick.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Random.insideUnitSphere * maxAngularSpeed * Mathf.Deg2Rad;
                rb.WakeUp();
            }
        }

        activeBricks.Add(new ActiveBrick { prefab = prefab, instance = brick, returnAt = Time.time + lifeTime });
    }

    private Stack<GameObject> GetStack(GameObject prefab)
    {
        if (!idle.TryGetValue(prefab, out Stack<GameObject> stack))
        {
            stack = new Stack<GameObject>();
            idle.Add(prefab, stack);
        }
        return stack;
    }

    private void ReturnBrick(ActiveBrick entry)
    {
        if (entry.instance == null) return;
        entry.instance.SetActive(false);
        GetStack(entry.prefab).Push(entry.instance);
    }
}
