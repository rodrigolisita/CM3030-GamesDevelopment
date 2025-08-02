using UnityEngine;

// NEW: This line ensures the GameObject always has an AudioSource component.
[RequireComponent(typeof(AudioSource))]
[RequireComponent (typeof(PlaneHealth))]
public class PlayerCollisionHandler : MonoBehaviour, PlaneCollisionHandler
{
    [Header("Effects")]
    public ParticleSystem explosionParticle;
    public AudioClip playerHitSound; // NEW: Variable to hold the sound clip.
    public GameObject explosionSprite;
    public float explosionSize = 1.0f;

    // NEW: Private reference to the AudioSource component.
    private AudioSource audioSource;

    private PlaneHealth playerHealth;

    // NEW: The Start method is used to get component references.
    void Start()
    {
        // Get the AudioSource component that is on this same GameObject.
        audioSource = GetComponent<AudioSource>();
        playerHealth = GetComponent<PlaneHealth>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // If we get hit by an EnemyProjectile OR an Enemy ship...
        if (other.CompareTag("EnemyProjectile") || other.CompareTag("Enemy"))
        {
            Debug.Log("Player has been hit by: " + other.name);

            // --- NEW: Play the hit sound ---
            // Check if the audioSource and the sound clip are assigned to prevent errors.
            if (audioSource != null && playerHitSound != null)
            {
                // Play the sound effect once.
                audioSource.PlayOneShot(playerHitSound);
            }

            // Destroy the projectile if that's what hit us
            if (other.CompareTag("EnemyProjectile"))
            {
                Destroy(other.gameObject);
            }

            // Play our own explosion particle effect
            /*if (explosionParticle != null)
            {
                explosionParticle.Play();

            }*/

            
                
            // Tell our PlaneHealth we lost a life
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
                // Inform the game manager of our health
                if (GameManager2D.Instance != null)
                {
                    GameManager2D.Instance.UpdateHealth(playerHealth.GetHealth());
                }
            }

            
        }
    }

    public void HandleDefeat()
    {
        // Play the explosion sprite animation
        if (explosionSprite != null)
        {
            GameObject explosionInstance = Instantiate(explosionSprite, transform);
            explosionInstance.transform.SetParent(transform);
            explosionInstance.transform.localScale = explosionInstance.transform.localScale * explosionSize;
        }

        if (GameManager2D.Instance != null)
        {
            GameManager2D.Instance.HandlePlayerDefeat();
        }
    }

}