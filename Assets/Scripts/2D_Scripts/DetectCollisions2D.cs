using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(EnemyHealth))] // This enemy now requires an EnemyHealth script
public class DetectCollisions2D : MonoBehaviour
{
    // Public variables for effects and points
    public AudioClip collisionSound;
    public ParticleSystem enemyDestroyedExplosion;
    public int pointsAwarded = 10;

    // Private component references
    private AudioSource audioSource;
    private EnemyHealth enemyHealth; // Reference to the health script

    // Tags for collision checking
    private const string ProjectileTag = "Projectile";
    private const string PlayerTag = "Player";

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        enemyHealth = GetComponent<EnemyHealth>(); // Get the EnemyHealth script on this object

        // --- Component and reference checks ---
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found on " + gameObject.name, gameObject);
        }
        if (enemyHealth == null)
        {
            Debug.LogError("EnemyHealth component not found on " + gameObject.name, gameObject);
        }
        if (collisionSound == null && (audioSource != null && audioSource.clip == null))
        {
            Debug.LogWarning("Collision sound not assigned in the Inspector for " + gameObject.name, gameObject);
        }
        if (enemyDestroyedExplosion == null)
        {
            Debug.LogWarning("Enemy Hit Particle System not assigned in the Inspector for " + gameObject.name, gameObject);
        }
        else
        {
            var mainModule = enemyDestroyedExplosion.main;
            mainModule.playOnAwake = false;
            if (mainModule.stopAction == ParticleSystemStopAction.Destroy)
            {
                Debug.LogWarning("The 'Stop Action' for 'enemyDestroyedExplosion' on " + gameObject.name + " is set to 'Destroy'. Consider setting to 'None' or 'Disable'.", enemyDestroyedExplosion.gameObject);
            }
        }
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError("Enemy prefab is missing a Collider2D component!", gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayerCollision = other.CompareTag(PlayerTag);
        bool isProjectileCollision = other.CompareTag(ProjectileTag);

        if (isPlayerCollision)
        {
            // --- Player Collision Logic ---
            Debug.Log("Enemy collided with Player. Triggering effects.");

            // Play the enemy's own hit/destruction sound and particle effect
            PlayHitEffects();
            
            // Tell GameManager the player lost a life
            if (GameManager2D.Instance != null)
            {
                GameManager2D.Instance.LoseLife();
            }

            // Attempt to find the PlayerController script to trigger its explosion
            PlayerController2D playerController = other.gameObject.GetComponent<PlayerController2D>();
            if (playerController != null) 
            {
                // Check if the Player has an explosion particle assigned
                if (playerController.explosionParticle != null) 
                {
                    // Play the player's own explosion particle system
                    playerController.explosionParticle.Play();
                    Debug.Log("Player's explosion particle system triggered by enemy.");
                }
                else
                {
                    Debug.LogWarning("PlayerController found on Player, but its explosionParticle is not assigned.", other.gameObject);
                }
            }
            else
            {
                Debug.LogWarning("PlayerController2D script not found on the collided Player object.", other.gameObject);
            }


            // Immediately destroy this enemy when it hits the player
            Destroy(gameObject);
        }
        else if (isProjectileCollision)
        {
            // --- Projectile Collision Logic ---
            Debug.Log(gameObject.name + " collided with a projectile.");

            // Destroy the projectile that hit the enemy
            Destroy(other.gameObject);

            // Tell the health script to take damage
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1); // Assuming each projectile does 1 damage
            }
        }
    }

    /// <summary>
    /// This method is now called by the EnemyHealth script when health reaches zero.
    /// </summary>
    public void HandleEnemyDefeat()
    {
        Debug.Log(gameObject.name + " has been defeated.");

        // Update score
        if (GameManager2D.Instance != null)
        {
            GameManager2D.Instance.UpdateScore(pointsAwarded);
        }

        // Play final destruction sound and particle effects
        PlayHitEffects();

        // Disable components to prevent further interaction while effects play out
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider != null) enemyCollider.enabled = false;

        Renderer enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null) enemyRenderer.enabled = false;

        // Calculate destruction delay based on sound or particle duration
        float soundLength = (collisionSound != null) ? collisionSound.length : 0.5f;
        float particleDuration = (enemyDestroyedExplosion != null) ? enemyDestroyedExplosion.main.duration : 0f;
        float destructionDelay = Mathf.Max(soundLength, particleDuration);
        
        // Destroy this enemy GameObject after the delay
        Destroy(gameObject, destructionDelay);
    }

    /// <summary>
    /// A helper method to play sound and particle effects to reduce code duplication.
    /// </summary>
    private void PlayHitEffects()
    {
        // Play collision sound
        if (audioSource != null && collisionSound != null)
        {
            // Play sound at the position of this enemy, independent of its destruction
            AudioSource.PlayClipAtPoint(collisionSound, transform.position);
        }

        // Play particle effect
        if (enemyDestroyedExplosion != null)
        {
            // Instantiate the particle effect prefab at this position so it's not destroyed with the enemy
            ParticleSystem explosionInstance = Instantiate(enemyDestroyedExplosion, transform.position, transform.rotation);
            explosionInstance.Play(); // Explicitly play the instantiated system
            // Note: The explosionInstance prefab should have a script or a "Stop Action" set to "Destroy"
            // to clean itself up after it has finished playing.
        }
    }
}
