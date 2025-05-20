using UnityEngine;
using TMPro; // If you have a UI element for lives

public class PlayerStatsManager : MonoBehaviour
{

    public TextMeshProUGUI livesText;

    public static PlayerStatsManager Instance { get; private set; }

    public int initialLives = 3;
    private int currentLives;

    // public TextMeshProUGUI livesText; // Optional: If you display lives

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject.transform.root.gameObject); // If GameManager persists
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentLives = initialLives;
        UpdateLivesDisplay(); // If you have a UI
    }

    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
            Debug.Log("Player lost a life. Lives remaining: " + currentLives);
            UpdateLivesDisplay(); // If you have a UI

            if (currentLives <= 0)
            {
                // Handle Game Over (e.g., tell a GameManager to end the game)
                Debug.Log("Game Over!");
                // GameManager.Instance.GameOver(); // Example
            }
        }
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }

    void UpdateLivesDisplay()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives;
        }
    }
}