using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public enum GameState
{
    PreGame,
    Active,
    GameOver
}

[RequireComponent(typeof(AudioSource))]
public class GameManager2D : MonoBehaviour
{
    public static GameManager2D Instance { get; private set; }

    // --- UI Element References ---
    private GameObject titleScreen; 
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI livesText;
    private TextMeshProUGUI gameOverText;
    private TextMeshProUGUI playInstructionText;
    private TextMeshProUGUI nextUpgradeText; 
    private TextMeshProUGUI upgradeTimerText;
    private TextMeshProUGUI playerInformationText;
    private Button restartButton;
    private Button easyButton; 
    private Button mediumButton; // Added reference for Medium Button

    // --- Names of UI GameObjects in the Scene ---
    // IMPORTANT: These GameObjects MUST BE ACTIVE in your scene by default for FindUIElements to locate them.
    [Header("UI Object Names")]
    public string titleScreenName = "TitleScreen"; 
    public string scoreTextName = "ScoreText"; 
    public string livesTextName = "LivesText";
    public string gameOverTextName = "GameOverText";
    public string playInstructionTextName = "PlayInstruction";
    public string restartButtonName = "RestartButton";
    public string easyButtonName = "EasyButton"; 
    public string mediumButtonName = "MediumButton"; 
    public string nextUpgradeTextName = "NextUpgradeText"; 
    public string upgradeTimerTextName = "UpgradeTimerText"; 
    public string playerInformationTextName = "playerInformationText"; 

    // --- Game State & other private variables ---
    private int score;
    // currentLives is deprecated, do not use
    private int currentLives = 0;
    private GameObject playerPlane;
    public GameObject playerPlanePrefab;
    public GameObject playerSpawnPoint;

    private UpgradeManager upgradeManager;
    private PlayerUpgradeManager playerUpgradeManager;
    private PlayerController2D playerController;

    [Header("Gameplay Settings")]
    public GameState gameState = GameState.PreGame; // Game starts as PreGame


    // --- Audio Management ---
    private AudioSource audioSource;
    [Header("Audio Clips")]
    public AudioClip startScreenMusic;
    public AudioClip activeGameMusic;
    public AudioClip gameOverMusic;

    // -- Event to announce score changes to any listening scripts.
    public static Action<int> OnScoreChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject.transform.root.gameObject);

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("GameManager2D is missing an AudioSource component!", this.gameObject);
            }
            else
            {
                audioSource.loop = true;
            }
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Set initial internal game state for the very first launch
            gameState = GameState.PreGame;
            //currentLives = initialLives; 
            Debug.Log("GameManager2D Awake(): Initial internal state set. gameSate: " + gameState + ", currentLives: " + currentLives);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Start() is called once for the persistent object.
        // UI setup is now fully handled by OnSceneLoaded, which will be called for the initial scene load.
        upgradeManager = FindObjectOfType<UpgradeManager>();
        Debug.Log("GameManager2D Start() called. Initial UI setup is handled by OnSceneLoaded.");

        PlayerController2D playerController = playerPlane.GetComponent<PlayerController2D>();

        if (GameManager2D.Instance != null)
        {
            GameManager2D.Instance.UpdatePlayerStatsUI();
        }

    }

    void Update()
    {
        // We only need to update the timer if the game is active
        if (gameState == GameState.Active)
        {
            UpdateUpgradeTimerUI();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void SpawnNewPlayer()
    {
        if (playerPlane != null)
        {
            Destroy(playerPlane);
        }
        Vector3 playerPosition = new Vector3(playerSpawnPoint.transform.position.x, playerSpawnPoint.transform.position.y, playerSpawnPoint.transform.position.z);
        Quaternion playerRotation = new Quaternion(playerSpawnPoint.transform.rotation.x, playerSpawnPoint.transform.rotation.y, playerSpawnPoint.transform.rotation.z, playerSpawnPoint.transform.rotation.w);
        playerPlane = Instantiate(playerPlanePrefab, playerPosition, playerRotation);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // This runs every time a scene finishes loading, INCLUDING THE FIRST SCENE.
        Debug.Log("GameManager2D OnSceneLoaded: Scene '" + scene.name + "' loaded. Current gameState (from persistent instance): " + gameState);


        // Re-find references to scene objects here, as old ones were destroyed.
        upgradeManager = FindObjectOfType<UpgradeManager>();
        playerUpgradeManager = null; // We'll find this when the player is active
        playerController = null;

        SpawnNewPlayer();
        FindUIElements();       // Re-find UI elements in the newly loaded scene
        UpdateAllUIDisplays();  // Update their visibility based on current game state

        

        if (gameState == GameState.Active)
        {
            // If the scene loads and the game is already active (e.g., moving to level 2), play game music.
            PlayActiveMusic();
        }
        else { 
             // If you have menu music and game is not active 
            PlayStartScreenMusic();
        }
    }

    void FindUIElements()
    {
        Debug.Log("GameManager2D FindUIElements: Attempting to find UI elements by name. Ensure they are ACTIVE in the scene by default to be found.");

        GameObject titleScreenGO_temp = GameObject.Find(titleScreenName); 
        if (titleScreenGO_temp != null) 
        {
            titleScreen = titleScreenGO_temp;
            Debug.Log("GameManager2D: Found TitleScreen GameObject named: '" + titleScreenName + "'.");
            
            Transform easyButtonTransform = titleScreen.transform.Find(easyButtonName);
            if (easyButtonTransform != null)
            {
                easyButton = easyButtonTransform.GetComponent<Button>();
                if (easyButton == null) Debug.LogError("GameManager2D: Found EasyButton GameObject as child of TitleScreen but it's missing a Button component.", easyButtonTransform.gameObject);
                else Debug.Log("GameManager2D: Found EasyButton as child of TitleScreen.");
            }
            else 
            {
                GameObject easyButtonGO_global = GameObject.Find(easyButtonName);
                if (easyButtonGO_global != null) easyButton = easyButtonGO_global.GetComponent<Button>();
                else Debug.LogError("GameManager2D: Could not find Start/EasyButton GameObject named: '" + easyButtonName + "' (neither as child of TitleScreen nor globally). Ensure it exists and is ACTIVE in the scene by default.", this.gameObject);
            }

            Transform mediumButtonTransform = titleScreen.transform.Find(mediumButtonName);
            if (mediumButtonTransform != null)
            {
                mediumButton = mediumButtonTransform.GetComponent<Button>();
                if (mediumButton == null) Debug.LogError("GameManager2D: Found MediumButton GameObject as child of TitleScreen but it's missing a Button component.", mediumButtonTransform.gameObject);
                else Debug.Log("GameManager2D: Found MediumButton as child of TitleScreen.");
            }
            else
            {
                GameObject mediumButtonGO_global = GameObject.Find(mediumButtonName);
                if (mediumButtonGO_global != null) mediumButton = mediumButtonGO_global.GetComponent<Button>();
                else Debug.LogWarning("GameManager2D: Could not find MediumButton GameObject named: '" + mediumButtonName + "' (neither as child of TitleScreen nor globally). Ensure it exists and is ACTIVE in the scene by default.", this.gameObject);
            }
        }
        else 
        {
            Debug.LogError("GameManager2D: Could not find TitleScreen GameObject named: '" + titleScreenName + "'. UI will not display correctly. Ensure it exists and is ACTIVE in the scene by default.", this.gameObject);
            titleScreen = null; 
            
            GameObject easyButtonGO_global_fallback = GameObject.Find(easyButtonName);
            if (easyButtonGO_global_fallback != null) easyButton = easyButtonGO_global_fallback.GetComponent<Button>();
            else Debug.LogError("GameManager2D: Could not find Start/EasyButton GameObject named: '" + easyButtonName + "' (TitleScreen also not found).", this.gameObject);
            
            GameObject mediumButtonGO_global_fallback = GameObject.Find(mediumButtonName);
            if (mediumButtonGO_global_fallback != null) mediumButton = mediumButtonGO_global_fallback.GetComponent<Button>();
            else Debug.LogWarning("GameManager2D: Could not find MediumButton GameObject named: '" + mediumButtonName + "' (TitleScreen also not found).", this.gameObject);
        }
        
        GameObject scoreTextGO = GameObject.Find(scoreTextName);
        if (scoreTextGO != null) scoreText = scoreTextGO.GetComponent<TextMeshProUGUI>();
        else Debug.LogError("GameManager2D: Could not find ScoreText GameObject named: '" + scoreTextName + "'. Ensure it is ACTIVE in the scene by default.", this.gameObject);

        GameObject livesTextGO = GameObject.Find(livesTextName);
        if (livesTextGO != null) livesText = livesTextGO.GetComponent<TextMeshProUGUI>();
        else Debug.LogError("GameManager2D: Could not find LivesText GameObject named: '" + livesTextName + "'. Ensure it is ACTIVE in the scene by default.", this.gameObject);
        
        GameObject gameOverTextGO = GameObject.Find(gameOverTextName);
        if (gameOverTextGO != null) gameOverText = gameOverTextGO.GetComponent<TextMeshProUGUI>();
        else Debug.LogError("GameManager2D: Could not find GameOverText GameObject named: '" + gameOverTextName + "'. Ensure it is ACTIVE in the scene by default.", this.gameObject);

        GameObject playInstructionTextGO = GameObject.Find(playInstructionTextName);
        if (playInstructionTextGO != null) playInstructionText = playInstructionTextGO.GetComponent<TextMeshProUGUI>();
        else Debug.LogWarning("GameManager2D: Could not find PlayInstruction Text GameObject named: '" + playInstructionTextName + "' (neither as child of TitleScreen nor globally). Ensure it exists and is ACTIVE in the scene by default.", this.gameObject);

        GameObject restartButtonGO = GameObject.Find(restartButtonName);
        if (restartButtonGO != null) restartButton = restartButtonGO.GetComponent<Button>();
        else Debug.LogError("GameManager2D: Could not find RestartButton GameObject named: '" + restartButtonName + "'. Ensure it is ACTIVE in the scene by default.", this.gameObject);

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners(); 
            restartButton.onClick.AddListener(RestartGame);
        }

        GameObject nextUpgradeGO = GameObject.Find(nextUpgradeTextName);
        if (nextUpgradeGO != null) nextUpgradeText = nextUpgradeGO.GetComponent<TextMeshProUGUI>();
        else Debug.LogWarning("GameManager2D: Could not find NextUpgradeText GameObject named: " + nextUpgradeTextName);

        GameObject upgradeTimerGO = GameObject.Find(upgradeTimerTextName);
        if (upgradeTimerGO != null) upgradeTimerText = upgradeTimerGO.GetComponent<TextMeshProUGUI>();
        else Debug.LogWarning("GameManager2D: Could not find UpgradeTimerText GameObject named: " + upgradeTimerTextName);

        GameObject playerInformationGO = GameObject.Find(playerInformationTextName);
        if (playerInformationGO != null) playerInformationText = playerInformationGO.GetComponent<TextMeshProUGUI>();
        else Debug.LogWarning("GameManager2D: Could not find playerInformationText GameObject named: " + playerInformationTextName);

        
  
    }

    void ResetInternalGameState()
    {
        score = 0;
        Debug.Log("GameManager2D ResetInternalGameState: Score: " + score + ". (gameState is currently: " + gameState + ")");
        SpawnNewPlayer();
    }

    void UpdateAllUIDisplays()
    {
        Debug.Log("GameManager2D UpdateAllUIDisplays: gameState = " + gameState);

        // Determine which UI sets should be active
        bool showTitleScreenUI = (gameState == GameState.PreGame);
        bool showGameHUD = (gameState == GameState.Active);
        bool showGameOverUI = (gameState == GameState.GameOver);

        // --- Title Screen Elements ---
        if (titleScreen != null) titleScreen.SetActive(showTitleScreenUI);
        if (easyButton != null) easyButton.gameObject.SetActive(showTitleScreenUI);
        if (mediumButton != null) mediumButton.gameObject.SetActive(showTitleScreenUI);
        if (playInstructionText != null) playInstructionText.gameObject.SetActive(showTitleScreenUI);

        // --- Game Over Screen Elements ---
        if (gameOverText != null) gameOverText.gameObject.SetActive(showGameOverUI);
        if (restartButton != null) restartButton.gameObject.SetActive(showGameOverUI);

        // --- In-Game HUD Elements ---
        if (scoreText != null) 
        {
            scoreText.gameObject.SetActive(showGameHUD);
            if(showGameHUD) scoreText.text = "Score: " + score; 
        }
        if (livesText != null) 
        {
            livesText.gameObject.SetActive(showGameHUD);
            if(showGameHUD) UpdateLivesDisplay(); 
        }

        if (nextUpgradeText != null)
        {
            nextUpgradeText.gameObject.SetActive(showGameHUD);
        }
        if (upgradeTimerText != null)
        {
            // The timer text's content is updated every frame in Update(),
            // but its visibility should be tied to the Active game state.
            // If the game is not active, ensure it's hidden.
            if (!showGameHUD)
            {
                upgradeTimerText.gameObject.SetActive(false);
            }
        }

        if (playerInformationText != null) playerInformationText.gameObject.SetActive(showGameHUD);
       
        
    }    
                               
                                                                        
                                                                        
                
    // --- Game Lifecycle Methods ---
    //public void StartGame(float difficultySpawnInterval) 
    public void StartGame(int difficultyLevel) 
    {
        Debug.Log("GameManager2D: StartGame() called with spawnInterval: " + difficultyLevel);
        gameState = GameState.Active;
        ResetInternalGameState(); 
        UpdateAllUIDisplays();    
        PlayActiveMusic();
        UpdateNextUpgradeUI();
        UpdatePlayerStatsUI();

        SpawnManager2D spawnManager = FindObjectOfType<SpawnManager2D>(); 
        if (spawnManager != null)
        {
            spawnManager.BeginSpawningEnemies(difficultyLevel);
        }
        else
        {
            Debug.LogError("GameManager2D: SpawnManager2D instance not found in StartGame(). Cannot start enemy spawning.", this.gameObject);
        }
    }
    
    public void GameOver()
    {
        Debug.Log("GameManager2D: GameOver() called.");
        gameState = GameState.GameOver; 
        PlayGameOverMusic(); 
        UpdateAllUIDisplays(); 

        SpawnManager2D spawnManager = FindObjectOfType<SpawnManager2D>();
        if (spawnManager != null)
        {
            spawnManager.StopSpawningEnemies(); 
        }
    }

    public void RestartGame()
    {
        Debug.Log("GameManager2D: RestartGame() called. Setting isGameActive to false for title screen return.");
        gameState = GameState.PreGame;
        ResetInternalGameState(); 
        
        SpawnManager2D spawnManager = FindObjectOfType<SpawnManager2D>();
        if (spawnManager != null)
        {
            spawnManager.StopSpawningEnemies();
        }
        Debug.Log("GameManager2D: State before loading scene in RestartGame - gameState: " + gameState);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- Audio Control Methods ---
    void PlayStartScreenMusic()
    {
    if (audioSource != null && startScreenMusic != null)
    {
        // Don't restart the music if this track is already playing
        if (audioSource.clip == startScreenMusic && audioSource.isPlaying) return; 
        
        audioSource.Stop(); 
        audioSource.clip = startScreenMusic; 
        audioSource.loop = true; 
        audioSource.Play();
        Debug.Log("GameManager2D: Playing start screen music.");
    }
    else 
    {
        Debug.LogWarning("GameManager2D: Cannot play start screen music - AudioSource or AudioClip missing.");
    }
}
    void PlayActiveMusic()
    {
        if (audioSource != null && activeGameMusic != null)
        {
            if (audioSource.clip == activeGameMusic && audioSource.isPlaying) return; 
            audioSource.Stop(); audioSource.clip = activeGameMusic; audioSource.loop = true; audioSource.Play();
            Debug.Log("GameManager2D: Playing active game music.");
        } else { Debug.LogError("GameManager2D: Cannot play active music - AudioSource or AudioClip missing.");}
    }

    void PlayGameOverMusic()
    {
        if (audioSource != null && gameOverMusic != null)
        {
            if (audioSource.clip == gameOverMusic && audioSource.isPlaying) return;
            audioSource.Stop(); audioSource.clip = gameOverMusic; audioSource.loop = true; audioSource.Play();
            Debug.Log("GameManager2D: Playing game over music.");
        } else { Debug.LogError("GameManager2D: Cannot play game over music - AudioSource or AudioClip missing.");}
    }
    
    // --- Score functions ---
    public void UpdateScore(int scoreToAdd)
    {
        if (!(gameState == GameState.Active) && scoreToAdd > 0) return;
        score += scoreToAdd;
        if (scoreText != null && gameState == GameState.Active) scoreText.text = "Score: " + score;

        OnScoreChanged?.Invoke(score);
        UpdateNextUpgradeUI();
    }

    // --- Lives functions ---
    /*public void LoseLife()
    {
        if (!(gameState == GameState.Active)) return; 
        if (currentLives > 0)
        {
            currentLives--;
            Debug.Log("Player lost a life. Lives remaining: " + currentLives);
            if (livesText != null && (gameState == GameState.Active)) UpdateLivesDisplay(); 
            if (currentLives <= 0) GameOver();
        }
    }

    public int GetCurrentLives() { return currentLives; }*/

    public void UpdateLivesDisplay()
    {
        if (livesText != null && playerPlane != null)
        {
            PlaneHealth playerHealth = playerPlane.GetComponent<PlaneHealth>();
            if (playerHealth != null)
            {
                livesText.text = "Lives: " + playerHealth.GetCurrentHealth();
            }
            else
            {
                Debug.LogError("playerHealth null");
            }
        } else
        {
            Debug.LogError("livesText " + livesText + " or playerPlane " + playerPlane + " is null");
        }
    }     

    /*public void UpdateHealth(int healthRemaining)
    {
        if (livesText != null) livesText.text = "Lives: " + healthRemaining;
        currentLives = healthRemaining;
    }*/

    public void HandlePlayerDefeat()
    {
        if (!(gameState == GameState.Active)) return;
        GameOver();
    }

    void UpdateNextUpgradeUI()
    {
        if (upgradeManager == null || nextUpgradeText == null) return;

        int nextScoreGoal = upgradeManager.GetNextUpgradeScore();
        if (nextScoreGoal != -1)
        {
            nextUpgradeText.gameObject.SetActive(true);
            nextUpgradeText.text = "Next Upgrade: " + nextScoreGoal;
        }
        else
        {
            // All upgrades collected, hide the text
            nextUpgradeText.gameObject.SetActive(false);
        }
    }

    void UpdateUpgradeTimerUI()
    {
        // Find the player's upgrade manager if we don't have it yet
        if (playerUpgradeManager == null)
        {
            playerUpgradeManager = FindObjectOfType<PlayerUpgradeManager>();
        }

        if (playerUpgradeManager != null && upgradeTimerText != null)
        {
            float timeLeft = playerUpgradeManager.GetMaxTimeLeft();
            if (timeLeft > 0)
            {
                upgradeTimerText.gameObject.SetActive(true);
                upgradeTimerText.text = "Power-Up: " + timeLeft.ToString("F1") + "s";
            }
            else
            {
                upgradeTimerText.gameObject.SetActive(false);
            }
        }
        else if (upgradeTimerText != null)
        {
            // Hide the timer if there's no player
            upgradeTimerText.gameObject.SetActive(false);
        }
    }

    public void UpdatePlayerStatsUI()
    {
        // The GameManager is responsible for finding the player controller
        if (playerController == null)
        {
            // We use FindObjectOfType because the player is instantiated at runtime.
            playerController = FindObjectOfType<PlayerController2D>();
            if (playerController == null)
            {
                // If there's still no player, we can't update the UI, so we exit.
                return;
            }
        }
        
        // Now that we have a reference, update the text fields.
        if (playerInformationText != null)
            playerInformationText.text = 
            "H-Speed: " + playerController.horizontalSpeed.ToString("F1") +
            " V-Speed: " + playerController.verticalSpeed.ToString("F1") +
            " Fire Rate per minute: " + playerController.GetCurrentFireRate().ToString("F0")
            ;
            
    }
}
