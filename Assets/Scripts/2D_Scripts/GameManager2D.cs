using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    // --- Game State Variables ---
    private int score;
    private int currentLives;

    [Header("Gameplay Settings")]
    public int initialLives = 3;
    public bool isGameActive = false; // Game starts as inactive

    // --- Audio Management ---
    private AudioSource audioSource;
    [Header("Audio Clips")]
    public AudioClip startScreenMusic;
    public AudioClip activeGameMusic;
    public AudioClip gameOverMusic;
    

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
            isGameActive = false; 
            currentLives = initialLives; 
            Debug.Log("GameManager2D Awake(): Initial internal state set. isGameActive: " + isGameActive + ", currentLives: " + currentLives);
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
        Debug.Log("GameManager2D Start() called. Initial UI setup is handled by OnSceneLoaded.");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // This runs every time a scene finishes loading, INCLUDING THE FIRST SCENE.
        Debug.Log("GameManager2D OnSceneLoaded: Scene '" + scene.name + "' loaded. Current isGameActive (from persistent instance): " + isGameActive);
        FindUIElements();       // Re-find UI elements in the newly loaded scene
        UpdateAllUIDisplays();  // Update their visibility based on current game state

        if (isGameActive)
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
    }

    void ResetInternalGameState()
    {
        score = 0;
        currentLives = initialLives;
        Debug.Log("GameManager2D ResetInternalGameState: Score: " + score + ", Lives: " + currentLives + ". (isGameActive is currently: " + isGameActive + ")");
    }

    void UpdateAllUIDisplays()
    {
        bool isPreGameState = !isGameActive && currentLives > 0; 
        bool isTrulyGameOver = !isGameActive && currentLives <= 0; 

        Debug.Log("GameManager2D UpdateAllUIDisplays: isGameActive=" + isGameActive + ", currentLives=" + currentLives + " -> isPreGameState=" + isPreGameState + ", isTrulyGameOver=" + isTrulyGameOver);

        if (titleScreen != null)
        {
            titleScreen.SetActive(isPreGameState);
            Debug.Log("GameManager2D: TitleScreen active state set to: " + isPreGameState);
        }
        else
        {
            Debug.LogError("GameManager2D UpdateAllUIDisplays: titleScreen reference is NULL. Cannot set its active state.");
        }

        if (easyButton != null)
        {
            easyButton.gameObject.SetActive(isPreGameState);
            Debug.Log("GameManager2D: EasyButton active state set to: " + isPreGameState);
        }
        else
        {
            Debug.LogWarning("GameManager2D UpdateAllUIDisplays: easyButton reference is NULL. Cannot set its active state.");
        }
        
        if (mediumButton != null)
        {
            mediumButton.gameObject.SetActive(isPreGameState);
            Debug.Log("GameManager2D: MediumButton active state set to: " + isPreGameState);
        }
        else
        {
            Debug.LogWarning("GameManager2D UpdateAllUIDisplays: mediumButton reference is NULL. Cannot set its active state.");
        }

        if (scoreText != null) 
        {
            scoreText.gameObject.SetActive(isGameActive);
            if(isGameActive) scoreText.text = "Score: " + score; 
        }
        if (livesText != null) 
        {
            livesText.gameObject.SetActive(isGameActive);
            if(isGameActive) UpdateLivesDisplay(); 
        }
        
        if (gameOverText != null) 
        {
            gameOverText.gameObject.SetActive(isTrulyGameOver);
        }
        if (restartButton != null) 
        {
            restartButton.gameObject.SetActive(isTrulyGameOver);
        }

        if (playInstructionText != null)
        {
            playInstructionText.gameObject.SetActive(isPreGameState);
        }
    }

    // --- Game Lifecycle Methods ---
    //public void StartGame(float difficultySpawnInterval) 
    public void StartGame(int difficultyLevel) 
    {
        Debug.Log("GameManager2D: StartGame() called with spawnInterval: " + difficultyLevel);
        isGameActive = true;
        ResetInternalGameState(); 
        UpdateAllUIDisplays();    
        PlayActiveMusic();

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
        if (!isGameActive && currentLives <= 0) return; 
      
        Debug.Log("GameManager2D: GameOver() called.");
        isGameActive = false; 
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
        isGameActive = false; 
        ResetInternalGameState(); 
        
        SpawnManager2D spawnManager = FindObjectOfType<SpawnManager2D>();
        if (spawnManager != null)
        {
            spawnManager.StopSpawningEnemies();
        }
        Debug.Log("GameManager2D: State before loading scene in RestartGame - isGameActive: " + isGameActive + ", currentLives: " + currentLives);
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
        if (!isGameActive && scoreToAdd > 0) return;
        score += scoreToAdd;
        if (scoreText != null && isGameActive) scoreText.text = "Score: " + score;
    }

    // --- Lives functions ---
    public void LoseLife()
    {
        if (!isGameActive) return; 
        if (currentLives > 0)
        {
            currentLives--;
            Debug.Log("Player lost a life. Lives remaining: " + currentLives);
            if (livesText != null && isGameActive) UpdateLivesDisplay(); 
            if (currentLives <= 0) GameOver();
        }
    }

    public int GetCurrentLives() { return currentLives; }

    void UpdateLivesDisplay()
    {
        if (livesText != null) livesText.text = "Lives: " + currentLives;
    }     

    public void UpdateHealth(int healthRemaining)
    {
        if (livesText != null) livesText.text = "Lives: " + healthRemaining;
    }

    public void HandlePlayerDefeat()
    {
        if (!isGameActive) return;
        GameOver();
    }
}
