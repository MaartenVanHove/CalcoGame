using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5.0f;
    private float distanceToReach = 0.1f;
    
    private Rigidbody2D platformRb;
    private Transform checkpoint1;
    private Transform checkpoint2;
    private Transform[] checkpoints;
    private int currentCheckpointIndex = 0;

    void Start()
    {
        platformRb = transform.Find("Platform").GetComponent<Rigidbody2D>();
        
        checkpoint1 = transform.Find("Checkpoint1");
        checkpoint2 = transform.Find("Checkpoint2");
        checkpoints = new Transform[] { checkpoint1, checkpoint2 };
    }

    void FixedUpdate()
    {
        Transform target = checkpoints[currentCheckpointIndex];

        Vector2 newPos = Vector2.MoveTowards(
            platformRb.position, 
            target.position, 
            movementSpeed * Time.fixedDeltaTime
        );

        platformRb.MovePosition(newPos);

        if (Vector2.Distance(platformRb.position, target.position) <= distanceToReach)
        {
            currentCheckpointIndex = (currentCheckpointIndex + 1) % checkpoints.Length;
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