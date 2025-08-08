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
    [Tooltip("The player's primary weapon.")]
    public GameObject primaryWeapon;
    private Weapon primaryWeaponScript;
    [Tooltip("The player's secondary weapon.")]
    public GameObject secondaryWeapon;
    private Weapon secondaryWeaponScript;

    

    [Header("Effects & Audio")]
    [Tooltip("The particle system that plays when the player is hit or destroyed.")]
    public ParticleSystem explosionParticle;

    /*[Header("Component References")]
    [Tooltip("The Animator component that controls the plane's animations.")]
    [SerializeField]
    private Animator planeAnimController;*/

    // Private variables for internal logic
    private AudioSource playerAudioSource; // Private variable to hold the AudioSource component
    


    // Game Manager
    GameManager2D gameManager2D;
    
    // This will hold a reference to the upgrade manager component.
    private PlayerUpgradeManager playerUpgradeManager;

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

        playerUpgradeManager = GetComponent<PlayerUpgradeManager>();

        
        
        if (primaryWeapon != null)
        {
            primaryWeaponScript = primaryWeapon.GetComponent<Weapon>();
        }
        

        if (secondaryWeapon != null)
        {
            secondaryWeaponScript = secondaryWeapon.GetComponent<Weapon>();
        }
        
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
            

            //Handle primary fire
            if (Input.GetKey(KeyCode.Space))
            {
                primaryWeaponScript.PullTrigger();
            } else
            {
                primaryWeaponScript.CeaseFire();
            }

            //Handle secondary fire
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                secondaryWeaponScript.PullTrigger();
            }
            else
            {
                secondaryWeaponScript.CeaseFire();
            }
        }
    }

    // Public method receives an UpgradeData asset and applies its values.
    //public void ApplyUpgrade(UpgradeData upgrade)
    //{
    //    if (upgrade == null) return;
    
        // The upgrade asset itself contains all the logic for how to apply it.
    //    upgrade.Apply(this.gameObject);
    //}

     public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null) return;

        // Pass the upgrade to the manager to handle its application and timing.
        playerUpgradeManager.AddUpgrade(upgrade);

        // After applying an upgrade, tell the GameManager to update the stats UI.
        if (GameManager2D.Instance != null)
        {
            GameManager2D.Instance.UpdatePlayerStatsUI();
        }
    }
    
    /// <summary>
    /// Swaps to an entirely new Weapon game object
    /// </summary>
    /// <param name="newWeapon"></param>
    /// <param name="weaponKey"></param>
    public void SwapWeapon(GameObject newWeapon, int weaponKey = 1)
    {
        if (weaponKey == 1)
        {
            primaryWeapon = newWeapon;
            primaryWeaponScript = primaryWeapon.GetComponent<Weapon>();
        } else if (weaponKey == 2)
        {
            secondaryWeapon = newWeapon;
            secondaryWeaponScript = secondaryWeapon.GetComponent<Weapon>();
        }
    }

    /// <summary>
    /// Modifies different fields of the current weapon without changing it out entirely.
    /// </summary>
    /// <param name="fireRateMultiplier"></param>
    /// <param name="newProjectile"></param>
    /// <param name="weaponKey"></param>
    public void ModifyWeapon(float fireRateMultiplier, GameObject newProjectile, int weaponKey = 1)
    {
        ModifyWeapon(fireRateMultiplier, weaponKey);
        ModifyWeapon(newProjectile, weaponKey);
    }

    public void ModifyWeapon(float fireRateMultiplier, int weaponKey = 1)
    {
        if (weaponKey == 1)
        {
            primaryWeaponScript.MultiplyFireRate(fireRateMultiplier);
        } else if (weaponKey == 2)
        {
            secondaryWeaponScript.MultiplyFireRate(fireRateMultiplier);
        }
        // After modifying fire rate, tell the GameManager to update the stats UI.
        if (GameManager2D.Instance != null)
        {
            GameManager2D.Instance.UpdatePlayerStatsUI();
        }
    }

    public void ModifyWeapon(GameObject newProjectilePrefab, int weaponKey = 1)
    {
        if (weaponKey == 1)
        {
            primaryWeaponScript.changeProjectile(newProjectilePrefab);
        }
        else if (weaponKey == 2)
        {
            secondaryWeaponScript.changeProjectile(newProjectilePrefab);
        }
    }
    
    public void RevertToDefaultProjectile(int weaponKey = 1)
    {
        if (weaponKey == 1)
        {
            primaryWeaponScript.revertProjectile();
        } else if (weaponKey == 2)
        {
            secondaryWeaponScript.revertProjectile();
        }
    }

    public float GetCurrentFireRate(int weaponKey = 1)
    {
        if (weaponKey == 1)
        {
            return primaryWeaponScript.GetFireRate();
        } else if (weaponKey == 2)
        {
            return secondaryWeaponScript.GetFireRate();
        }
        return 0;
    }
    
    
    
    
    
    
    
    
    
    

    
}
