using System;
using System.Collections.Generic;
using UnityEngine;

public class ClearableBrick : MonoBehaviour
{
    [Tooltip("Açıksa, tuğla temizlendiğinde (hayalet hacme düştüğünde) sahneden yok edilir. Kapalıysa fizik simülasyonunda kalmaya devam eder (örn. alt tarafta birikmesi istenirse).")]
    [SerializeField] private bool destroyAfterClear = true;

    [Header("Ses")]
    [Tooltip("Skor onayı sesi — fizik çarpışma gürültüsünden ayrışsın diye 3D değil, UI benzeri sabit seviyede çalınır.")]
    [SerializeField] private AudioClip[] clearClips;
    [SerializeField] private float clearVolume = 0.6f;

    public static event Action<ClearableBrick> OnCleared;

    // Sahnedeki tüm tuğlalar. Seviye sonu kontrolü (kalan tuğla var mı, hâlâ hareket eden var mı)
    // buradan yapılır — her karede sahneyi taramak yerine kayıt tutuluyor.
    private static readonly HashSet<ClearableBrick> active = new HashSet<ClearableBrick>();

    private Rigidbody body;

    public bool IsCleared { get; private set; }

    /// <summary>Henüz temizlenmemiş tuğla sayısı.</summary>
    public static int CountRemaining()
    {
        int count = 0;
        foreach (ClearableBrick brick in active)
        {
            if (!brick.IsCleared) count++;
        }
        return count;
    }

    /// <summary>Temizlenmemiş tuğlalardan herhangi biri verilen hızın (dünya birimi/sn veya rad/sn) üstünde hareket ediyor mu.</summary>
    public static bool AnyMoving(float speedThreshold)
    {
        float sqrThreshold = speedThreshold * speedThreshold;
        foreach (ClearableBrick brick in active)
        {
            if (brick.IsCleared || brick.body == null) continue;
            if (brick.body.linearVelocity.sqrMagnitude > sqrThreshold || brick.body.angularVelocity.sqrMagnitude > sqrThreshold) return true;
        }
        return false;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        active.Add(this);
    }

    private void OnDisable()
    {
        active.Remove(this);
    }

    public void MarkCleared()
    {
        if (IsCleared) return;
        IsCleared = true;
        OnCleared?.Invoke(this);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(AudioManager.PickRandom(clearClips), transform.position, clearVolume, spatial: false);
        }

        if (destroyAfterClear) Destroy(gameObject);
    }
}
