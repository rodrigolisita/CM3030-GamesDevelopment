using UnityEngine;

// This helper class creates a neat, collapsible section in the Inspector for each upgrade tier.
// It allows you to link a score requirement directly to a specific upgrade asset.
[System.Serializable]
public class UpgradeLevel
{
    [Tooltip("The score the player must reach to unlock this upgrade.")]
    public int scoreThreshold;

    [Tooltip("The UpgradeData asset to award when the score threshold is met.")]
    public UpgradeData upgradeToAward;
    
    // This is hidden from the Inspector and used by the script to track if the upgrade has been given.
    [HideInInspector] public bool awarded = false;
}

public class UpgradeManager : MonoBehaviour
{
    [Header("Upgrade Tiers")]
    [Tooltip("Add all the score-based upgrades for the game here. You can add multiple levels.")]
    public UpgradeLevel[] upgradeLevels;

    [Header("Effects")]
    [Tooltip("The sound effect to play when any upgrade is awarded.")]
    [SerializeField] private AudioClip upgradeSound;

    // When this object is enabled (e.g., at the start of the game),
    // it subscribes to the GameManager's score change announcement.
    private void OnEnable()
    {
        GameManager2D.OnScoreChanged += CheckForUpgrades;
    }

    // When this object is disabled, it's very important to unsubscribe
    // to prevent errors or memory leaks.
    private void OnDisable()
    {
        GameManager2D.OnScoreChanged -= CheckForUpgrades;
    }

    // This method runs automatically whenever the GameManager announces a score change.
    private void CheckForUpgrades(int newScore)
    {
        // Loop through all the upgrade levels you've set up in the Inspector.
        for (int i = 0; i < upgradeLevels.Length; i++)
        {
            // Check if the player's score is high enough for the current level
            // AND if this specific upgrade has not already been awarded.
            if (newScore >= upgradeLevels[i].scoreThreshold && !upgradeLevels[i].awarded)
            {
                // Mark this level as awarded so it won't be given again.
                upgradeLevels[i].awarded = true;

                // Find the player in the scene.
                PlayerController2D player = FindObjectOfType<PlayerController2D>();
                if (player != null)
                {
                    // Get the specific upgrade asset for this level and tell the player to apply it.
                    player.ApplyUpgrade(upgradeLevels[i].upgradeToAward);
                    Debug.Log("UPGRADE AWARDED: " + upgradeLevels[i].upgradeToAward.name + " at " + newScore + " points!");

                    // Play the upgrade sound effect via the GameManager.
                    if (upgradeSound != null)
                    {
                        GameManager2D.Instance.PlaySoundEffect(upgradeSound);
                    }
                }
            }
        }
    }

    public int GetNextUpgradeScore()
    {
        // Find the next upgrade that hasn't been awarded yet
        for (int i = 0; i < upgradeLevels.Length; i++)
        {
            if (!upgradeLevels[i].awarded)
            {
                // Return the score threshold of the next available upgrade
                return upgradeLevels[i].scoreThreshold;
            }
        }
        // Return -1 or another indicator that all upgrades have been awarded
        return -1;
    }

    public void TotalReset()
    {

    }
}