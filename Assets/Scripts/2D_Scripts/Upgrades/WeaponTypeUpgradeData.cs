using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Type Upgrade", menuName = "Skyfire/Upgrades/Weapon Type")]
public class WeaponTypeUpgradeData : UpgradeData
{
    [Tooltip("The new projectile prefab to assign to the player.")]
    public GameObject newProjectilePrefab;

    // The Apply method changes the weapon to use the new projectile.
    public override void Apply(GameObject target)
    {
        PlayerController2D player = target.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.ModifyWeapon(newProjectilePrefab);
        }
    }

    // The Revert method tells the player to switch back to its default.
    public override void Revert(GameObject target)
    {
        PlayerController2D player = target.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.RevertToDefaultProjectile();
        }
    }
}