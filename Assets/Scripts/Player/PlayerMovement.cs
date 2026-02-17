using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [Range(0, 1)] public float airControlMultiplier = 0.5f; // 1.0 is full speed, 0.5 is half speed

    [Header("Jump settings:")]
    public bool hasDoubleJump = true; // The toggle you asked for
    public float jumpForce = 7f;
    public float doubleJumpForce = 5f; 

    private bool isGrounded;
    private bool canDoubleJump; // Internal tracker

    [Header("Dash Settings")]
    public float dashVelocity = 14f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f;
    public float doubleTapTime = 0.2f;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private bool isDashing;
    private bool canDash = true;

    private float lastTapTimeA;
    private float lastTapTimeD;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDashing) return;

        Move();
        Jump();
        Flip();
        CheckDoubleTap();
    }


    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        
        // Calculate desired speed
        float currentMoveSpeed = isGrounded ? moveSpeed : moveSpeed * airControlMultiplier;

        // Apply movement while preserving current Y velocity (gravity/jumping)
        rb.linearVelocity = new Vector2(x * currentMoveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                // Normal Jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                canDoubleJump = true; 
            }
            else if (hasDoubleJump && canDoubleJump)
            {
                // Apply the double jump force
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
                
                canDoubleJump = false; 
            }
        }
    }

    void Flip()
    {
        float x = Input.GetAxisRaw("Horizontal");
        if (x > 0) sprite.flipX = false;
        else if (x < 0) sprite.flipX = true;
    }

    void CheckDoubleTap()
    {
        if (!canDash) return;

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            if (Time.time - lastTapTimeD <= doubleTapTime)
                StartCoroutine(PerformDash(1f));
            lastTapTimeD = Time.time;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            if (Time.time - lastTapTimeA <= doubleTapTime)
                StartCoroutine(PerformDash(-1f));
            lastTapTimeA = Time.time;
        }
    }

    IEnumerator PerformDash(float direction)
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(direction * dashVelocity, 0f);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

// Ground Detection Logic
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Refresh jumps only when we first hit the ground
        isGrounded = true;
        canDoubleJump = true; 
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}