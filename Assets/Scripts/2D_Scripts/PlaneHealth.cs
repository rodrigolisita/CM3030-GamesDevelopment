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

    /// <summary>
    /// Updates the enemy's sprite to the correct damaged state.
    /// </summary>
    private void UpdateSprite()
    {
        // Don't try to change the sprite if health is full (it uses its default sprite)
        // or if it's already defeated.
        if (currentHealth >= maxHealth || currentHealth <= 0)
        {
            return;
        }
        
        if (spriteRenderer == null || damageVisuals == null || damageVisuals.Length == 0)
        {
            return; // Exit if we don't have the necessary components/assets.
        }

        float damagePercent = currentHealth * 100f / maxHealth;
        int newVisualIndex = 0;
        while (newVisualIndex + 1 < damageVisuals.Length)
        {
            if (damageVisuals[newVisualIndex + 1].activationPercent >= damagePercent)
            {
                newVisualIndex += 1;
            } else
            {
                break;
            }
        }
        DamageVisual newVisual = damageVisuals[newVisualIndex];

        if (newVisual.damageSprite != null)
        {
            spriteRenderer.sprite = newVisual.damageSprite;
        }
        
        if (newVisual.smoking && newVisual.smokeColor != null)
        {
            PlaySmokeEffects(newVisual.smokeColor);
        }

        if (newVisual.onFire)
        {
            PlayFireEffects();
        }



        /*// This logic maps the remaining health to the correct damaged sprite.
        // The sprites are ordered from lightest damage to heaviest damage.
        // Example (maxHealth = 3, array size = 2):
        // - currentHealth = 2 -> index = 3 - 2 - 1 = 0 (first damaged sprite)
        // - currentHealth = 1 -> index = 3 - 1 - 1 = 1 (second damaged sprite)
        int spriteIndex = maxHealth - currentHealth - 1;

        // Clamp the index to be safe and apply the new sprite.
        if (spriteIndex >= 0 && spriteIndex < damageStateSprites.Length)
        {
            if (damageStateSprites[spriteIndex] != null)
            {
                spriteRenderer.sprite = damageStateSprites[spriteIndex];
            }
        }*/
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
