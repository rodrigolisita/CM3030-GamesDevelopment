using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LifeBonusHandler : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The amount of health to restore to the player.")]
    [SerializeField] private int healthToRestore = 1;

    [Header("Effects")]
    [Tooltip("Sound effect to play when the player collects this bonus.")]
    [SerializeField] private AudioClip collectionSound;
    
    // This function runs automatically when another collider enters this object's trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object we collided with has the "Player" tag.
        if (other.CompareTag("Player"))
        {
            // Try to find the PlaneHealth component on the player object.
            PlaneHealth playerHealth = other.GetComponent<PlaneHealth>();
            if (playerHealth != null)
            {
                // Tell the player's health script to add health.
                playerHealth.AddHealth(healthToRestore);
            }

            // Ask the GameManager to play the collection sound for us.
            if (collectionSound != null)
            {
                GameManager2D.Instance.PlaySoundEffect(collectionSound);
            }           
            
            

            // Destroy the life bonus object itself.
            Destroy(gameObject);
        }
    }
}