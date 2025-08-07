using UnityEngine;

public abstract class UpgradeData : ScriptableObject
{
    [Header("Timed Upgrade Settings")]
    [Tooltip("How long the upgrade lasts in seconds. Set to 0 for a permanent upgrade.")]
    public float duration = 10f;

    // Defines how the upgrade is applied.
    public abstract void Apply(GameObject target);
    
    // Defines how the upgrade is removed.
    public abstract void Revert(GameObject target);
}