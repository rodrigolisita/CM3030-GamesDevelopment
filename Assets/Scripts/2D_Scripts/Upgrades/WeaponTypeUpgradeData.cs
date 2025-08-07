using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Type Upgrade", menuName = "Skyfire/Upgrades/Weapon Type")]
public class WeaponTypeUpgradeData : UpgradeData
{
    [Tooltip("The new projectile prefab to assign to the player.")]
    public GameObject newProjectilePrefab;

    public override void Apply(GameObject target)
    {
        PlayerController2D player = target.GetComponent<PlayerController2D>();
        if (player != null)
        {
            // Apply the specific upgrade effect.
            player.projectilePrefab = newProjectilePrefab;
        }
    }
}