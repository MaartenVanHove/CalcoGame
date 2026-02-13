using UnityEngine;
using System.Collections.Generic; // Required for Lists

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5.0f;
    private float distanceToReach = 0.1f;
    
    private Rigidbody2D platformRb;
    private List<Transform> checkpoints = new List<Transform>();
    private int currentCheckpointIndex = 0;

    void Start()
    {
        platformRb = transform.Find("Platform").GetComponent<Rigidbody2D>();
        platformRb.bodyType = RigidbodyType2D.Kinematic;
        platformRb.useFullKinematicContacts = true; // Essential for triggers on kinematic bodies

        // Dynamically find all children except the Platform
        foreach (Transform child in transform)
        {
            if (child != platformRb.transform)
            {
                checkpoints.Add(child);
            }
        }

        if (checkpoints.Count == 0)
        {
            Debug.LogWarning("MovingPlatform: No checkpoints found as children!");
        }
    }

    void FixedUpdate()
    {
        if (checkpoints.Count == 0) return;

        Transform target = checkpoints[currentCheckpointIndex];
        Vector2 direction = (target.position - platformRb.transform.position).normalized;
        float distance = Vector2.Distance(platformRb.position, target.position);

        if (distance > distanceToReach)
        {
            platformRb.linearVelocity = direction * movementSpeed;
        }
        else
        {
            platformRb.linearVelocity = Vector2.zero;
            currentCheckpointIndex = (currentCheckpointIndex + 1) % checkpoints.Count;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        collision.transform.SetParent(platformRb.transform);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        collision.transform.SetParent(null);
    }
}