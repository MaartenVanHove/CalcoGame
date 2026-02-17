using UnityEngine;

public class MovingTowardsPlayer : MonoBehaviour
{
[Header("Targeting")]
    public Transform target;

    [Header("Smoothing")]
    [Tooltip("Higher = more delay/smoother. Lower = tighter follow.")]
    public float smoothTime = 0.25f;
    
    [Header("Offset")]
    public Vector2 offset = new Vector2(0, 2); // Look slightly above player

    private Vector3 currentVelocity = Vector3.zero;
    private float cameraZ;

    void Start()
    {
        cameraZ = transform.position.z;
    }

    // LateUpdate is BEST for cameras because it runs AFTER the player moves
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, cameraZ);

        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPos, 
            ref currentVelocity, 
            smoothTime
        );
    }
}