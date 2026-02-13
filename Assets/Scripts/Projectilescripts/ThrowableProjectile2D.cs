using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ThrowableProjectile2D : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounciness = 0.8f;
    public LayerMask ignoreCollisionLayers;

    [Header("Hit Settings")]
    public float hitBoostForce = 10f;
    public int maxHitMultiplier = 2;

    [Header("Visual Effects")]
    public SpriteRenderer spriteRenderer;
    public Gradient speedColorGradient;
    public float maxSpeedForColor = 20f;
    public TrailRenderer trail;

    private Rigidbody2D rb;
    private Collider2D col;
    private Transform holdPoint;
    private GameObject owner;
    private bool isHeld = false;
    private int currentHitMultiplier = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        PhysicsMaterial2D mat = new PhysicsMaterial2D();
        mat.bounciness = bounciness;
        mat.friction = 0f;
        rb.sharedMaterial = mat;
    }

    void FixedUpdate()
    {
        if (isHeld && holdPoint != null)
        {
            rb.position = holdPoint.position;
            rb.linearVelocity = Vector2.zero;
        }

        if (!isHeld && spriteRenderer != null)
        {
            float speed = rb.linearVelocity.magnitude;
            float t = Mathf.Clamp01(speed / maxSpeedForColor);
            spriteRenderer.color = speedColorGradient.Evaluate(t);
        }
    }

    // =========================
    // PICKUP
    // =========================
    public void Initialize(Transform hold, GameObject playerOwner)
    {
        holdPoint = hold;
        owner = playerOwner;
        isHeld = true;

        rb.bodyType = RigidbodyType2D.Kinematic;
        col.isTrigger = true;
    }

    // =========================
    // THROW
    // =========================
    public void Throw(Vector2 velocity)
    {
        isHeld = false;
        holdPoint = null;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = velocity;

        col.isTrigger = false;
        currentHitMultiplier = 0;
    }

    // =========================
    // HIT
    // =========================
    public void HitFromPlayer(Vector2 direction)
    {
        if (isHeld) return;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * hitBoostForce, ForceMode2D.Impulse);

        currentHitMultiplier++;

        if (currentHitMultiplier > maxHitMultiplier)
            currentHitMultiplier = 0;

        BoostTrail();
    }

    public void BoostTrail()
    {
        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
            trail.emitting = true;
        }
    }

    // =========================
    // HELPERS
    // =========================
    public bool IsHeld() => isHeld;
    public int CurrentHitMultiplier() => currentHitMultiplier;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isHeld) return;
        if (owner != null && collision.gameObject == owner) return;

        if (((1 << collision.gameObject.layer) & ignoreCollisionLayers) != 0)
        {
            Physics2D.IgnoreCollision(col, collision.collider);
        }
    }
}
