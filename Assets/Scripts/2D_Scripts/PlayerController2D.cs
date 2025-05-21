using UnityEngine;

// Require an AudioSource component to be attached to the same GameObject.
// This will automatically add an AudioSource if one doesn't exist when you add this script.
[RequireComponent(typeof(AudioSource))]
public class PlayerController2D : MonoBehaviour
{
    public float horizontalInput;
    public float speed = 10.0f;
    public float xRange = 24;

    public GameObject projectilePrefab;
    public AudioClip shootingSound; // Public variable to assign your shooting sound effect

    private AudioSource playerAudioSource; // Private variable to hold the AudioSource component

    //public ParticleSystem explosionParticle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the AudioSource component attached to this GameObject
        playerAudioSource = GetComponent<AudioSource>();

        // Check if the AudioSource component was found and configure it
        if (playerAudioSource != null)
        {
            // Ensure the sound doesn't play automatically when the game starts
            playerAudioSource.playOnAwake = false;
            // Ensure the AudioSource itself isn't set to loop, PlayOneShot handles its own playback.
            playerAudioSource.loop = false;
        }
        else
        {
            Debug.LogError("Player AudioSource component not found on " + gameObject.name + ". Please add one.", gameObject);
        }

        // Check if a shooting sound has been assigned in the Inspector
        // This warning is still useful to ensure a clip is available for shooting.
        if (shootingSound == null && (playerAudioSource != null && playerAudioSource.clip == null)) // Only warn if no clip is set anywhere for shooting
        {
            Debug.LogWarning("Shooting sound not assigned in the Inspector for " + gameObject.name +
                             ", and no default clip on AudioSource. Please ensure a sound is set if you want shooting sounds.", gameObject);
        }

        //// Check if the explosion particle system has been assigned
        //if (explosionParticle == null)
        //{
        //    Debug.LogError("Explosion Particle System not assigned in the Inspector for " + gameObject.name + ". Please assign it. It will be triggered by other scripts (e.g., Enemy).", gameObject);
        //}
        //else
        //{
        //    // Ensure the particle system doesn't play on awake by default
        //    // You might have already set this in the Inspector, but it's good practice.
        //    var main = explosionParticle.main;
        //    main.playOnAwake = false;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        // Move player left and right
        horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * Time.deltaTime * speed);

        // Ensure the player stays within x bounds
        if (transform.position.x < -xRange)
        {
            transform.position = new Vector3(-xRange, transform.position.y, transform.position.z);
        }
        if (transform.position.x > xRange)
        {
            transform.position = new Vector3(xRange, transform.position.y, transform.position.z);
        }

        // Instantiate the projectile and play sound
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Launch a projectile from the player
            Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);

            // Play the shooting sound
            if (playerAudioSource != null && shootingSound != null)
            {
                playerAudioSource.PlayOneShot(shootingSound);
            }
            // Fallback to play clip directly on AudioSource if shootingSound (script variable) isn't set
            else if (playerAudioSource != null && playerAudioSource.clip != null)
            {
                 Debug.LogWarning("Playing default clip from Player's AudioSource as 'shootingSound' was not set in script for " + gameObject.name + ". Consider setting the public 'shootingSound' variable.", gameObject);
                playerAudioSource.PlayOneShot(playerAudioSource.clip);
            }
            else if (playerAudioSource != null) // No clip assigned anywhere
            {
                 Debug.LogWarning("Player AudioSource found on " + gameObject.name + " but no AudioClip is assigned to the script's 'shootingSound' field or the AudioSource's 'AudioClip' field. No sound will play.", gameObject);
            }
        }
    }

    
}
