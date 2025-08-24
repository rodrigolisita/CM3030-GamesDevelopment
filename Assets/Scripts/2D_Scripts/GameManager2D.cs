using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;

public enum GameState
{
    ModeSelect,
    DifficultySelect,
    Active,
    BossFight,
    GameOver
}



public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

[RequireComponent(typeof(AudioSource))]
public class GameManager2D : MonoBehaviour
{
    public static GameManager2D Instance { get; private set; }

    // --- UI Element References ---
    [Header("UI Object Names")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI playInstructionText;
    [SerializeField] private TextMeshProUGUI nextUpgradeText;
    [SerializeField] private TextMeshProUGUI upgradeTimerText;
    [SerializeField] private TextMeshProUGUI playerInformationText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button arcadeButton;
    [SerializeField] private Button campaignButton;

    // --- Game State & other private variables ---
    private int score;
    private int initialDifficulty;
    private GameMode gameMode;
    private GameObject playerPlane;
    [Header("Player Plane")]
    public GameObject playerPlanePrefab;
    public GameObject playerSpawnPoint;

    //private UpgradeManager upgradeManager;
    [SerializeField] private UpgradeManager upgradeManager;
    private PlayerUpgradeManager playerUpgradeManager;
    private PlayerController2D playerController;

    [Header("Gameplay Settings")]
    public GameState gameState = GameState.DifficultySelect; // Game starts as PreGame


    // --- Audio Management ---
    private AudioSource audioSource;
    [Header("Audio Clips")]
    public AudioClip startScreenMusic;
    public AudioClip activeGameMusic;
    public AudioClip gameOverMusic;

    // -- Event to announce score changes to any listening scripts.
    public static Action<int> OnScoreChanged;

    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private int scoreToTriggerBoss = 2000;
    [SerializeField] private AudioClip bossFightMusic;
    [SerializeField] private AudioClip victoryFanfare;

    
    private bool bossHasBeenTriggered = false;
    
    
    
    
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject.transform.root.gameObject);

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
            gameState = GameState.ModeSelect;

            Debug.Log("GameManager2D Awake(): Initial internal state set. gameSate: " + gameState);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {

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
        //upgradeManager = FindObjectOfType<UpgradeManager>();
        playerUpgradeManager = null; // We'll find this when the player is active
        playerController = null;

        SpawnNewPlayer();
        UpdateAllUIDisplays();  // Update their visibility based on current game state

        if (gameState == GameState.Active)
        {
            // If the scene loads and the game is already active (e.g., moving to level 2), play game music.
            PlayActiveMusic();
        }
        else
        {
            // If you have menu music and game is not active 
            PlayStartScreenMusic();
        }
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
        bool showStartScreenUI = (gameState == GameState.DifficultySelect || gameState == GameState.ModeSelect);
        bool showModeSelectUI = (gameState == GameState.ModeSelect);
        bool showDifficultySelectUI = (gameState == GameState.DifficultySelect);
        bool showGameHUD = (gameState == GameState.Active);
        bool showGameOverUI = (gameState == GameState.GameOver);

        // --- Start Screen Elements ---
        if (startScreen != null) startScreen.SetActive(showStartScreenUI);
        if (arcadeButton != null) arcadeButton.gameObject.SetActive(showModeSelectUI);
        if (campaignButton != null) campaignButton.gameObject.SetActive(showModeSelectUI);
        if (easyButton != null) easyButton.gameObject.SetActive(showDifficultySelectUI);
        if (mediumButton != null) mediumButton.gameObject.SetActive(showDifficultySelectUI);
        if (hardButton != null) hardButton.gameObject.SetActive(showDifficultySelectUI);
        if (playInstructionText != null) playInstructionText.gameObject.SetActive(showStartScreenUI);

        // --- Game Over Screen Elements ---
        if (gameOverText != null) gameOverText.gameObject.SetActive(showGameOverUI);
        if (restartButton != null) restartButton.gameObject.SetActive(showGameOverUI);

        // --- In-Game HUD Elements ---
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(showGameHUD);
            if (showGameHUD) scoreText.text = "Score: " + score;
        }
        if (livesText != null)
        {
            livesText.gameObject.SetActive(showGameHUD);
            if (showGameHUD) UpdateLivesDisplay();
        }
        if (healthBarFill != null) healthBarFill.transform.parent.gameObject.SetActive(showGameHUD);

        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(showGameHUD);
            if (showGameHUD) UpdatePlayerAmmoUI();
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
        initialDifficulty = difficultyLevel;
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

    public void SelectGameMode(GameModeHolder newMode)
    {
        gameMode = newMode.gameMode;
        gameState = GameState.DifficultySelect;
        UpdateAllUIDisplays();
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

        // 1. Set the game state back to PreGame. This ensures that when the
        //    scene reloads, the title screen will be displayed.
        gameState = GameState.DifficultySelect;

        // 2. Reload the current scene. This is the most reliable way to reset
        //    all game objects (enemies, projectiles, player) to their initial state.
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
        }
        else { Debug.LogError("GameManager2D: Cannot play active music - AudioSource or AudioClip missing."); }
    }

    void PlayGameOverMusic()
    {
        if (audioSource != null && gameOverMusic != null)
        {
            if (audioSource.clip == gameOverMusic && audioSource.isPlaying) return;
            audioSource.Stop(); audioSource.clip = gameOverMusic; audioSource.loop = true; audioSource.Play();
            Debug.Log("GameManager2D: Playing game over music.");
        }
        else { Debug.LogError("GameManager2D: Cannot play game over music - AudioSource or AudioClip missing."); }
    }

    // --- Score functions ---
    public void UpdateScore(int scoreToAdd)
    {
        if (!(gameState == GameState.Active) && scoreToAdd > 0) return;
        score += scoreToAdd;
        if (scoreText != null && gameState == GameState.Active) scoreText.text = "Score: " + score;

        OnScoreChanged?.Invoke(score);
        UpdateNextUpgradeUI();

        if (!bossHasBeenTriggered && score >= scoreToTriggerBoss)
        {
            bossHasBeenTriggered = true; // This ensures it only runs once per game
            //TriggerBossEncounter();
            StartCoroutine(TriggerBossEncounter());
        }
        
        
        
    }


    public void UpdateLivesDisplay()
    {
        if (livesText != null && playerPlane != null)
        {
            PlaneHealth playerHealth = playerPlane.GetComponent<PlaneHealth>();
            if (playerHealth != null)
            {
                int currentHealth = playerHealth.GetCurrentHealth();
                int maxHealth = playerHealth.GetMaxHealth();
                float healthAmount = (float)currentHealth / maxHealth;

                livesText.text = healthAmount * 100 + "%";


                // Calculate the fill amount (a value from 0.0 to 1.0).
                healthBarFill.fillAmount = healthAmount;
            }
            else
            {
                Debug.LogError("playerHealth null");
            }
        }
        else
        {
            Debug.LogError("livesText " + livesText + " or playerPlane " + playerPlane + " is null");
        }
    }

    public void UpdatePlayerAmmoUI()
    {
        if (ammoText != null && playerPlane != null)
        {
            PlayerController2D playerController = playerPlane.GetComponent<PlayerController2D>();
            if (playerController != null)
            {
                int ammo = playerController.GetCurrentAmmo(2); // 2 is the key for the secondary weapon
                if (ammo == -1)
                {
                    ammoText.text = "Secondary Ammo: Unlimited";
                }
                else
                {
                    ammoText.text = "Secondary Ammo: " + ammo;
                }
            }
            else
            {
                Debug.LogError("playerController null");
            }
        }
        else
        {
            Debug.LogError("ammoText " + ammoText + " or playerPlane " + playerPlane + " is null");
        }
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthBarFill != null)
        {
            // Calculate the fill amount as a value between 0 and 1
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

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

    public void PlaySoundEffect(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // Use PlayOneShot to play a sound without interrupting the background music.
            audioSource.PlayOneShot(clip);
        }
    }

    
    private IEnumerator TriggerBossEncounter()
    {
        Debug.Log("BOSS ENCOUNTER TRIGGERED!");

        // 1. Stop the regular enemy spawner.
        FindObjectOfType<SpawnManager2D>()?.StopSpawningEnemies();

        // 2. Change to the boss music.
        PlayMusic(bossFightMusic);

        // --- THIS IS THE FIX ---
        // 3. Wait for a few seconds for dramatic effect.
        // You can make this delay a public variable to change it in the Inspector.
        yield return new WaitForSeconds(3.0f); 

        // 4. Spawn the boss prefab.
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        }
        else
        {
            Debug.LogError("Boss Prefab or Boss Spawn Point is not assigned in the GameManager!");
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    public void OnBossDefeated()
    {
        Debug.Log("BOSS DEFEATED! Displaying victory message.");

        // 1. Award a large score bonus for defeating the boss.
        UpdateScore(1000); // You can make this value a variable in the Inspector

        // 2. Play a victory sound or change the music.
        PlayMusic(victoryFanfare);

        // 3. Display a victory message on the screen.
        // We can reuse the gameOverText for this.
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "YOU WIN!";
        }

        // 4. After a short delay, you could end the level or restart.
        // For now, we can just stop the game.
        gameState = GameState.GameOver; // Or a new "LevelComplete" state
        UpdateAllUIDisplays();
    }

































    
    private void PlayMusic(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // Don't restart the music if this track is already playing
            if (audioSource.clip == clip && audioSource.isPlaying) return;
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
    
    
    
    
        
    
    
        
    
    
}
