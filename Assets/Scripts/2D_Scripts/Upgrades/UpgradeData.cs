using UnityEngine;

// This is our base template for all upgrades.
public abstract class UpgradeData : ScriptableObject
{
    // This abstract method MUST be implemented by any script that inherits from this one.
    // It takes the GameObject of the player as a target to apply the upgrade to.
    public abstract void Apply(GameObject target);
}