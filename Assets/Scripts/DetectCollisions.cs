using UnityEngine;
using System.Collections; // Required for IEnumerator and StartCoroutine

// Require an AudioSource component to be attached to the same GameObject.
// This will automatically add an AudioSource if one doesn't exist when the script is added.
[RequireComponent(typeof(AudioSource))]
public class DetectCollisions : MonoBehaviour
{
    // Public variable to assign your enemy's collision sound effect in the Inspector
    public AudioClip collisionSound;
    // Public variable to assign the particle system for the enemy's own hit/destruction effect
    public ParticleSystem enemyHitParticle;
    // Public variable to define how many points this enemy is worth
    public int pointsAwarded = 10; 


    // Private variable to hold the AudioSource component
    private AudioSource audioSource;

    // Define the tag for projectiles - ensure the projectile prefabs have this tag
    private const string ProjectileTag = "Projectile";
    private const string PlayerTag = "Player";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();

        // Check if the AudioSource component was found
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found on " + gameObject.name + ". Please add one.", gameObject);
        }

        // Check if a collision sound has been assigned in the Inspector
        if (collisionSound == null && (audioSource != null && audioSource.clip == null) ) // Only warn if no clip is set anywhere
        {
            Debug.LogWarning("Collision sound not assigned in the Inspector for " + gameObject.name + 
                             ", and no default clip on AudioSource. Please ensure a sound is set if you want collision sounds.", gameObject);
        }

        // Check if the enemy hit particle system has been assigned
        if (enemyHitParticle == null)
        {
            Debug.LogWarning("Enemy Hit Particle System not assigned in the Inspector for " + gameObject.name + ". Please assign it if you want a hit effect.", gameObject);
        }
        else
        {
            // Ensure the particle system doesn't play on awake by default
            var mainModule = enemyHitParticle.main; // Renamed for clarity
            mainModule.playOnAwake = false;
            // IMPORTANT: Also ensure its 'Stop Action' in the Inspector is NOT set to 'Destroy' if this particle system's GameObject is the enemy itself or a critical child.
            // If 'Stop Action' is 'Destroy', the GameObject holding the particle system will be destroyed after it finishes playing.
            // It's generally better to manage destruction via this script's Destroy(gameObject, delay) calls.
            if (mainModule.stopAction == ParticleSystemStopAction.Destroy)
            {
                Debug.LogWarning("The 'Stop Action' for 'enemyHitParticle' on " + gameObject.name + " is set to 'Destroy'. This might cause issues if the particle system's GameObject is the enemy itself or an essential child. Consider setting it to 'None' or 'Disable' and letting this script handle destruction.", enemyHitParticle.gameObject);
            }
        }

        // Add any initialization for the enemy here if needed
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError("Enemy prefab is missing a Collider component!", gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Usually, collision detection logic goes into OnTriggerEnter or OnCollisionEnter,
        // not Update, unless you're doing manual overlap checks.
    }

    void OnTriggerEnter(Collider other)
    {
        // Log every collision to see what this enemy is interacting with
        Debug.Log(gameObject.name + " (Enemy) triggered with: " + other.gameObject.name + " (Tag: " + other.gameObject.tag + ")");

        bool isPlayerCollision = other.gameObject.CompareTag(PlayerTag);
        bool isProjectileCollision = other.gameObject.CompareTag(ProjectileTag);

        AudioClip clipToPlay = null;
        float soundLength = 0.5f; // Default delay if no clip length can be found

        // Determine which clip to play for the enemy's own sound
        if (collisionSound != null)
        {
            clipToPlay = collisionSound;
            soundLength = clipToPlay.length;
        }
        else if (audioSource != null && audioSource.clip != null)
        {
            clipToPlay = audioSource.clip;
            soundLength = clipToPlay.length;
            Debug.LogWarning("Playing default enemy collision sound from AudioSource as 'collisionSound' was not set in script for " + gameObject.name, gameObject);
        }

        // Play the enemy's collision sound AND its own hit particle if it's a player OR a projectile collision
        if (isPlayerCollision || isProjectileCollision)
        {
            // Play enemy's own sound
            if (audioSource != null && clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
            else if (audioSource != null) // No clip assigned anywhere for enemy sound
            {
                Debug.LogWarning("Enemy AudioSource found on " + gameObject.name + " but no AudioClip is assigned for its collision sound.", gameObject);
            }

            // Play enemy's own hit particle effect
            if (enemyHitParticle != null)
            {
                // Log particle system state before playing
                Debug.Log("Attempting to play enemyHitParticle on " + gameObject.name +
                          ". IsPlaying (before stop/clear): " + enemyHitParticle.isPlaying +
                          ", ParticleCount (before stop/clear): " + enemyHitParticle.particleCount +
                          ", Main.Duration: " + enemyHitParticle.main.duration +
                          ", Emission.enabled: " + enemyHitParticle.emission.enabled +
                          ", Renderer.enabled: " + (enemyHitParticle.GetComponent<ParticleSystemRenderer>() != null ? enemyHitParticle.GetComponent<ParticleSystemRenderer>().enabled.ToString() : "NoRenderer") +
                          ", GameObject Active: " + enemyHitParticle.gameObject.activeInHierarchy);
                
                // Explicitly stop and clear the particle system to ensure a fresh play.
                enemyHitParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // Stop emission & clear particles.
                // The 'true' argument affects children particle systems as well.
                // ParticleSystemStopBehavior.StopEmittingAndClear is generally a good choice for a hard reset.

                enemyHitParticle.Play(true); // Play, including children.
                Debug.Log("Enemy hit particle Play() command issued on " + gameObject.name + ". IsPlaying (after play): " + enemyHitParticle.isPlaying);
            }
        }

        // Handle specific game logic based on collision type
        if (isPlayerCollision)
        {
            Debug.Log("Enemy collided with Player. Attempting to trigger Player's explosion particle and reduce life.");

            // --- Reduce Player's Life ---
            if (PlayerStatsManager.Instance != null)
            {
                PlayerStatsManager.Instance.LoseLife();
            }
            else
            {
                Debug.LogError("PlayerStatsManager.Instance is null! Cannot call LoseLife(). Ensure PlayerStatsManager is in the scene and correctly initialized.");
            }
            // --- End of Reduce Player's Life ---


            // Attempt to find the PlayerController script on the player GameObject
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();

            if (playerController != null) // Check if PlayerController component exists
            {
                // Check if the Player's explosionParticle reference itself is not null AND
                // the GameObject of the particle system has not been destroyed.
                if (playerController.explosionParticle != null && playerController.explosionParticle.gameObject != null) 
                {
                    // Play the player's explosion particle system
                    playerController.explosionParticle.Play();
                    Debug.Log("Player's explosion particle system triggered by enemy.");
                }
                else
                {
                    Debug.LogWarning("PlayerController found on Player, but its explosionParticle is not assigned, or the particle system GameObject has been destroyed.", other.gameObject);
                }
            }
            else
            {
                // This means the Player GameObject itself might have been destroyed, or it doesn't have the PlayerController script.
                Debug.LogWarning("PlayerController script not found on the collided Player object (it might have been destroyed or is missing the script).", other.gameObject);
            }

            // IMPORTANT: Player Destruction Logic & Enemy Destruction Logic
            // This script currently DOES NOT destroy the player OR THE ENEMY when colliding with the player.
            // The PlayerStatsManager should handle what happens when lives reach zero (e.g., destroying the player, showing game over screen).
            // If the enemy should be destroyed after colliding with the player (regardless of player lives), add that logic here.
            // For example, after playing its sound and particle:
            // float enemyDestructionDelayPlayerCollision = soundLength;
            // if (enemyHitParticle != null && enemyHitParticle.main.duration > soundLength)
            // {
            //     enemyDestructionDelayPlayerCollision = enemyHitParticle.main.duration;
            // }
            // Destroy(gameObject, enemyDestructionDelayPlayerCollision); // Example: Destroy this enemy
        }
        else if (isProjectileCollision)
        {
            // If the collision is with a projectile
            Debug.Log(gameObject.name + " (Enemy) collided with a projectile: " + other.gameObject.name + ". Destroying projectile and this enemy (after sound/particle).");
            
             // --- Score Update Logic ---
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.UpdateScore(pointsAwarded); 
                Debug.Log("Score updated by " + pointsAwarded + " for destroying " + gameObject.name);
            }
            else
            {
                Debug.LogWarning("ScoreManager.Instance is null. Cannot update score.");
            }
            // --- End of Score Update ---

            Collider enemyCollider = GetComponent<Collider>();
            if(enemyCollider != null) enemyCollider.enabled = false;
            
            Renderer enemyRenderer = GetComponent<Renderer>();
            if(enemyRenderer != null) enemyRenderer.enabled = false;
            // If you have child renderers, you might need to loop through them:
            // foreach(Renderer r in GetComponentsInChildren<Renderer>()) { r.enabled = false; }

            Destroy(other.gameObject); // Destroy the projectile immediately
            // Delay destruction of enemy by the longer of its sound or its particle duration
            float enemyDestructionDelay = soundLength;
            if (enemyHitParticle != null && enemyHitParticle.main.duration > soundLength)
            {
                enemyDestructionDelay = enemyHitParticle.main.duration;
            }
            Destroy(gameObject, enemyDestructionDelay); 
        }
        else
        {
            // This block executes if the collision is with ANY object NOT tagged "Player" OR "Projectile"
            Debug.Log(gameObject.name + " (Enemy) collided with a non-player, non-projectile object: " + other.gameObject.name + ".");
        }
    }
}
