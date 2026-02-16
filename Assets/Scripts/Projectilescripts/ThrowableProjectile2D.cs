using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ThrowableProjectile2D : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounciness = 0.8f;
    public LayerMask ignoreCollisionLayers;

    [Header("Hit Settings")]
    public float baseHitForce = 10f;

    [Header("Boost UI")]
    public TextMeshProUGUI boostText;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Gradient speedColorGradient;
    public float maxSpeedForColor = 20f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Transform holdPoint;
    private GameObject owner;
    private bool isHeld = false;

    private int hitCount = 0;
    private bool firstHitAfterThrow = false;

    private readonly float[] multipliers = { 1.3f, 1.6f, 2.0f };
    private readonly Color[] boostColors =
    {
        Color.yellow,
        new Color(1f, 0.5f, 0f),
        Color.red
    };

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        PhysicsMaterial2D mat = new PhysicsMaterial2D { bounciness = bounciness, friction = 0f };
        rb.sharedMaterial = mat;

        if (boostText != null)
            boostText.gameObject.SetActive(false);
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
            float t = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeedForColor);
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

        hitCount = 0;
        firstHitAfterThrow = false;

        if (boostText != null)
            boostText.gameObject.SetActive(false);
    }

    // =========================
    // THROW
    // =========================
    public void Throw(Vector2 velocity)
    {
        if (!isHeld) return;

        isHeld = false;
        holdPoint = null;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = velocity;
        col.isTrigger = false;

        hitCount = 0;
        firstHitAfterThrow = false;

        if (boostText != null)
            boostText.gameObject.SetActive(false);
    }

    // =========================
    // HIT
    // =========================
    public void HitFromPlayer(Vector2 direction)
    {
        if (isHeld) return;

        // Only start boost after the first hit post-throw
        if (!firstHitAfterThrow)
        {
            firstHitAfterThrow = true;
            hitCount = 1;
        }
        else
        {
            hitCount++;
            if (hitCount > 4) hitCount = 1; // reset after 4th hit
        }

        int boostIndex = Mathf.Clamp(hitCount - 1, 0, multipliers.Length - 1);
        float multiplier = multipliers[boostIndex];
        Color color = boostColors[boostIndex];

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * baseHitForce * multiplier, ForceMode2D.Impulse);

        UpdateBoostUI(multiplier, color);
    }

    private void UpdateBoostUI(float multiplier, Color color)
    {
        if (boostText == null) return;

        boostText.gameObject.SetActive(true);
        boostText.text = multiplier.ToString("0.0") + "x";
        boostText.color = color;
    }

    // =========================
    // HELPERS
    // =========================
    public void ResetBoost()
    {
        hitCount = 0;
        firstHitAfterThrow = false;
        if (boostText != null)
            boostText.gameObject.SetActive(false);
    }

    public bool IsHeld() => isHeld;

    public int CurrentHitMultiplier() => hitCount;

    public float GetCurrentMultiplier()
    {
        if (hitCount == 0) return 1f;
        int boostIndex = Mathf.Clamp(hitCount - 1, 0, multipliers.Length - 1);
        return multipliers[boostIndex];
    }

    public Color GetCurrentColor()
    {
        if (hitCount == 0) return Color.white;
        int boostIndex = Mathf.Clamp(hitCount - 1, 0, boostColors.Length - 1);
        return boostColors[boostIndex];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isHeld) return;
        if (owner != null && collision.gameObject == owner) return;

        if (((1 << collision.gameObject.layer) & ignoreCollisionLayers) != 0)
            Physics2D.IgnoreCollision(col, collision.collider);
    }
}
