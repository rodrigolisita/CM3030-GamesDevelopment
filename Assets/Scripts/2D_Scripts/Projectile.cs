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
        // player projectile logic
        if (type == ProjectileType.Player)
        {
            if (other.CompareTag(EnemyTag)) {
                Damageable enemy = other.GetComponent<Damageable>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }
        // enemy projectile logic
        else if (type == ProjectileType.Enemy)
        {
            if (other.CompareTag(PlayerTag))
            {
                Damageable player = other.GetComponent<Damageable>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }

    }

    // Called when a player projectile hits an enemy, or vice versa. Overridden by more complex projectiles like bombs.
    public virtual void HitOpponent(Collider2D opponentCollider)
    {
        Damageable opponentDamageable = opponentCollider.GetComponent<Damageable>();
        if (opponentDamageable != null)
        {
            opponentDamageable.TakeDamage(damage);
            Destroy(gameObject);
        }
    }


}