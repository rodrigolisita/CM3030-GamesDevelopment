using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Swap Upgrade", menuName = "Skyfire/Upgrades/Weapon Swap")]
public class WeaponSwapUpgradeData : UpgradeData
{
    [Tooltip("The new weapon to assign to the player.")]
    public GameObject newWeaponPrefab;

    // The Apply method changes the weapon to use the new projectile.
    public override void Apply(GameObject target)
    {
        PlayerController2D player = target.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.SwapWeapon(newWeaponPrefab);
        }
    }

    // The Revert method tells the player to switch back to its default.
    public override void Revert(GameObject target)
    {
        PlayerController2D player = target.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.RevertToDefaultWeapon();
        }
    }
}