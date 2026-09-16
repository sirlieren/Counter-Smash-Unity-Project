using System;
using UnityEngine;

public class ClearableBrick : MonoBehaviour
{
    [Tooltip("Açıksa, tuğla temizlendiğinde (hayalet hacme düştüğünde) sahneden yok edilir. Kapalıysa fizik simülasyonunda kalmaya devam eder (örn. alt tarafta birikmesi istenirse).")]
    [SerializeField] private bool destroyAfterClear = true;

    public static event Action<ClearableBrick> OnCleared;

    public bool IsCleared { get; private set; }

    public void MarkCleared()
    {
        if (IsCleared) return;
        IsCleared = true;
        OnCleared?.Invoke(this);
        if (destroyAfterClear) Destroy(gameObject);
    }
}
