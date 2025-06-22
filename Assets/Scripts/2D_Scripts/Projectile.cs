using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Enum to define who fired the projectile.
    // This will be set in the Inspector.
    public enum ProjectileType { Player, Enemy }

    [Header("Projectile Settings")]
    public ProjectileType type;
    public float speed = 3f;
    public float lifetime = 5f; // Time in seconds before the projectile destroys itself

    void Start()
    {
        // Automatically destroy the projectile after its lifetime expires
        // to prevent cluttering the scene.
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Use transform.up to move in the local "forward" direction.
        // This works correctly as long as the prefab is rotated properly
        // (0 for player projectiles, 180 for enemy projectiles).
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    // This method handles all collisions for this projectile.
    void OnTriggerEnter2D(Collider2D other)
    {
        // Try to get the Projectile component from the object we hit.
        Projectile otherProjectile = other.GetComponent<Projectile>();

        // Check if we hit another projectile.
        if (otherProjectile != null)
        {
            // If we hit a projectile of a DIFFERENT type (e.g., Player hits Enemy)...
            if (this.type != otherProjectile.type)
            {
                Debug.Log("Projectile collision: " + this.type + " hit " + otherProjectile.type);

                // Destroy this projectile.
                Destroy(gameObject);

                // Note: The other projectile will also detect this collision
                // and destroy itself, so we don't need to destroy it here.
            }
            // If we hit a projectile of the SAME type, nothing happens.
        }

        // IMPORTANT: We do NOT handle hitting the Player or Enemy here.
        // The PlayerCollisionHandler and DetectCollisions2D scripts are already
        // responsible for that, which is the correct design. They will destroy
        // this projectile when it hits them.
    }
}