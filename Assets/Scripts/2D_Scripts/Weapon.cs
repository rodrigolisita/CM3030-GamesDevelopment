using UnityEngine;


public class Weapon : MonoBehaviour
{
    [Header("Combat")]
    [Tooltip("The projectile prefab that the weapon will fire.")]
    public GameObject projectilePrefab;
    private GameObject originalProjectilePrefab;
    [Tooltip("The number of shots the weapon can fire per minute.")]
    public float roundsPerMinute = 800;
    public bool ammoLimited = false;
    public int maxAmmo = 3;
    public Transform[] firePoints;
    public bool sequential = false;
    public float firingArc = 0;




    [Header("Effects & Audio")]
    [Tooltip("The sound effect that plays when the weapon fires.")]
    public AudioClip[] shootingSounds; // Public variable to assign your shooting sound effect
    

    private AudioSource weaponAudioSource;
    private bool firing = false;
    private float burstStartTime = 0;
    private int shotsFiredThisBurst = 0;
    private float lastShotTime = 0;
    private int curAmmo;
    private int nextFirePointIndex = 0;
    private int nextShootingSoundIndex = 0;

    private void Awake()
    {
        curAmmo = maxAmmo;
    }

    private void Start()
    {
        // Get the AudioSource component attached to this GameObject
        weaponAudioSource = GetComponent<AudioSource>();

        // Check if the AudioSource component was found and configure it
        if (weaponAudioSource != null)
        {
            // Ensure the sound doesn't play automatically when the game starts
            weaponAudioSource.playOnAwake = false;
            // Ensure the AudioSource itself isn't set to loop, PlayOneShot handles its own playback.
            weaponAudioSource.loop = false;
        }
        else
        {
            Debug.LogError("Player AudioSource component not found on " + gameObject.name + ". Please add one.", gameObject);
        }

        // Check if a shooting sound has been assigned in the Inspector
        // This warning is still useful to ensure a clip is available for shooting.
        if (shootingSounds == null && (weaponAudioSource != null && weaponAudioSource.clip == null)) // Only warn if no clip is set anywhere for shooting
        {
            Debug.LogWarning("Shooting sound not assigned in the Inspector for " + gameObject.name +
                             ", and no default clip on AudioSource. Please ensure a sound is set if you want shooting sounds.", gameObject);
        }

        originalProjectilePrefab = projectilePrefab;

        if (firePoints.Length <= 0)
        {
            Debug.LogError("no fire points on " + gameObject.name, gameObject);
        }
    }
     
    /// <summary>
    /// Returns true if the weapon fired.
    /// </summary>
    /// <returns></returns>
    public bool PullTrigger()
    {
        if (ammoLimited && curAmmo <= 0)
        {
            return false;
        }

        float curTime = Time.time;
        //Check if the weapon can fire another shot yet
        //Debug.Log("Time.time = " + Time.time + "\nlastShotTime = " + lastShotTime + "\n burstStartTime = " + burstStartTime + "\n shotsFiredThisBurst = " + shotsFiredThisBurst);
        if (((!firing) && ((curTime - lastShotTime) > (60 / roundsPerMinute))) || firing && (shotsFiredThisBurst < (roundsPerMinute / 60) * (curTime - burstStartTime)))
        {
            //update the info on the shot
            lastShotTime = curTime;

            curAmmo -= 1;

            if (firing)
            {
                shotsFiredThisBurst += 1;
            }
            else
            {
                firing = true;
                burstStartTime = curTime;
                shotsFiredThisBurst = 1;
            }

            // Launch a projectile from the weapon (can be overridden by weapons that fire multiple projectiles or feature other behavior)
            FireProjectile();

            PlayShootingSound();

            return true;
        }

        return false;
    }

    public void PlayShootingSound()
    {


        // Play the next shooting sound
        if (weaponAudioSource != null && shootingSounds.Length >= 0)
        {
            if (nextShootingSoundIndex >= shootingSounds.Length)
            {
                nextShootingSoundIndex = 0;
            }
            weaponAudioSource.PlayOneShot(shootingSounds[nextShootingSoundIndex]);
            nextShootingSoundIndex += 1;
        }
        // Fallback to play clip directly on AudioSource if shootingSound (script variable) isn't set
        else if (weaponAudioSource != null && weaponAudioSource.clip != null)
        {
            Debug.LogWarning("Playing default clip from Player's AudioSource as 'shootingSound' was not set in script for " + gameObject.name + ". Consider setting the public 'shootingSound' variable.", gameObject);
            weaponAudioSource.PlayOneShot(weaponAudioSource.clip);
        }
        else if (weaponAudioSource != null) // No clip assigned anywhere
        {
            Debug.LogWarning("Player AudioSource found on " + gameObject.name + " but no AudioClip s are assigned to the script's 'shootingSounds' field or the AudioSource's 'AudioClip' field. No sound will play.", gameObject);
        }
    }


    /// <summary>
    /// By changing firing to false, we exit the current burst and start a new one the next time we're eligible.
    /// </summary>
    public void CeaseFire()
    {
        firing = false;
    }

    public bool isFiring()
    {
        return firing;
    }

    public void MultiplyFireRate(float multiplier)
    {
        roundsPerMinute = roundsPerMinute * multiplier;
        CeaseFire(); // If we don't reset the burst, it will massively slow down or speed up fire rate for a short period to try to "catch up" to where it should have been if the whole burst had the new fire rate.
    }

    public void changeProjectile(GameObject newProjectilePrefab)
    {
        projectilePrefab = newProjectilePrefab;
    }

    public void revertProjectile()
    {
        // Set the current projectile back to the one we saved at the start.
        projectilePrefab = originalProjectilePrefab;
        Debug.Log("Weapon reverted to default.");
    }


    /// <summary>
    /// This can be overridden by child classes that fire projectiles in different ways.
    /// </summary>
    public virtual void FireProjectile()
    {
        if (sequential)
        {
            if (nextFirePointIndex >= firePoints.Length)
            {
                nextFirePointIndex = 0;
            }
            FireProjectile(firePoints[nextFirePointIndex]);
            nextFirePointIndex += 1;
        }
        else
        {
            foreach (Transform fireTransform in firePoints)
            {
                FireProjectile(fireTransform);
            }
        }
        
        
    }

    public virtual void FireProjectile(Transform fireTransform)
    {
        Instantiate(projectilePrefab, fireTransform.position, fireTransform.rotation);
    }

    public float GetFireRate()
    {
        return roundsPerMinute;
    }

    /// <summary>
    /// Returns the max ammo count, unless it's unlimited (then -1)
    /// </summary>
    /// <returns></returns>
    public int GetMaxAmmo()
    {
        if (!ammoLimited)
        {
            return -1;
        }
        return maxAmmo;
    }
    
    /// <summary>
    /// Returns the amount of ammo remaining, unless it's unlimited (then -1)
    /// </summary>
    /// <returns></returns>
    public int GetCurAmmo()
    {
        if (!ammoLimited)
        {
            return -1;
        }
        return curAmmo;
    }

    public void AddAmmo(int amount)
    {
        curAmmo = Mathf.Clamp(curAmmo + amount, 0, maxAmmo);
    }
}
