using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SpawnManager2D : MonoBehaviour
{
    public static SpawnManager2D Instance { get; private set; }

    [Header("Default Arcade Mode")]
    [Tooltip("The Wave Definition asset used when playing in Arcade Mode.")]
    [SerializeField] private WaveSO arcadeWaveDefinition;
    
    // UI Settings
    [Header("UI Settings")]
    [SerializeField] private GameObject enemyIconPrefab;
    [SerializeField] private Transform iconLayoutGroup;
    [SerializeField] private TextMeshProUGUI extraEnemiesText;
    [SerializeField] private TextMeshProUGUI waveCountDisplay;
    [SerializeField] private int maxIconsToShow = 10;
    
    [SerializeField] private float startDelay = 2.0f;



    // Private state variables
    private bool isSpawningActive = false;
    private int enemiesRemaining;
    private int currentScore;
    private WaveSO activeWaveDef; // The ruleset we are currently using
    private List<GameObject> activeIcons = new List<GameObject>();
    private int waveCount = 0;

    void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; 
            InitializeIconPool();
        } 
        else 
        { 
            Destroy(gameObject); 
        }
    }

    private void OnEnable()
    {
        // Subscribe to the enemy defeated event
        EnemyCollisionHandler.OnAnyEnemyDefeated += HandleEnemyDefeated;

        GameManager2D.OnScoreChanged += (newScore) => { currentScore = newScore; };
    }

    private void OnDisable()
    {
        EnemyCollisionHandler.OnAnyEnemyDefeated -= HandleEnemyDefeated;

        // ensure the delegate has a target before removing
        if (GameManager2D.OnScoreChanged != null)
        {
            GameManager2D.OnScoreChanged -= (newScore) => { currentScore = newScore; };
        }
    }

    public void BeginSpawningEnemies(GameMode mode, int difficulty, Mission missionData = null)
    {
        isSpawningActive = true;
        currentScore = 0; // Reset score tracking

        if (mode == GameMode.Campaign && missionData != null)
        {
            activeWaveDef = missionData.GetWaveDefinition();
        }
        else // Arcade Mode
        {
            activeWaveDef = arcadeWaveDefinition;
        }

        StartCoroutine(UnifiedSpawnRoutine());
    }

    public void ResumeSpawningEnemies()
    {
        isSpawningActive = true;

        InitializeIconPool();

        StartCoroutine(UnifiedSpawnRoutine());
    }

    private IEnumerator UnifiedSpawnRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        while (isSpawningActive)
        {
            // 1. Calculate the target size and interval for the upcoming wave.
            int waveIncreaseSteps = currentScore / activeWaveDef.scoreStepForWaveIncrease;
            int targetWaveSize = Mathf.Min(activeWaveDef.initialWaveSize + waveIncreaseSteps, activeWaveDef.maxWaveSize);

            int speedUpSteps = currentScore / activeWaveDef.scoreStepForSpeedUp;
            float currentWaveInterval = Mathf.Max(activeWaveDef.initialWaveInterval - (speedUpSteps * activeWaveDef.intervalReductionPerStep), activeWaveDef.minimumWaveInterval);

            // 2. Set the counter and update the UI immediately to give the player a heads-up.
            enemiesRemaining = targetWaveSize;
            UpdateEnemyIconsUI();

            // Update the wave count
            waveCount += 1;
            UpdateWaveCountUI();

            // 3. NOW, pause for the "breather" time between waves.
            yield return new WaitForSeconds(currentWaveInterval);

            // --- SPAWNING & CULLING LOGIC ---
            List<GameObject> allEnemiesInWave = new List<GameObject>();
            while (allEnemiesInWave.Count < targetWaveSize)
            {
                if (activeWaveDef.enemyPrefabs == null || activeWaveDef.enemyPrefabs.Count == 0)
                {
                    Debug.LogError("The Enemy Prefabs list for the active WaveSO is empty!");
                    yield break; 
                }

                GameObject prefabToSpawn = activeWaveDef.enemyPrefabs[Random.Range(0, activeWaveDef.enemyPrefabs.Count)];
                allEnemiesInWave.AddRange(SpawnEnemyGroup(prefabToSpawn));

                if (allEnemiesInWave.Count < targetWaveSize)
                {
                    yield return new WaitForSeconds(activeWaveDef.spawnInterval);
                }
            }

            while (allEnemiesInWave.Count > targetWaveSize)
            {
                GameObject enemyToDestroy = allEnemiesInWave[allEnemiesInWave.Count - 1];
                allEnemiesInWave.RemoveAt(allEnemiesInWave.Count - 1);
                Destroy(enemyToDestroy);
            }

            // --- THE FAILSAFE FIX ---
            // 4. Silently correct the counter to the ACTUAL number of enemies.
            //enemiesRemaining = allEnemiesInWave.Count;
  
            // We update the UI again in case the count was different (e.g., from culling).
            UpdateEnemyIconsUI();

            // Wait for the wave to be cleared
            yield return new WaitUntil(() => enemiesRemaining <= 0);
            Debug.Log("Wave cleared!");

            GameManager2D.Instance.WaveDefeated(waveCount);
        }
    }
    
    
                
        
    
    // This is the method that will be called when the event happens
    private void HandleEnemyDefeated(GameObject enemy)
    {
        OnEnemyDestroyed();
    }               
    
    // --- HELPER METHODS ---
    // This is called by the gameManager as a failsafe
    // force the correct enemiesRemaining, just in case...
    public int GetEnemiesRemaining()
    {
        return enemiesRemaining;
    }
    public void ForceCorrectEnemyCount(int actualCount)
    {
        enemiesRemaining = actualCount;
        UpdateEnemyIconsUI();
    }
    
    // ---
    public void StopSpawningEnemies()
    {
        isSpawningActive = false;
        StopAllCoroutines();
        ClearEnemyIcons();
    }

    // Method called by the event when enemy is defeated
    public void OnEnemyDestroyed()
    {
        enemiesRemaining--;
        UpdateEnemyIconsUI();
    }
    
    private List<GameObject> SpawnEnemyGroup(GameObject enemyGroupPrefab)
    {
        List<GameObject> spawnedEnemies = new List<GameObject>();
        if (enemyGroupPrefab == null) return spawnedEnemies;
        
        float randomX = Random.Range(BoundaryManager.Instance.PaddedMinX, BoundaryManager.Instance.PaddedMaxX);
        float spawnY = BoundaryManager.Instance.PaddedMaxY;
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0);
        GameObject newEnemyGroup = Instantiate(enemyGroupPrefab, spawnPos, enemyGroupPrefab.transform.rotation);

        if (activeWaveDef != null && activeWaveDef.overrideEnemyColor)
        {
            // Find all EnemyColorizer scripts on the spawned enemies.
            EnemyColorizer[] colorizers = newEnemyGroup.GetComponentsInChildren<EnemyColorizer>();
            foreach (EnemyColorizer colorizer in colorizers)
            {
                // Apply the tint color from the WaveSO.
                colorizer.ApplyColor(activeWaveDef.enemyTintColor);
            }
        }

        foreach (EnemyCollisionHandler enemy in newEnemyGroup.GetComponentsInChildren<EnemyCollisionHandler>())
        {
            spawnedEnemies.Add(enemy.gameObject);
        }
        
        return spawnedEnemies;
    }
    
    private void InitializeIconPool()
    {
        // Safety check
        if (enemyIconPrefab == null || iconLayoutGroup == null) return;

        // Create the maximum number of icons we'll ever need to show and add them to our list.
        for (int i = 0; i < maxIconsToShow; i++)
        {
            GameObject newIcon = Instantiate(enemyIconPrefab, iconLayoutGroup);
            newIcon.SetActive(false); // Start with the icon hidden
            activeIcons.Add(newIcon);
        }
    }
    
    private void UpdateEnemyIconsUI()
    {
        // Determine how many icons should be visible.
        int iconsToShow = Mathf.Min(enemiesRemaining, maxIconsToShow);

        // Loop through our pre-made pool of icons.
        for (int i = 0; i < activeIcons.Count; i++)
        {
            // If this icon should be visible, activate it.
            if (i < iconsToShow)
            {
                activeIcons[i].SetActive(true);
            }
            // Otherwise, deactivate it.
            else
            {
                activeIcons[i].SetActive(false);
            }
        }

        // Handle the "+5" text for enemies that exceed the icon count.
        if (enemiesRemaining > maxIconsToShow)
        {
            if (extraEnemiesText != null)
            {
                int extraCount = enemiesRemaining - maxIconsToShow;
                extraEnemiesText.text = "+" + extraCount;
                extraEnemiesText.gameObject.SetActive(true);
                extraEnemiesText.transform.SetAsLastSibling();
            }
        }
        else
        {
            // Make sure the extra text is hidden if not needed.
            if (extraEnemiesText != null)
            {
                extraEnemiesText.gameObject.SetActive(false);
            }
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



    private void UpdateWaveCountUI()
    {
        if (waveCountDisplay != null)
        {
            waveCountDisplay.text = "Wave " + waveCount;
        }
    }

    public int GetWaveCount()
    {
        return waveCount;
    }

    public void ResetWaveCount()
    {
        waveCount = 0;
    }

}