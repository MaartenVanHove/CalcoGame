using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerThrow2D : MonoBehaviour
{
    [Header("References")]
    public Transform holdPoint;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public TextMeshProUGUI hitCounterText;

    [Header("Throw Settings")]
    public float throwForce = 10f;
    public float throwUpwardForce = 5f;

    [Header("Interaction Ranges")]
    public float pickupRadius = 1.5f;
    public float hitRadius = 1.2f;
    public float snapRadius = 1.8f;

    [Header("Bounce Settings")]
    public float snapBounceForce = 14f;

    [Header("Coyote Time Settings")]
    public float coyoteTime = 0.15f;

    [Header("UI Animation")]
    public float popScale = 1.3f;
    public float popSpeed = 8f;

    private Rigidbody2D rb;
    private Camera cam;
    private ThrowableProjectile2D projectile;

    private Vector3 originalTextScale;
    private bool hasSnappedThisAir = false;
    private float coyoteTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        if (hitCounterText != null)
            originalTextScale = hitCounterText.transform.localScale;
    }

    void Update()
    {
        UpdateCoyoteTime();
        ResetAirSnapOnLand();

        HandlePickup();
        HandleThrow();
        HandleHit();
        HandleSnap();
        UpdateHitCounterUI();
    }

    // =========================
    // GROUND CHECK
    // =========================
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void UpdateCoyoteTime()
    {
        if (IsGrounded())
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;
    }

    private void ResetAirSnapOnLand()
    {
        if (IsGrounded())
            hasSnappedThisAir = false;
    }

    // =========================
    // PICKUP
    // =========================
    private void HandlePickup()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);

            foreach (Collider2D col in hits)
            {
                ThrowableProjectile2D proj = col.GetComponent<ThrowableProjectile2D>();
                if (proj != null)
                {
                    projectile = proj;
                    projectile.Initialize(holdPoint, gameObject);
                    break;
                }
            }
        }
    }

    // =========================
    // THROW
    // =========================
    private void HandleThrow()
    {
        if (Input.GetMouseButtonDown(0) && projectile != null && projectile.IsHeld())
        {
            Vector2 aimDir = GetAimDirection();
            Vector2 velocity = aimDir.normalized * throwForce + Vector2.up * throwUpwardForce;
            projectile.Throw(velocity);
        }
    }

    // =========================
    // HIT
    // =========================
    private void HandleHit()
    {
        if (Input.GetMouseButtonDown(0) && projectile != null && !projectile.IsHeld())
        {
            float distance = Vector2.Distance(transform.position, projectile.transform.position);

            if (distance <= hitRadius)
            {
                Vector2 dir = (projectile.transform.position - transform.position).normalized;
                projectile.HitFromPlayer(dir);

                if (hitCounterText != null)
                    hitCounterText.transform.localScale = originalTextScale * popScale;
            }
        }
    }

    // =========================
    // SNAP BOUNCE (NO PULL)
    // =========================
    private void HandleSnap()
    {
        if (Input.GetKeyDown(KeyCode.Space) &&
            projectile != null &&
            !projectile.IsHeld() &&
            !hasSnappedThisAir)
        {
            bool canUseCoyote = coyoteTimer > 0f;
            bool airborne = !IsGrounded();

            if (airborne || canUseCoyote)
            {
                float distance = Vector2.Distance(transform.position, projectile.transform.position);

                if (distance <= snapRadius)
                {
                    hasSnappedThisAir = true;

                    Vector2 fromBall = (rb.position - (Vector2)projectile.transform.position).normalized;

                    if (fromBall.y < 0)
                        fromBall.y = Mathf.Abs(fromBall.y);

                    rb.linearVelocity = Vector2.zero;
                    rb.AddForce(fromBall * snapBounceForce, ForceMode2D.Impulse);

                    projectile.BoostTrail();
                }
            }
        }
    }

    // =========================
    // AIM
    // =========================
    private Vector2 GetAimDirection()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        return (Vector2)(mouseWorld - holdPoint.position);
    }

    // =========================
    // UI
    // =========================
    private void UpdateHitCounterUI()
    {
        if (hitCounterText == null) return;

        if (projectile != null && !projectile.IsHeld())
        {
            int hits = projectile.CurrentHitMultiplier();
            hitCounterText.text = $"Hit x{hits}";
            hitCounterText.color = (hits >= 2) ? Color.red : Color.white;

            hitCounterText.transform.localScale =
                Vector3.Lerp(hitCounterText.transform.localScale,
                originalTextScale,
                Time.deltaTime * popSpeed);
        }
        else
        {
            hitCounterText.text = "";
            hitCounterText.transform.localScale = originalTextScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, snapRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
