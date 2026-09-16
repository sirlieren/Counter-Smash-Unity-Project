using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallProjectile : MonoBehaviour
{
    [Tooltip("1 = normal yerçekimi, 0 = hiç etkilenmez. Atış sırasında hafif bir düşüş için düşük bir değer (örn. 0.2-0.4) kullan.")]
    [SerializeField] private float gravityScale = 0.3f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic) return;
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }
}
