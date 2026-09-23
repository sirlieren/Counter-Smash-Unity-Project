using CartoonFX;
using UnityEngine;

/// <summary>Efekt bitince kaynağının havuzuna döner.</summary>
public class PooledEffect : MonoBehaviour
{
    private GameplayEffectPool owner;
    private GameObject sourcePrefab;
    private ParticleSystem rootParticles;
    private bool rented;

    public void Initialize(GameplayEffectPool pool, GameObject prefab)
    {
        owner = pool;
        sourcePrefab = prefab;
        rootParticles = GetComponent<ParticleSystem>();
        foreach (CFXR_Effect effect in GetComponentsInChildren<CFXR_Effect>(true))
            effect.clearBehavior = CFXR_Effect.ClearBehavior.Disable;
    }

    public void Play(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = scale;
        rented = true;
        gameObject.SetActive(true);
        if (rootParticles != null)
        {
            rootParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            rootParticles.Play(true);
        }
    }

    private void OnDisable()
    {
        if (!rented) return;
        rented = false;
        if (owner != null && owner.isActiveAndEnabled) owner.Return(this, sourcePrefab);
    }
}
