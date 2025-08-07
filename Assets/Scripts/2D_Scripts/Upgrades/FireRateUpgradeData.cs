using UnityEngine;

[CreateAssetMenu(fileName = "New Fire Rate Upgrade", menuName = "Skyfire/Upgrades/Fire Rate")]
public class FireRateUpgradeData : UpgradeData
{
    [Tooltip("How much to multiply the current rounds per minute by (e.g., 1.5 for a 50% increase).")]
    public float fireRateMultiplier = 1.5f;

    // This is the implementation of the abstract Apply method from the base class.
    public override void Apply(GameObject target)
    {
        // Try to get the PlayerController2D component from the target.
        PlayerController2D player = target.GetComponent<PlayerController2D>();
        if (player != null)
        {
            // Apply the specific upgrade effect.
            player.roundsPerMinute *= fireRateMultiplier;
        }
    }
}