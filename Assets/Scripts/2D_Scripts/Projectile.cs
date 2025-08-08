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
    public int damage = 1;

    private Rigidbody2D rb; // A reference to the Rigidbody2D component

    private const string ProjectileTag = "Projectile";
    private const string EnemyProjectileTag = "EnemyProjectile";
    private const string PlayerTag = "Player";
    private const string EnemyTag = "Enemy";

    public void Awake() // Using Awake to get references early
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Projectile is missing a Rigidbody2D component!", this.gameObject);
        }
    }

    public void Start()
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
        if (ShouldTrigger(other))
        {
            TriggerProjectileEffect(other);
        }

    }

    public bool ShouldTrigger(Collider2D other)
    {
        // player projectile logic
        if (type == ProjectileType.Player)
        {
            if (other.CompareTag(EnemyTag))
            {
                return true;
            }
        }
        // enemy projectile logic
        else if (type == ProjectileType.Enemy)
        {
            if (other.CompareTag(PlayerTag))
            {
                return true;
            }
        }
        return false;
    }

    // Called when a player projectile hits an enemy, or vice versa. Overridden by more complex projectiles like bombs.
    public virtual void TriggerProjectileEffect(Collider2D other)
    {
        if (DealDamageTo(other))
        {
            Destroy(gameObject);
        }
    }

    public bool DealDamageTo(Collider2D other)
    {
        Damageable otherDamageable = other.GetComponent<Damageable>();
        if (otherDamageable != null)
        {
            otherDamageable.TakeDamage(damage);
            return true;
        }
        return false;
    }

    public void DisableForExplosion()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;
    }


}