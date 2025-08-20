using UnityEngine;
using System.Collections;
using System;


[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlaneHealth))] // This enemy now requires an PlaneHealth script
public class EnemyCollisionHandler : MonoBehaviour, PlaneCollisionHandler, Damageable
{
    // Public variables for effects and points
    public AudioClip collisionSound;
    public ParticleSystem enemyDestroyedExplosion;
    public int pointsAwarded = 10;

    public GameObject explosionSprite;
    public float explosionSize = 1;

    [Header("Item Drop Settings")]
    [Tooltip("The LifeBonus prefab this enemy can drop on defeat.")]
    [SerializeField] private GameObject lifeBonusPrefab;
    [Tooltip("The chance (from 0 to 100) that this enemy will drop the bonus.")]
    [Range(0, 100)] [SerializeField] private float dropChance = 100f; // This creates a 10% chance

    public static event Action<GameObject> OnAnyEnemyDefeated;

    // Private component references
    private AudioSource audioSource;
    private PlaneHealth enemyHealth; // Reference to the health script

    // Tags for collision checking
    private const string ProjectileTag = "Projectile";
    private const string EnemyProjectileTag = "EnemyProjectile";
    private const string PlayerTag = "Player";

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        enemyHealth = GetComponent<PlaneHealth>(); // Get the PlaneHealth script on this object


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
        if (explosionSprite == null)
        {
            Debug.LogWarning("Explosion Sprite not assigned in the Inspector for " + gameObject.name, gameObject);
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
        // This script NO LONGER damages the player. It only handles itself.

        if (other.CompareTag(PlayerTag))
        {
            // --- Player Collision Logic ---
            // The enemy simply destroys itself when it hits the player.
            // The Player's script is now responsible for deducting a life.
            Debug.Log("Enemy collided with Player. Destroying self.");
            HandleDefeat(); 
        }
        /*else if (other.CompareTag(ProjectileTag))
        {
            // --- Projectile Collision Logic ---
            Debug.Log(gameObject.name + " collided with a player projectile.");
            Destroy(other.gameObject);
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1);
            }
        }*/
    }

    /// <summary>
    /// This method is now called by the PlaneHealth script when health reaches zero.
    /// </summary>
    public void HandleDefeat()
    {
        Debug.Log(gameObject.name + " has been defeated.");

        // Update score
        if (GameManager2D.Instance != null)
        {
            GameManager2D.Instance.UpdateScore(pointsAwarded);
        }

        // Stop smoke trail
        //enemySmokeTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        //Destroy(enemySmokeTrail);

        // --- NEW LOGIC ---
        // Check if we should spawn a life bonus.
        if (lifeBonusPrefab != null && UnityEngine.Random.Range(0f, 100f) <= dropChance)
        {
            // Create the life bonus prefab at the enemy's current position.
            Instantiate(lifeBonusPrefab, transform.position, Quaternion.identity);
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


        // Notify the SpawnManager that an enemy has been destroyed.
        SpawnManager2D.Instance.OnEnemyDestroyed();
        
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
            AudioSource.PlayClipAtPoint(collisionSound, Camera.main.transform.position);
        }

        // commented out for now, we could add particles in addition to the sprite at some point though
        /*// Play particle effect
        if (enemyDestroyedExplosion != null)
        {
            // Instantiate the particle effect prefab at this position so it's not destroyed with the enemy
            ParticleSystem explosionInstance = Instantiate(enemyDestroyedExplosion, transform.position, transform.rotation);
            explosionInstance.Play(); // Explicitly play the instantiated system
            // Note: The explosionInstance prefab should have a script or a "Stop Action" set to "Destroy"
            // to clean itself up after it has finished playing.
        }*/

        // Spawn explosion sprite
        if (explosionSprite != null)
        {
            GameObject explosionInstance = Instantiate(explosionSprite, transform);
            explosionInstance.transform.SetParent(transform);
            explosionInstance.transform.localScale = explosionInstance.transform.localScale * explosionSize;
        }
    }

    public void TakeDamage(int damage)
    {
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
    }



}
