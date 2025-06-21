using UnityEngine;
using System.Collections; // Required for Coroutines

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    // Set the maximum health for this enemy type in the Inspector.
    public int maxHealth = 2;
    private int currentHealth;

    [Header("Damage Visuals")]
    // Assign sprites for the damaged states. The array size should be (maxHealth - 1).
    // Order: [First Damaged Sprite, Second Damaged Sprite, ...]
    public Sprite[] damageStateSprites;
    public Color flashColor = Color.white; // The color the sprite will flash when hit
    public float flashDuration = 0.1f;    // How long the flash lasts

    // Private component references
    private DetectCollisions2D collisionHandler;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        // Initialize health when the enemy spawns.
        currentHealth = maxHealth;

        // Get references to other components on this same GameObject.
        collisionHandler = GetComponent<DetectCollisions2D>();
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

        // Check that the number of damage sprites matches the number of damage states.
        if (damageStateSprites.Length != maxHealth - 1)
        {
            Debug.LogWarning("On " + gameObject.name + ", the number of damage sprites (" + damageStateSprites.Length + ") does not match the number of damage states (" + (maxHealth - 1) + "). Visuals may not display as expected.", gameObject);
        }
        // No need to call UpdateSprite() here, as the enemy already has its default full-health sprite.
    }

    /// <summary>
    /// Reduces the enemy's health by a specified amount and triggers visual feedback.
    /// </summary>
    /// <param name="damageAmount">The amount of damage to inflict.</param>
    public void TakeDamage(int damageAmount)
    {
        if (GameManager2D.Instance != null && !GameManager2D.Instance.isGameActive)
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
                collisionHandler.HandleEnemyDefeat();
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
        
        if (spriteRenderer == null || damageStateSprites == null || damageStateSprites.Length == 0)
        {
            return; // Exit if we don't have the necessary components/assets.
        }

        // This logic maps the remaining health to the correct damaged sprite.
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
