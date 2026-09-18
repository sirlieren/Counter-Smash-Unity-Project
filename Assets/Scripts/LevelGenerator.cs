using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Modül kütüphanesi")]
    [SerializeField] private List<ModuleWidth> modulePrefabs = new List<ModuleWidth>();

    [Header("Bütçe (stud cinsinden)")]
    [SerializeField] private int minBudget = 16;
    [SerializeField] private int maxBudget = 24;

    [Header("Dünya birimi dönüşümü")]
    [Tooltip("1 stud kaç dünya birimi tutuyor. Örn: 2x8 tuğlanın genişliği (1.908) / 8 = 0.2385.")]
    [SerializeField] private float worldUnitsPerStud = 0.2385f;

    [Header("Yerleşim")]
    [Tooltip("Üretilen sıranın ortalanacağı referans nokta (X'te ortalanır, Y/Z aynen kullanılır).")]
    [SerializeField] private Transform originPoint;

    [Header("Test")]
    [SerializeField] private int seed = 12345;
    [SerializeField] private bool generateOnStart = true;

    private readonly List<GameObject> spawned = new List<GameObject>();

    private void Start()
    {
        if (generateOnStart) Generate(seed);
    }

    public void Generate(int levelSeed)
    {
        ClearGenerated();

        int budgetSeed = SeededRandom.DeriveSeed(levelSeed, "budget");
        int placementSeed = SeededRandom.DeriveSeed(levelSeed, "placement");

        SeededRandom budgetRng = new SeededRandom(budgetSeed);
        SeededRandom placementRng = new SeededRandom(placementSeed);

        int targetWidth = budgetRng.Range(minBudget, maxBudget + 1);

        List<ModuleWidth> chosen = new List<ModuleWidth>();
        int usedWidth = 0;

        while (true)
        {
            int remaining = targetWidth - usedWidth;
            List<ModuleWidth> candidates = GetCandidates(remaining);
            if (candidates.Count == 0) break;

            ModuleWidth picked = WeightedPick(candidates, placementRng);
            chosen.Add(picked);
            usedWidth += picked.WidthInStuds;
        }

        PlaceRow(chosen, usedWidth);
    }

    private List<ModuleWidth> GetCandidates(int remaining)
    {
        List<ModuleWidth> candidates = new List<ModuleWidth>();
        foreach (ModuleWidth module in modulePrefabs)
        {
            if (module.WidthInStuds <= remaining) candidates.Add(module);
        }
        return candidates;
    }

    private ModuleWidth WeightedPick(List<ModuleWidth> candidates, SeededRandom rng)
    {
        float totalWeight = 0f;
        foreach (ModuleWidth candidate in candidates) totalWeight += candidate.SelectionWeight;

        float roll = rng.Float01() * totalWeight;
        float cumulative = 0f;
        foreach (ModuleWidth candidate in candidates)
        {
            cumulative += candidate.SelectionWeight;
            if (roll <= cumulative) return candidate;
        }

        return candidates[candidates.Count - 1];
    }

    private void PlaceRow(List<ModuleWidth> chosen, int usedWidthStuds)
    {
        if (originPoint == null) return;

        float totalWorldWidth = usedWidthStuds * worldUnitsPerStud;
        float cursorX = originPoint.position.x - totalWorldWidth / 2f;

        foreach (ModuleWidth module in chosen)
        {
            float moduleWorldWidth = module.WidthInStuds * worldUnitsPerStud;
            float centerX = cursorX + moduleWorldWidth / 2f;
            Vector3 spawnPosition = new Vector3(centerX, originPoint.position.y, originPoint.position.z);

            GameObject instance = Instantiate(module.gameObject, spawnPosition, Quaternion.identity, transform);
            spawned.Add(instance);

            cursorX += moduleWorldWidth;
        }
    }

    private void ClearGenerated()
    {
        foreach (GameObject instance in spawned)
        {
            if (instance != null) Destroy(instance);
        }
        spawned.Clear();
    }
}
