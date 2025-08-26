using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SpawnManager2D : MonoBehaviour
{
    public static SpawnManager2D Instance { get; private set; } // Singleton

    [Header("Assets")]
    public GameObject[] enemyPrefabs;

    // UI VARIABLES
    [Header("UI Settings")]
    [Tooltip("The UI icon prefab representing one enemy.")]
    [SerializeField] private GameObject enemyIconPrefab;
    [Tooltip("The parent object with a Horizontal Layout Group to hold the icons.")]
    [SerializeField] private Transform iconLayoutGroup;
    [Tooltip("The text element that displays the count of extra enemies.")]
    [SerializeField] private TextMeshProUGUI extraEnemiesText;
    [Tooltip("The maximum number of enemy icons to display on screen at once.")]
    [SerializeField] private int maxIconsToShow = 10;

    [Header("Wave Settings")]
    [Tooltip("The initial number of enemies per wave.")]
    [SerializeField] private int initialWaveSize = 1;
    [Tooltip("The maximum number of enemies per wave.")]
    [SerializeField] private int maxWaveSize = 5;
    [Tooltip("The score required to add another enemy to the wave.")]
    [SerializeField] private int scoreStepForWaveIncrease = 200;
    
    [Header("Timing Settings")]
    [Tooltip("The delay before the first wave spawns.")]
    [Header("Timing Settings")]
    [SerializeField] private float startDelay = 2.0f;
    [SerializeField] private float initialWaveInterval = 3.0f; // Start with a 3s delay between waves
    [SerializeField] private float minimumWaveInterval = 1.0f; // The fastest delay between waves
    [SerializeField] private float intervalReductionPerStep = 0.1f; // How much to reduce the delay
    [SerializeField] private int scoreStepForSpeedUp = 100; // Score needed to speed up
    [SerializeField] private float spawnInterval = 0.5f; // Time between each enemy *within* a wave

    private bool isSpawningActive = false;
    private int currentWaveSize;
    private float currentWaveInterval;
    private int enemiesRemaining;
    private int initialDifficulty = 1;

    // A list to keep track of the active UI icons
    private List<GameObject> activeIcons = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        GameManager2D.OnScoreChanged += UpdateDifficulty;
    }

    private void OnDisable()
    {
        GameManager2D.OnScoreChanged -= UpdateDifficulty;
    }

    public void BeginSpawningEnemies(int difficulty)
    {
        initialDifficulty = difficulty;
        currentWaveSize = initialWaveSize * initialDifficulty;
        currentWaveInterval = initialWaveInterval / initialDifficulty; // Set initial interval

        isSpawningActive = true;
        StartCoroutine(SpawnWaveRoutine());
    }

    public void StopSpawningEnemies()
    {
        isSpawningActive = false;
        StopAllCoroutines();
        ClearEnemyIcons(); // Clear any remaining icons when the game stops
    }


    public void OnEnemyDestroyed()
    {
        enemiesRemaining--;

        // Every time an enemy is destroyed, just redraw the UI.
        UpdateEnemyIconsUI();
        
        // Remove one icon from the display
        //if (activeIcons.Count > 0)
        //{
            // Get the last icon in the list.
        //    GameObject iconToRemove = activeIcons[activeIcons.Count - 1];
            // Remove it from our tracking list.
        //    activeIcons.RemoveAt(activeIcons.Count - 1);
            // Destroy the icon's GameObject.
        //    Destroy(iconToRemove);
        //}
    }

    // This method now handles both wave size and speed increases.
    private void UpdateDifficulty(int newScore)
    {
        if (!isSpawningActive) return;

        // --- Wave Size Logic ---
        int waveIncreaseSteps = newScore / scoreStepForWaveIncrease;
        int newWaveSize = initialWaveSize * initialDifficulty + waveIncreaseSteps;
        currentWaveSize = Mathf.Min(newWaveSize, maxWaveSize * initialDifficulty);

        // --- Spawn Speed Logic ---
        int speedUpSteps = newScore / scoreStepForSpeedUp;
        float newInterval = (initialWaveInterval / initialDifficulty) - (speedUpSteps * intervalReductionPerStep);
        currentWaveInterval = Mathf.Max(newInterval, minimumWaveInterval);
    }

    private IEnumerator SpawnWaveRoutine()
{
    // Initial delay before the very first wave
    yield return new WaitForSeconds(startDelay);

    while (isSpawningActive)
    {
        // 1. Set the counter and show the icons for the UPCOMING wave immediately.
        enemiesRemaining = currentWaveSize;
        UpdateEnemyIconsUI(); // This now correctly shows the icons and + text
        Debug.Log("Next wave will have " + enemiesRemaining + " enemies. Spawning in " + currentWaveInterval + " seconds.");

        // 2. Wait for the interval between waves BEFORE spawning.
        yield return new WaitForSeconds(currentWaveInterval);

        // 3. Spawn all the enemies for the wave, culling any extras.
        List<GameObject> allEnemiesInWave = new List<GameObject>();
        while (allEnemiesInWave.Count < currentWaveSize)
        {
            allEnemiesInWave.AddRange(SpawnRandomEnemyGroup());
            // A small delay between spawning groups to spread them out.
            if (allEnemiesInWave.Count < currentWaveSize)
            {
                 yield return new WaitForSeconds(spawnInterval);
            }
        }

        // 4. Cull any excess enemies to match the exact wave size.
        // This is important for keeping the enemy count accurate.
        while (allEnemiesInWave.Count > currentWaveSize)
        {
            GameObject enemyToDestroy = allEnemiesInWave[allEnemiesInWave.Count - 1];
            allEnemiesInWave.RemoveAt(allEnemiesInWave.Count - 1);
            Destroy(enemyToDestroy);
        }
        
        Debug.Log("Wave active with " + enemiesRemaining + " enemies.");

        // 5. Wait until the wave is cleared.
        yield return new WaitUntil(() => enemiesRemaining <= 0);
        
        Debug.Log("Wave cleared!");
        // The loop will now repeat, showing the icons for the next wave and then waiting again.
    }
}

// --- The CreateEnemyIcons and ClearEnemyIcons methods are now combined and improved ---
private void UpdateEnemyIconsUI()
{
    // 1. Clear any old icons from the screen.
    foreach (GameObject icon in activeIcons)
    {
        Destroy(icon);
    }
    activeIcons.Clear();

    // 2. Hide the extra text by default.
    if (extraEnemiesText != null)
    {
        extraEnemiesText.gameObject.SetActive(false);
    }
    
    // Safety check
    if (enemyIconPrefab == null || iconLayoutGroup == null) return;

    // 3. Create the visible icons.
    int iconsToCreate = Mathf.Min(enemiesRemaining, maxIconsToShow);
    for (int i = 0; i < iconsToCreate; i++)
    {
        GameObject newIcon = Instantiate(enemyIconPrefab, iconLayoutGroup);
        activeIcons.Add(newIcon);
    }

    // 4. If there are more enemies than icons, show and update the extra text.
    if (enemiesRemaining > maxIconsToShow)
    {
        if (extraEnemiesText != null)
        {
            int extraCount = enemiesRemaining - maxIconsToShow;
            extraEnemiesText.text = "+" + extraCount;
            extraEnemiesText.gameObject.SetActive(true);

            // This tells the text object to move to the end of the layout group's child list.
            extraEnemiesText.transform.SetAsLastSibling();
        }
    }
}


































































    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    // returns a List of the individual enemies it created.
    List<GameObject> SpawnRandomEnemyGroup()
    {
        List<GameObject> spawnedEnemies = new List<GameObject>();
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return spawnedEnemies;

        float randomX = Random.Range(BoundaryManager.Instance.PaddedMinX, BoundaryManager.Instance.PaddedMaxX);
        float spawnY = BoundaryManager.Instance.PaddedMaxY;
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0);

        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject newEnemyGroup = Instantiate(enemyPrefabs[enemyIndex], spawnPos, enemyPrefabs[enemyIndex].transform.rotation);

        // Find all individual enemies within the group and add them to our list.
        foreach (EnemyCollisionHandler enemy in newEnemyGroup.GetComponentsInChildren<EnemyCollisionHandler>())
        {
            spawnedEnemies.Add(enemy.gameObject);
        }
        
        return spawnedEnemies;

    }

    // METHODS for managing UI icons
    private void CreateEnemyIcons(int count)
    {
        // First, clear any old icons that might exist.
        ClearEnemyIcons();

        // Check if the prefab and layout group are assigned.
        if (enemyIconPrefab == null || iconLayoutGroup == null)
        {
            Debug.LogWarning("Enemy Icon Prefab or Layout Group not assigned in SpawnManager!");
            return;
        }

        // Create a new icon for each enemy in the wave.
        for (int i = 0; i < count; i++)
        {
            GameObject newIcon = Instantiate(enemyIconPrefab, iconLayoutGroup);
            activeIcons.Add(newIcon);
        }
    }

    private void ClearEnemyIcons()
    {
        foreach (GameObject icon in activeIcons)
        {
            Destroy(icon);
        }
        activeIcons.Clear();
    }
}