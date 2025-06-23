using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public enum ProjectileType { Player, Enemy }

    [Header("Projectile Settings")]
    public ProjectileType type;
    public float speed = 3f;
    public float lifetime = 5f;
    public AudioClip hitSound;

    private Rigidbody2D rb; // A reference to the Rigidbody2D component

    void Awake() // Using Awake to get references early
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Projectile is missing a Rigidbody2D component!", this.gameObject);
        }
    }

    void Start()
    {
        // MOVEMENT LOGIC
        if (rb != null)
        {
            rb.linearVelocity = transform.up * speed;
        }

        Destroy(gameObject, lifetime);
    }

    // --- The Update() method is not needed for this script. ---

    void OnTriggerEnter2D(Collider2D other)
    {
        Projectile otherProjectile = other.GetComponent<Projectile>();
        if (otherProjectile != null)
        {
            if (this.type != otherProjectile.type)
            {
                Debug.Log("Projectile collision: " + this.type + " hit " + otherProjectile.type);

                if (hitSound != null)
                {
                    // AudioSource.PlayClipAtPoint(hitSound, transform.position);
                    AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position);

                }

                Destroy(gameObject);
            }
        }
    }
}