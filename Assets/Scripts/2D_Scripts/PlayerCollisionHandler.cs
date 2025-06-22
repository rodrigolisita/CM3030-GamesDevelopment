using UnityEngine;

// NEW: This line ensures the GameObject always has an AudioSource component.
[RequireComponent(typeof(AudioSource))]
public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Effects")]
    public ParticleSystem explosionParticle;
    public AudioClip playerHitSound; // NEW: Variable to hold the sound clip.

    // NEW: Private reference to the AudioSource component.
    private AudioSource audioSource;

    // NEW: The Start method is used to get component references.
    void Start()
    {
        // Get the AudioSource component that is on this same GameObject.
        audioSource = GetComponent<AudioSource>();
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
            if (explosionParticle != null)
            {
                explosionParticle.Play();
            }

            // Tell the GameManager we lost a life
            if (GameManager2D.Instance != null)
            {
                GameManager2D.Instance.LoseLife();
            }
        }
    }
}