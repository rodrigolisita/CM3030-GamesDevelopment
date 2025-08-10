using UnityEngine;

public class SpawnManager2D : MonoBehaviour
{
    [Header("Assets")]
    [Tooltip("An array of enemy prefabs to be spawned.")]
    public GameObject[] enemyPrefabs;

    [Header("Difficulty Settings")]
    [Tooltip("The initial time between spawns for difficulty level 1.")]
    [SerializeField] private float initialSpawnInterval = 1.5f;
    [Tooltip("The fastest possible spawn interval, no matter the score.")]
    [SerializeField] private float minimumSpawnInterval = 0.3f;
    [Tooltip("How much to reduce the interval by each time the difficulty increases.")]
    [SerializeField] private float spawnIntervalReduction = 0.01f;
    [Tooltip("The score required to trigger a speed increase.")]
    [SerializeField] private int scoreStepForSpeedUp = 20;

    [Tooltip("The delay before the first enemy spawns.")]
    [SerializeField] private float startDelay = 2.0f;

    private bool isSpawningActive = false;
    private float currentSpawnInterval;
    private int initialDifficulty = 1;

    // Subscribe to score updates when the spawner becomes active.
    private void OnEnable()
    {
        GameManager2D.OnScoreChanged += UpdateDifficulty;
    }

    // Unsubscribe when the spawner is disabled to prevent errors.
    private void OnDisable()
    {
        GameManager2D.OnScoreChanged -= UpdateDifficulty;
    }

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        Debug.Log("SpawnManager2D Start(): Waiting for GameManager2D to initiate spawning.");
    }

    /// <summary>
    /// This public method is called by the GameManager to begin spawning enemies.
    /// </summary>
    public void BeginSpawningEnemies(int difficulty)
    {
        initialDifficulty = difficulty;
        currentSpawnInterval = initialSpawnInterval / initialDifficulty;
        isSpawningActive = true;

        // Start calling the SpawnRandomEnemy method repeatedly.
        CancelInvoke("SpawnRandomEnemy"); // Ensure no previous invokes are running
        InvokeRepeating("SpawnRandomEnemy", startDelay, currentSpawnInterval);
        Debug.Log("SpawnManager2D: Began spawning enemies with interval: " + currentSpawnInterval);
    }

    /// <summary>
    /// This public method is called by the GameManager to stop spawning enemies.
    /// </summary>
    public void StopSpawningEnemies()
    {
        isSpawningActive = false;
        CancelInvoke("SpawnRandomEnemy");
        Debug.Log("SpawnManager2D: Stopped spawning enemies.");
    }

    // This method is called automatically by the GameManager whenever the score changes.
    private void UpdateDifficulty(int newScore)
    {
        if (!isSpawningActive) return;

        // Calculate how many times the difficulty should have increased based on score.
        int speedUpSteps = newScore / scoreStepForSpeedUp;

        // Calculate the new target interval.
        float newInterval = (initialSpawnInterval / initialDifficulty) - (speedUpSteps * spawnIntervalReduction);

        // Ensure the interval never goes below the minimum limit.
        newInterval = Mathf.Max(newInterval, minimumSpawnInterval);

        // If the calculated interval is faster than our current one, update it.
        if (newInterval < currentSpawnInterval)
        {
            currentSpawnInterval = newInterval;
            CancelInvoke("SpawnRandomEnemy");
            InvokeRepeating("SpawnRandomEnemy", currentSpawnInterval, currentSpawnInterval);
            Debug.Log("DIFFICULTY INCREASED! New spawn interval: " + currentSpawnInterval);
        }
    }

    /// <summary>
    /// Spawns a single random enemy at a position just off the top of the screen.
    /// </summary>
    void SpawnRandomEnemy()
    {
        if (isSpawningActive)
        {
            // Check if the array is empty before trying to access it.
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogError("SpawnManager2D: Enemy prefabs array has not been assigned in the Inspector!");
                isSpawningActive = false; // Stop trying to spawn to prevent more errors
                return;
            }
            
            // This spawning logic remains exactly the same.
            float randomX = Random.Range(BoundaryManager.Instance.MinX, BoundaryManager.Instance.MaxX);
            float spawnY = BoundaryManager.Instance.PaddedMaxY;
            Vector3 spawnPos = new Vector3(randomX, spawnY, 0);
            
            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[enemyIndex], spawnPos, enemyPrefabs[enemyIndex].transform.rotation);
        }
    }
}