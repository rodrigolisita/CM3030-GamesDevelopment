using UnityEngine;

// Require an AudioSource component to be attached to the same GameObject.
// This will automatically add an AudioSource if one doesn't exist when you add this script.
[RequireComponent(typeof(AudioSource))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("The speed of the player's horizontal movement.")]
    public float horizontalSpeed = 5.0f;
    [Tooltip("The speed of the player's vertical movement.")]
    public float verticalSpeed = 2.0f;  
            
    [Header("Combat")]
    [Tooltip("The projectile prefab that the player will fire.")]
    public GameObject projectilePrefab;
    [Tooltip("The number of shots the player can fire per minute.")]
    public float roundsPerMinute = 800;

    [Header("Effects & Audio")]
    [Tooltip("The sound effect that plays when the player shoots.")]
    public AudioClip shootingSound; // Public variable to assign your shooting sound effect
    [Tooltip("The particle system that plays when the player is hit or destroyed.")]
    public ParticleSystem explosionParticle;

    [Header("Component References")]
    [Tooltip("The Animator component that controls the plane's animations.")]
    [SerializeField]
    private Animator planeAnimController;

    // Private variables for internal logic
    private AudioSource playerAudioSource; // Private variable to hold the AudioSource component
    private bool firing = false;
    private float burstStartTime = 0;
    private int shotsFiredThisBurst = 0;
    private float lastShotTime = 0;
    

    // Game Manager
    GameManager2D gameManager2D;

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

        // Game Manager
        gameManager2D = GameObject.Find("GameManager2D").GetComponent<GameManager2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((GameManager2D.Instance.gameState == GameState.Active))
        {
            // --- Movement logic ---
            // Move player left and right
            float horizontalInput = Input.GetAxisRaw("Horizontal");
             
            // Move player top and bottom
            float verticalInput = Input.GetAxisRaw("Vertical");
            //transform.Translate(Vector3.up * verticalInput * Time.deltaTime * speed);

            // Calculate the movement for each axis individually using the separate speeds
            float xMovement = horizontalInput * horizontalSpeed;
            float yMovement = verticalInput * verticalSpeed;

            // Create a final movement vector from the individual calculations
            Vector3 movementVelocity = new Vector3(xMovement, yMovement, 0);

            // Move the player in the calculated direction
            transform.Translate(movementVelocity * Time.deltaTime);

            // --- BOUNDARY LOGIC ---
            // Get the current position
            Vector3 currentPosition = transform.position;

            // Clamp both the X and Y positions using the BoundaryManager
            currentPosition.x = Mathf.Clamp(currentPosition.x, BoundaryManager.Instance.MinX, BoundaryManager.Instance.MaxX);
            currentPosition.y = Mathf.Clamp(currentPosition.y, BoundaryManager.Instance.MinY, BoundaryManager.Instance.MaxY);
        
            // Apply the clamped position
            transform.position = currentPosition;
            

            // Instantiate the projectile and play sound
            if (Input.GetKey(KeyCode.Space))
            {
                float curTime = Time.time;
                //Check if the weapon can fire another shot yet
                //Debug.Log("Time.time = " + Time.time + "\nlastShotTime = " + lastShotTime + "\n burstStartTime = " + burstStartTime + "\n shotsFiredThisBurst = " + shotsFiredThisBurst);
                if (((!firing) && ((curTime - lastShotTime) > (60 / roundsPerMinute))) || firing && (shotsFiredThisBurst < (roundsPerMinute / 60) * (curTime - burstStartTime)))
                {
                    //update the info on the shot
                    lastShotTime = curTime;

                    if (firing)
                    {
                        shotsFiredThisBurst += 1;
                    } else
                    {
                        firing = true;
                        burstStartTime = curTime;
                        shotsFiredThisBurst = 1;
                    }

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

                    // Play the shooting animation
                    planeAnimController.SetTrigger("isFiring");
                }
            } else
            {
                firing = false; // Stop the current burst if space bar is not pressed
            }
        }
    }

    
}
