using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem;

public class PingPongWeapon : MonoBehaviour
{
    [Header("Settings:")]
    public GameObject projectilePrefab;
    public Transform shootingPosition;

    private bool shootingKeyPressed;

    void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            shootingKeyPressed = true;
        }
    }

    void FixedUpdate()
    {
        if(shootingKeyPressed)
        {
            InstantiateProjectile();
            shootingKeyPressed = false;
        }
    }

    private Vector3 GetMousePosition()
    {
        // Mouse pos in your screen.
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 10.0f; 

        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }

    private void InstantiateProjectile()
    {   
        if(projectilePrefab != null)
        {
            Vector3 mousePos = GetMousePosition();
            Vector2 direction = (mousePos - shootingPosition.position);

            // Atan2 returns radians, Rad2Deg converts to 0-360 degrees
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            Instantiate(projectilePrefab, shootingPosition.position, rotation);
        }
        else
        {
            Debug.LogWarning("No projectile prefab selected");
        }
    }
}
