using UnityEngine;
using System.Collections;

public class SpawnManager2D : MonoBehaviour
{
    public static SpawnManager2D Instance { get; private set; } // Singleton

    [Header("Assets")]
    public GameObject[] enemyPrefabs;

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
    }


    public void OnEnemyDestroyed()
    {
        enemiesRemaining--;
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
        yield return new WaitForSeconds(startDelay);

        while (isSpawningActive)
        {
            enemiesRemaining = currentWaveSize;
            for (int i = 0; i < currentWaveSize; i++)
            {
                SpawnRandomEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }
            
            yield return new WaitUntil(() => enemiesRemaining <= 0);
            
            Debug.Log("Wave cleared! Next wave in " + currentWaveInterval + " seconds.");
            yield return new WaitForSeconds(currentWaveInterval); // Use the dynamic interval
        }
    }

    void SpawnRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        float randomX = Random.Range(BoundaryManager.Instance.PaddedMinX, BoundaryManager.Instance.PaddedMaxX);
        float spawnY = BoundaryManager.Instance.PaddedMaxY;
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0);

        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
        Instantiate(enemyPrefabs[enemyIndex], spawnPos, enemyPrefabs[enemyIndex].transform.rotation);
    }
}