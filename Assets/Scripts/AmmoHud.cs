using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoHud : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("İkonların dizileceği kapsayıcı. Icon'ların otomatik yan yana dizilmesi için üstüne bir Horizontal Layout Group ekle.")]
    [SerializeField] private RectTransform iconContainer;
    [SerializeField] private GameObject ammoIconPrefab;

    [Header("Juice")]
    [Tooltip("Bir mermi harcandığında ikonun küçülüp kaybolma süresi.")]
    [SerializeField] private float popDuration = 0.15f;

    private readonly List<GameObject> icons = new List<GameObject>();
    private bool initialized;

    private void OnEnable()
    {
        initialized = false;
        BallLauncher.OnAmmoChanged += HandleAmmoChanged;
    }

    private void OnDisable()
    {
        BallLauncher.OnAmmoChanged -= HandleAmmoChanged;
    }

    private void HandleAmmoChanged(int remaining)
    {
        if (!initialized)
        {
            initialized = true;
            BuildIcons(remaining);
        }
        else
        {
            RemoveOneIcon();
        }
    }

    private void BuildIcons(int count)
    {
        foreach (GameObject icon in icons) Destroy(icon);
        icons.Clear();

        for (int i = 0; i < count; i++)
        {
            icons.Add(Instantiate(ammoIconPrefab, iconContainer));
        }
    }

    private void RemoveOneIcon()
    {
        if (icons.Count == 0) return;

        int lastIndex = icons.Count - 1;
        GameObject icon = icons[lastIndex];
        icons.RemoveAt(lastIndex);
        StartCoroutine(PopAndDestroy(icon));
    }

    private IEnumerator PopAndDestroy(GameObject icon)
    {
        Transform iconTransform = icon.transform;
        Vector3 startScale = iconTransform.localScale;
        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            iconTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / popDuration);
            yield return null;
        }

        Destroy(icon);
    }
}
