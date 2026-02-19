using UnityEngine;

public class NewPingPongScript : MonoBehaviour
{
    [Header("Settings:")]
    public float speed = 15f;
    public int availableHits = 3;
    [Tooltip("How much to predict player movement (0.1 to 0.5 recommended)")]
    public float predictionFactor = 0.2f;

    [Header("Layers (Ensure these match your Project Settings):")]
    public int environmentLayer = 8;
    public int enemyLayer = 9;

    private Rigidbody2D rb;
    private Rigidbody2D playerRb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerRb = playerObj.GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogError("Projectile could not find a GameObject with the tag 'Player'!");
        }

        rb.linearVelocity = transform.right * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == environmentLayer)
        {
            Destroy(gameObject);
        }

        if (collision.gameObject.layer == enemyLayer)
        {
            HandleEnemyHit();
        }
    }

    private void HandleEnemyHit()
    {
        if (playerRb == null) return;

        // 1. Calculate the predicted position of the player
        // We cast to Vector2 to ensure we stay in 2D space
        Vector2 playerPos = playerRb.position;
        Vector2 playerVel = playerRb.linearVelocity;
        Vector2 currentPos = transform.position;

        // Math: (Target Position + Lead) - My Current Position
        Vector2 predictedTarget = playerPos + (playerVel * predictionFactor);
        Vector2 returnDirection = (predictedTarget - currentPos).normalized;

        // 2. Apply the new velocity
        rb.linearVelocity = returnDirection * speed;

        // 3. Update rotation to face the return direction
        float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 4. Update Hit Logic
        availableHits--;
        Debug.Log($"Hit Enemy! Rebounding. Hits left: {availableHits}");

        if (availableHits <= 0)
        {
            Destroy(gameObject, 0.1f); // Destroy slightly after impact for visual polish
        }
    }
}