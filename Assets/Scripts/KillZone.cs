using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillZone : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        ClearableBrick brick = other.GetComponentInParent<ClearableBrick>();
        if (brick != null) brick.MarkCleared();
    }
}
