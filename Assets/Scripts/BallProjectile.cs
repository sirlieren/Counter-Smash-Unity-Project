using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallProjectile : MonoBehaviour
{
    [Tooltip("1 = normal yerçekimi, 0 = hiç etkilenmez. Atış sırasında hafif bir düşüş için düşük bir değer (örn. 0.2-0.4) kullan.")]
    [SerializeField] private float gravityScale = 0.3f;

    // Sahnedeki toplar — seviye sonu kontrolü "atılan top hâlâ hareket ediyor mu"yu buradan sorar.
    private static readonly HashSet<BallProjectile> active = new HashSet<BallProjectile>();

    private Rigidbody rb;
    private BallLauncher poolOwner;
    private BrickImpactSound impactSound;
    private BallHitEffect hitEffect;
    private PerfectBallExplosion perfectExplosion;
    private PerfectBallSkin perfectSkin;

    public Rigidbody Body => rb;

    /// <summary>Fırlatılmış (kinematik olmayan) toplardan herhangi biri verilen hızın üstünde hareket ediyor mu.</summary>
    public static bool AnyMoving(float speedThreshold)
    {
        float sqrThreshold = speedThreshold * speedThreshold;
        foreach (BallProjectile ball in active)
        {
            if (ball.rb == null || ball.rb.isKinematic) continue;
            if (ball.rb.linearVelocity.sqrMagnitude > sqrThreshold) return true;
        }
        return false;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        impactSound = GetComponent<BrickImpactSound>();
        hitEffect = GetComponent<BallHitEffect>();
        perfectExplosion = GetComponent<PerfectBallExplosion>();
        perfectSkin = GetComponent<PerfectBallSkin>();
    }

    public void SetPoolOwner(BallLauncher owner) => poolOwner = owner;

    public void PrepareForLaunch(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        if (impactSound != null) impactSound.ResetForReuse();
        if (hitEffect != null) hitEffect.ResetForReuse();
        if (perfectExplosion != null) perfectExplosion.ResetForReuse();
        if (perfectSkin != null) perfectSkin.Deactivate();
        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        if (poolOwner != null) poolOwner.ReturnBall(this);
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        active.Add(this);
    }

    private void OnDisable()
    {
        active.Remove(this);
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic) return;
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }
}
