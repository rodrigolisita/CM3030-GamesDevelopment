using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bomb : Projectile
{
    public float blastRadius = 10;
    public AudioClip explosionSound;
    public GameObject explosionSprite;

    private AudioSource audioSource;

    public new void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        base.Awake();
        Debug.Log("Bomb Awake");
    }

    public override void TriggerProjectileEffect(Collider2D other)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, blastRadius);

        if (explosionSprite != null)
        {
            GameObject explosionInstance = Instantiate(explosionSprite, transform.position, transform.rotation);
            //explosionInstance.transform.SetParent(transform);
            explosionInstance.transform.localScale = explosionInstance.transform.localScale * blastRadius * .1f;
        }

        // --- NEW: Play the hit sound ---
        // Check if the audioSource and the sound clip are assigned to prevent errors.
        if (audioSource != null && explosionSound != null)
        {
            // Play the sound effect once.
            audioSource.PlayOneShot(explosionSound);
        }

        foreach (Collider2D collider in colliders)
        {
            if(ShouldTrigger(collider))
            {
                DealDamageTo(collider);
            }
        }

        DisableForExplosion();

        // Calculate destruction delay based on sound or particle duration
        float soundLength = (explosionSound != null) ? explosionSound.length : 0.5f;

        // Destroy this enemy GameObject after the delay
        Destroy(gameObject, soundLength);
    }
}
