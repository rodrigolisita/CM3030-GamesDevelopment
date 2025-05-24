using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager2D : MonoBehaviour
{
    public static GameManager2D Instance { get; private set; }

    // --- UI Element References (will be re-found on scene load) ---
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    // --- Names of UI GameObjects in the Scene (ensure these match your scene) ---
    public string scoreTextName = "ScoreText"; 
    public string livesTextName = "LivesText";
    public string gameOverTextName = "GameOverText";
    public string restartButtonName = "RestartButton";

    // --- Game State Variables ---
    private int score;
    public int initialLives = 3;
    private int currentLives;
    public bool isGameActive;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject.transform.root.gameObject); // Persist the root GameObject
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene loaded event
            ResetInternalGameState(); // Initialize for the very first game session
        }
        else if (Instance != this)
        {
            // If another instance exists, destroy this duplicate.
            Destroy(gameObject);
            return; 
        }
        // If Instance == this, it's the persistent GameManager2D.
        // OnSceneLoaded will handle UI re-linking and updates.
    }

    void OnDestroy()
    {
        // Unsubscribe from the event when the GameManager2D is truly destroyed
        // to prevent memory leaks, though with DontDestroyOnLoad, this is less common.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called every time a scene finishes loading
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("GameManager2D OnSceneLoaded: Scene '" + scene.name + "' loaded. Re-finding UI elements and updating display.");
        FindUIElements(); // First, get references to the new UI elements
        UpdateAllUIDisplays(); // Then, update them based on current game state
    }

    // Finds UI elements in the current scene.
    void FindUIElements()
    {
        Debug.Log("GameManager2D FindUIElements: Attempting to find UI elements by name.");
        GameObject scoreTextGO = GameObject.Find(scoreTextName);
        if (scoreTextGO != null) scoreText = scoreTextGO.GetComponent<TextMeshProUGUI>();
        else Debug.LogError("GameManager2D FindUIElements: Could not find ScoreText GameObject named: '" + scoreTextName + "'. Ensure it exists in the scene and the name matches.", this.gameObject);

        GameObject livesTextGO = GameObject.Find(livesTextName);
        if (livesTextGO != null) livesText = livesTextGO.GetComponent<TextMeshProUGUI>();
        else Debug.LogError("GameManager2D FindUIElements: Could not find LivesText GameObject named: '" + livesTextName + "'.", this.gameObject);
        
        GameObject gameOverTextGO = GameObject.Find(gameOverTextName);
        if (gameOverTextGO != null) gameOverText = gameOverTextGO.GetComponent<TextMeshProUGUI>(); // Changed from TextMeshProUGUI to GameObject
        else Debug.LogError("GameManager2D FindUIElements: Could not find GameOverText GameObject named: '" + gameOverTextName + "'.", this.gameObject);

        GameObject restartButtonGO = GameObject.Find(restartButtonName);
        if (restartButtonGO != null) restartButton = restartButtonGO.GetComponent<Button>();
        else Debug.LogError("GameManager2D FindUIElements: Could not find RestartButton GameObject named: '" + restartButtonName + "'.", this.gameObject);

        // Ensure the restart button's listener is set up.
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners(); // Clear previous listeners to be safe
            restartButton.onClick.AddListener(RestartGame);
            Debug.Log("GameManager2D FindUIElements: RestartButton listener re-assigned.");
        }
    }

    // Resets the internal game state variables.
    void ResetInternalGameState()
    {
        isGameActive = true; // Explicitly set to true for a new game
        score = 0;
        currentLives = initialLives;
        Debug.Log("GameManager2D ResetInternalGameState: Game state reset. isGameActive: " + isGameActive + ", Score: " + score + ", Lives: " + currentLives);
        // DO NOT try to SetActive(false) on UI elements here, as their references might be stale
        // if this is called before a scene reload. UI visibility is handled in UpdateAllUIDisplays.
    }

    // Updates all relevant UI elements based on the current game state.
    void UpdateAllUIDisplays()
    {
        Debug.Log("GameManager2D UpdateAllUIDisplays: Updating UI. Current isGameActive: " + isGameActive);

        // Update score display
        if (scoreText != null) 
        {
            scoreText.text = "Score: " + score; 
            Debug.Log("GameManager2D UpdateAllUIDisplays: Score UI updated to: " + scoreText.text);
        }
        else Debug.LogError("ScoreText reference is null in UpdateAllUIDisplays. Cannot update score display.", this.gameObject);

        // Update lives display
        if (livesText != null) 
        {
            UpdateLivesDisplay(); // This method already contains a null check for livesText
            Debug.Log("GameManager2D UpdateAllUIDisplays: Lives UI updated via UpdateLivesDisplay(). Current lives on UI: " + (livesText.text));
        }
        else Debug.LogError("LivesText reference is null in UpdateAllUIDisplays. Cannot initially call UpdateLivesDisplay().", this.gameObject);
        
        // Determine if Game Over UI should be active
        bool showGameOverUI = !isGameActive; // If game is active, don't show game over UI

        if (gameOverText != null) 
        {
            Debug.Log("GameManager2D UpdateAllUIDisplays: Setting GameOverText active state to: " + showGameOverUI + " (because isGameActive is " + isGameActive + ")");
            gameOverText.gameObject.SetActive(showGameOverUI);
        }
        else Debug.LogError("GameOverText reference is null in UpdateAllUIDisplays. Cannot set active state.", this.gameObject);

        if (restartButton != null) 
        {
            Debug.Log("GameManager2D UpdateAllUIDisplays: Setting RestartButton active state to: " + showGameOverUI + " (because isGameActive is " + isGameActive + ")");
            restartButton.gameObject.SetActive(showGameOverUI);
        }
        else Debug.LogError("RestartButton reference is null in UpdateAllUIDisplays. Cannot set active state.", this.gameObject);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // For a persistent GameManager2D, this will only run when it's first created.
    void Start()
    {
        Debug.Log("GameManager2D Start() called (should only be once for persistent instance).");
    }

    // Update is called once per frame
    void Update()
    {

    }

    // --- Score functions ---
    public void UpdateScore(int scoreToAdd)
    {
        if (!isGameActive && scoreToAdd > 0) 
        {
            // Debug.Log("Game is not active. Score not updated.");
            // return;
        }
        score += scoreToAdd;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
        // No need for an else here if FindUIElements and UpdateAllUIDisplays handle initial setup
    }

    // --- Lives functions ---
    public void LoseLife()
    {
        if (!isGameActive) return; 

        if (currentLives > 0)
        {
            currentLives--;
            Debug.Log("Player lost a life. Lives remaining: " + currentLives);
            UpdateLivesDisplay(); 

            if (currentLives <= 0)
            {
                GameOver();
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
        // No need for an else here if FindUIElements and UpdateAllUIDisplays handle initial setup
    }     

    // --- Game Over Functions ---
    public void GameOver()
    {
        if (!isGameActive) return; // Prevent calling GameOver multiple times

        Debug.Log("GameManager2D: GameOver() called.");
        isGameActive = false; // Set game to inactive first

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("GameOverText is not assigned/found! Cannot show Game Over text.", this.gameObject);
        }
        
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("RestartButton is not assigned/found! Cannot show Restart button.", this.gameObject);
        }
    }

    public void RestartGame()
    {
        Debug.Log("GameManager2D: RestartGame() called. Resetting internal state and reloading scene.");
        ResetInternalGameState(); // This will set isGameActive = true and reset score/lives
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // After scene load, OnSceneLoaded will fire, which calls FindUIElements and then UpdateAllUIDisplays.
        // UpdateAllUIDisplays will then use the fresh isGameActive = true to hide the game over UI.
    }
}
