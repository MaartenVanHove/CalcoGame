using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerThrow2D : MonoBehaviour
{
    [Header("References")]
    public Transform holdPoint;
    public TextMeshProUGUI hitCounterText; // boost text
    public TextBehaviour textBehaviour;     // follows player

    [Header("Throw Settings")]
    public float throwForce = 10f;
    public float throwUpwardForce = 5f;

    [Header("Interaction")]
    public float pickupRadius = 1.5f;
    public float hitRadius = 1.2f;

    private Rigidbody2D rb;
    private Camera cam;
    private ThrowableProjectile2D projectile;
    private Vector3 originalTextScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        if (hitCounterText != null)
            originalTextScale = hitCounterText.transform.localScale;

        // Make UI always follow player
        if (textBehaviour != null)
            textBehaviour.SetTarget(transform);
    }

    void Update()
    {
        HandlePickup();
        HandleThrow();
        HandleHit();
        UpdateHitCounterUI();
    }

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

    private void HandleThrow()
    {
        if (Input.GetMouseButtonDown(0) && projectile != null && projectile.IsHeld())
        {
            Vector2 aimDir = GetAimDirection();
            Vector2 velocity = aimDir.normalized * throwForce + Vector2.up * throwUpwardForce;
            projectile.Throw(velocity);
        }
    }

    private void HandleHit()
    {
        if (Input.GetMouseButtonDown(0) && projectile != null && !projectile.IsHeld())
        {
            float distance = Vector2.Distance(transform.position, projectile.transform.position);
            if (distance <= hitRadius)
            {
                Vector2 dir = (projectile.transform.position - transform.position).normalized;
                projectile.HitFromPlayer(dir);

                // Pop animation
                if (hitCounterText != null)
                    hitCounterText.transform.localScale = originalTextScale * 1.3f;
            }
        }
    }

    private Vector2 GetAimDirection()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        return (Vector2)(mouseWorld - holdPoint.position);
    }

    private void UpdateHitCounterUI()
    {
        if (hitCounterText == null || projectile == null) return;

        if (!projectile.IsHeld() && projectile.CurrentHitMultiplier() > 0)
        {
            float multiplier = projectile.GetCurrentMultiplier();
            Color color = projectile.GetCurrentColor();

            hitCounterText.text = multiplier.ToString("0.0") + "x";
            hitCounterText.color = color;

            // Smooth pop animation
            hitCounterText.transform.localScale =
                Vector3.Lerp(hitCounterText.transform.localScale, originalTextScale, Time.deltaTime * 8f);
        }
        else
        {
            hitCounterText.text = "";
            hitCounterText.transform.localScale = originalTextScale;
        }
    }
}
