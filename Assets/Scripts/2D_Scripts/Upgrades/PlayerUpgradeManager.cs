using UnityEngine;
using System.Collections.Generic;

// Helper class to track an active upgrade and its timer
public class ActiveUpgrade
{
    public UpgradeData upgrade;
    public float timer;

    public ActiveUpgrade(UpgradeData upgradeData)
    {
        upgrade = upgradeData;
        timer = upgradeData.duration;
    }
}

public class PlayerUpgradeManager : MonoBehaviour
{
    private List<ActiveUpgrade> activeUpgrades = new List<ActiveUpgrade>();
    private List<UpgradeData> permanentUpgrades = new List<UpgradeData>();

    void Update()
    {
        // We loop backwards because we might remove items from the list.
        for (int i = activeUpgrades.Count - 1; i >= 0; i--)
        {
            // Count down the timer for this upgrade.
            activeUpgrades[i].timer -= Time.deltaTime;

            // If the timer runs out, revert the upgrade and remove it from the list.
            if (activeUpgrades[i].timer <= 0)
            {
                activeUpgrades[i].upgrade.Revert(this.gameObject);
                Debug.Log("Reverting upgrade: " + activeUpgrades[i].upgrade.name);
                activeUpgrades.RemoveAt(i);
            }
        }
    }

    // This is the new public method that receives upgrades.
    public void AddUpgrade(UpgradeData upgradeData)
    {
        // Immediately apply the upgrade's effect.
        upgradeData.Apply(this.gameObject);
        Debug.Log("Applied upgrade: " + upgradeData.name);

        // If the upgrade has a duration, add it to our list to be tracked.
        if (upgradeData.duration > 0)
        {
            activeUpgrades.Add(new ActiveUpgrade(upgradeData));
        }
    }

    

    // Used when changing to a new weapon. Applies all the other upgrades to the new weapon.
    public void ApplyAllToNewWeapon()
    {
        for (int i = activeUpgrades.Count - 1; i >= 0; i--)
        {
            UpgradeData upgrade = activeUpgrades[i].upgrade;
            if (upgrade is WeaponSwapUpgradeData) continue;
            upgrade.Apply(this.gameObject);
        }
        for (int i = permanentUpgrades.Count - 1; i >= 0; i--)
        {
            UpgradeData upgrade = permanentUpgrades[i];
            if (upgrade is WeaponSwapUpgradeData) continue;
            upgrade.Apply(this.gameObject);
        }
    }

     public float GetMaxTimeLeft()
    {
        float maxTime = 0;
        foreach (var upgrade in activeUpgrades)
        {
            if (upgrade.timer > maxTime)
            {
                maxTime = upgrade.timer;
            }
        }
        return maxTime;
    }
}