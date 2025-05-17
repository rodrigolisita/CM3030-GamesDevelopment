using UnityEngine;
using System.Collections; // Required for IEnumerator and StartCoroutine

// Require an AudioSource component to be attached to the same GameObject.
// This will automatically add an AudioSource if one doesn't exist when you add the script.
[RequireComponent(typeof(AudioSource))]
public class DetectCollisions : MonoBehaviour
{
    // Public variable to assign your collision sound effect in the Inspector
    public AudioClip collisionSound;

    // Private variable to hold the AudioSource component
    private AudioSource audioSource;

    // Define the tag for projectiles - ensure your projectile prefabs have this tag
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

        // You can add any initialization for the enemy here if needed
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
        Debug.Log(gameObject.name + " triggered with: " + other.gameObject.name + " (Tag: " + other.gameObject.tag + ")");

        bool isPlayerCollision = other.gameObject.CompareTag(PlayerTag);
        bool isProjectileCollision = other.gameObject.CompareTag(ProjectileTag);

        AudioClip clipToPlay = null;
        float soundLength = 0.5f; // Default delay if no clip length can be found

        // Determine which clip to play and get its length
        if (collisionSound != null)
        {
            clipToPlay = collisionSound;
            soundLength = clipToPlay.length;
        }
        else if (audioSource != null && audioSource.clip != null)
        {
            clipToPlay = audioSource.clip;
            soundLength = clipToPlay.length;
            Debug.LogWarning("Playing default clip from AudioSource as 'collisionSound' was not set in script for " + gameObject.name + ". Consider setting the public 'collisionSound' variable.", gameObject);
        }


        // Play the collision sound if it's a player OR a projectile collision
        if (isPlayerCollision || isProjectileCollision)
        {
            if (audioSource != null && clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
            else if (audioSource != null) // No clip assigned anywhere
            {
                Debug.LogWarning("AudioSource found on " + gameObject.name + " but no AudioClip is assigned to the script's 'collisionSound' field or the AudioSource's 'AudioClip' field. No sound will play.", gameObject);
            }
        }

        // Handle specific game logic based on collision type
        if (isPlayerCollision)
        {
            // If the tag is "Player", print "Game Over" to the Unity console
            Debug.Log("Game Over");
            // Example: Destroy player and then enemy after sound
            // Destroy(other.gameObject); 
            // Destroy(gameObject, soundLength > 0 ? soundLength : 0.5f); 
            // Or handle game over state differently
        }
        else if (isProjectileCollision)
        {
            // If the collision is with a projectile
            Debug.Log(gameObject.name + " collided with a projectile: " + other.gameObject.name + ". Destroying projectile and this enemy (after sound).");
            
            // Disable the enemy's collider and renderer immediately so it can't interact further
            // and appears to be gone, but delay the actual destruction of the GameObject.
            Collider enemyCollider = GetComponent<Collider>();
            if(enemyCollider != null) enemyCollider.enabled = false;
            
            Renderer enemyRenderer = GetComponent<Renderer>();
            if(enemyRenderer != null) enemyRenderer.enabled = false;
            // If you have child renderers, you might need to loop through them:
            // foreach(Renderer r in GetComponentsInChildren<Renderer>()) { r.enabled = false; }


            Destroy(other.gameObject); // Destroy the projectile immediately
            Destroy(gameObject, soundLength); // Destroy this enemy GameObject after the sound has had time to play
        }
        else
        {
            // This block executes if the collision is with ANY object NOT tagged "Player" OR "Projectile"
            // For example, colliding with a wall or other environmental objects.
            // Consider if sound should play here too.
            Debug.Log(gameObject.name + " collided with a non-player, non-projectile object: " + other.gameObject.name + ".");
            // Decide what to do:
            // Destroy(gameObject);        
            // Destroy(other.gameObject);  
        }
    }
}
