using System.Collections.Generic;
using UnityEngine;

/// <summary>Tek atımlık parçacık efektlerini aynı sahnede tekrar kullanır.</summary>
public class GameplayEffectPool : MonoBehaviour
{
    private const int MaxIdlePerPrefab = 8;
    private static GameplayEffectPool instance;
    private readonly Dictionary<GameObject, Stack<PooledEffect>> idle = new Dictionary<GameObject, Stack<PooledEffect>>();
    private bool shuttingDown;

    public static void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null) return;
        GameplayEffectPool pool = GetOrCreate();
        Stack<PooledEffect> stack = pool.GetStack(prefab);
        while (stack.Count < count) stack.Push(pool.Create(prefab));
    }

    public static void Spawn(GameObject prefab, Vector3 position, Quaternion rotation, float uniformScale = 1f)
    {
        if (prefab == null) return;
        GameplayEffectPool pool = GetOrCreate();
        Stack<PooledEffect> stack = pool.GetStack(prefab);
        PooledEffect effect = stack.Count > 0 ? stack.Pop() : pool.Create(prefab);
        effect.Play(position, rotation, prefab.transform.localScale * uniformScale);
    }

    private static GameplayEffectPool GetOrCreate()
    {
        if (instance != null) return instance;
        var root = new GameObject("GameplayEffectPool");
        instance = root.AddComponent<GameplayEffectPool>();
        return instance;
    }

    private Stack<PooledEffect> GetStack(GameObject prefab)
    {
        if (!idle.TryGetValue(prefab, out Stack<PooledEffect> stack))
        {
            stack = new Stack<PooledEffect>();
            idle.Add(prefab, stack);
        }
        return stack;
    }

    private PooledEffect Create(GameObject prefab)
    {
        GameObject effectObject = Instantiate(prefab, transform);
        PooledEffect effect = effectObject.AddComponent<PooledEffect>();
        effect.Initialize(this, prefab);
        effectObject.SetActive(false);
        return effect;
    }

    internal void Return(PooledEffect effect, GameObject prefab)
    {
        if (shuttingDown) return;
        Stack<PooledEffect> stack = GetStack(prefab);
        if (stack.Count >= MaxIdlePerPrefab)
        {
            Destroy(effect.gameObject);
            return;
        }
        stack.Push(effect);
    }

    private void OnEnable() => shuttingDown = false;
    private void OnDisable() => shuttingDown = true;

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
