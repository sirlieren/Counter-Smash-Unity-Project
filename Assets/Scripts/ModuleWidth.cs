using UnityEngine;

public class ModuleWidth : MonoBehaviour
{
    [Tooltip("Bu modülün gerçek genişliği, stud (tuğla birimi) cinsinden — 2X2 için 2, 2x8 için 8 gibi.")]
    [SerializeField] private int widthInStuds = 2;

    [Tooltip("Rastgele seçimde bu modülün ne kadar olası olacağı. Büyük sayı = daha sık seçilir.")]
    [SerializeField] private float selectionWeight = 1f;

    public int WidthInStuds => widthInStuds;
    public float SelectionWeight => selectionWeight;
}
