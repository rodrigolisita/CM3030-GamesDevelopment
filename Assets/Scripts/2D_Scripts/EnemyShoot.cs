using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    [Header("Shooting Settings")]

    [Tooltip("The bullet object that will be fired.")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("The point from where the bullet will be spawned.")]
    [SerializeField] private Transform firePoint;

    [Tooltip("Time in seconds between each shot.")]
    [SerializeField] private float fireRate = 1f;

    // used to check if this enemy is actually active
    private Collider2D enemyCollider;

    private float fireTimer;

    void Start()
    {
        // Add a small initial random delay to prevent all enemies from shooting at the exact same time
        fireTimer = Random.Range(0, fireRate);

        // used to check if this enemy is actually active
        enemyCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Count down the timer
        fireTimer -= Time.deltaTime;

        // If the timer reaches zero, it's time to shoot
        // Won't shoot if enemy collider is disabled (due to enemy being dead)
        if (fireTimer <= 0f && enemyCollider != null && enemyCollider.enabled)
        {
            Shoot();
            // Reset the timer back to the fire rate
            fireTimer = fireRate;
        }
    }

    private void Shoot()
    {
        // Check if the projectile prefab or fire point has been set to avoid errors
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError(gameObject.name + ": Projectile Prefab or Fire Point is not set.");
            return;
        }

        // Create a new bullet at the firePoint's position and rotation
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}