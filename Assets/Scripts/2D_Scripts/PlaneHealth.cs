using UnityEngine;
using System.Collections; // Required for Coroutines


[System.Serializable]
public class DamageVisual
{
    public float activationPercent;
    public Sprite damageSprite;
    public Color smokeColor;
    public bool smoking;
    public bool onFire;

    public DamageVisual()
    {
        smokeColor = Color.white;
        smoking = true;
        onFire = false;
    }
}

public class PlaneHealth : MonoBehaviour
{
    [Header("Health Settings")]
    // Set the maximum health for this enemy type in the Inspector.
    public int maxHealth = 2;
    private int currentHealth;

    [Header("Damage Visuals")]
    /*// Assign sprites for the damaged states. The array size should be (maxHealth - 1).
    // Order: [First Damaged Sprite, Second Damaged Sprite, ...]
    public Sprite[] damageStateSprites;*/

    // Assign visuals for each damage state based on percentage of health remaining. Visuals must be ordered from full health to least health.
    public DamageVisual[] damageVisuals;
    
    public Color flashColor = Color.red; // The color the sprite will flash when hit
    public float flashDuration = 0.1f;    // How long the flash lasts

    public ParticleSystem smokeTrail;
    public ParticleSystem fireTrail;

    // Private component references
    private PlaneCollisionHandler collisionHandler;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
     private Sprite originalSprite;

    private void Awake()
    {
        // Initialize health when the enemy spawns.
        currentHealth = maxHealth;
    }
    void Start()
    {
        

        // Get references to other components on this same GameObject.
        collisionHandler = GetComponent<PlaneCollisionHandler>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (collisionHandler == null)
        {
            Debug.LogError("EnemyHealth script on " + gameObject.name + " requires a DetectCollisions2D script on the same object.", gameObject);
        }
        if (spriteRenderer == null)
        {
            Debug.LogError("EnemyHealth script on " + gameObject.name + " requires a SpriteRenderer component to show damage.", gameObject);
        }
        else
        {
            // Store the sprite's original color to return to after flashing.
            originalColor = spriteRenderer.color;
            originalSprite = spriteRenderer.sprite;
        }

        

        /*// Check that the number of damage sprites matches the number of damage states.
        if (damageStateSprites.Length != maxHealth - 1)
        {
            Debug.LogWarning("On " + gameObject.name + ", the number of damage sprites (" + damageStateSprites.Length + ") does not match the number of damage states (" + (maxHealth - 1) + "). Visuals may not display as expected.", gameObject);
        }*/
        // No need to call UpdateSprite() here, as the enemy already has its default full-health sprite.
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// Reduces the enemy's health by a specified amount and triggers visual feedback.
    /// </summary>
    /// <param name="damageAmount">The amount of damage to inflict.</param>
    public void TakeDamage(int damageAmount)
    {
        if (GameManager2D.Instance != null && !(GameManager2D.Instance.gameState == GameState.Active))
        {
            return;
        }

        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current health: " + currentHealth + "/" + maxHealth);

        // Update the sprite to reflect the new, damaged health state.
        UpdateSprite();
        
        // Trigger the flash effect.
        StartCoroutine(FlashOnHit());

        if (currentHealth <= 0)
        {
            // If health is depleted, tell the collision handler to start the death sequence.
            if (collisionHandler != null)
            {
                if (smokeTrail != null)
                {
                    Destroy(smokeTrail);
                }
                if (fireTrail != null)
                {
                    Destroy(fireTrail);
                }

                collisionHandler.HandleDefeat();
            }
            else
            {
                Destroy(gameObject); 
            }
        }
    }

    public void AddHealth(int amount)
    {
        // Increase health, but clamp it so it doesn't go over the maximum.
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log("Player gained health! Current health: " + currentHealth);

        // Tell the GameManager to update the health bar/lives display.
        if (GameManager2D.Instance != null)
        {
            GameManager2D.Instance.UpdateLivesDisplay();
        }

        // Update the sprite to reflect the new, damaged health state.
        UpdateSprite();
    }

    /// <summary>
    /// Updates the plane's sprite to the correct damaged state.
    /// </summary>

    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        // 'damageVisuals' array must be sorted from highest % to lowest % in the Inspector

        // If health is full, revert to the original sprite and stop all damage effects.
        if (currentHealth >= maxHealth)
        {
            spriteRenderer.sprite = originalSprite;

            if (smokeTrail != null && smokeTrail.isPlaying)
            {
                smokeTrail.Stop();
            }
            if (fireTrail != null && fireTrail.isPlaying)
            {
                fireTrail.Stop();
            }

            return;
        }

        // If health is zero or less, do nothing.
        if (currentHealth <= 0)
        {
            return;
        }

        if (damageVisuals != null && damageVisuals.Length > 0)
    {
        float healthPercent = (float)currentHealth / maxHealth * 100f;

        // Loop from the highest health tier (100%) to the lowest.
        for (int i = 0; i < damageVisuals.Length; i++)
        {
            // Find the first tier that our current health is greater than or equal to.
            if (healthPercent >= damageVisuals[i].activationPercent)
            {
                DamageVisual newVisual = damageVisuals[i];

                // Apply the sprite and visual effects for this tier.
                if (newVisual.damageSprite != null)
                {
                    spriteRenderer.sprite = newVisual.damageSprite;
                }
              
                if (newVisual.smoking)
                {
                    PlaySmokeEffects(newVisual.smokeColor);
                }
                if (newVisual.onFire)
                {
                    PlayFireEffects();
                }

                // Exit the method since we've found and applied the correct state.
                return;
            }
        }
    }

        

    }

    public void PlaySmokeEffects()
    {
        if (!smokeTrail.isPlaying)
        {
            smokeTrail.Play();
        }
    }

    public void PlaySmokeEffects(Color color)
    {
        ParticleSystem.MainModule enemySmokeTrailMain = smokeTrail.main;
        ParticleSystem.MinMaxGradient newColor = new ParticleSystem.MinMaxGradient(color);
        enemySmokeTrailMain.startColor = newColor;
        PlaySmokeEffects();
    }

    public void PlayFireEffects()
    {
        if (fireTrail != null)
        {
            if (!fireTrail.isPlaying)
            {
                fireTrail.Play();
            }
        }
    }

    /// <summary>
    /// A coroutine that briefly changes the sprite's color to indicate a hit.
    /// </summary>
    private IEnumerator FlashOnHit()
    {
        if (spriteRenderer != null)
        {
            // Change to the flash color
            spriteRenderer.color = flashColor;
            
            // Wait for the specified duration
            yield return new WaitForSeconds(flashDuration);
            
            // Revert to the original color
            spriteRenderer.color = originalColor;
        }
    }
}
