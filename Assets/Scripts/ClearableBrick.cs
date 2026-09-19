using System;
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

    public bool IsCleared { get; private set; }

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
