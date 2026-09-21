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
        if (brick != null)
        {
            brick.MarkCleared();
            return;
        }

        // Alana düşen top da yok edilsin — yoksa sonsuza kadar düşüp seviye sonu kontrolünü
        // "top hâlâ hareket ediyor" diye bekletir ve sahnede birikir.
        BallProjectile ball = other.GetComponentInParent<BallProjectile>();
        if (ball != null) Destroy(ball.gameObject);
    }
}
