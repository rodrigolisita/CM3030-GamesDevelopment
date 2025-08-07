using UnityEngine;

[CreateAssetMenu(fileName = "New Fire Rate Upgrade", menuName = "Skyfire/Upgrades/Fire Rate")]
public class FireRateUpgradeData : UpgradeData
{
    [Tooltip("How much to multiply the current rounds per minute by (e.g., 1.5 for a 50% increase).")]
    public float fireRateMultiplier = 1.5f;

    public override void Apply(GameObject target)
    {
        PlayerController2D player = target.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.roundsPerMinute *= fireRateMultiplier;
        }
    }

    // --- REVERT METHOD ---
    public override void Revert(GameObject target)
    {
        PlayerController2D player = target.GetComponent<PlayerController2D>();
        if (player != null)
        {
            // To revert, we divide by the same amount we multiplied by.
            player.roundsPerMinute /= fireRateMultiplier;
        }
    }
}