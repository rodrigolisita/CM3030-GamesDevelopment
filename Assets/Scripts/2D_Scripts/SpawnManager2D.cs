using UnityEngine;

public class SpawnManager2D : MonoBehaviour
{
    public GameObject[] enemyPrefabs;

    [SerializeField]
    private float spawnRangeX = 7.0f; 
    [SerializeField]
    private float spawnY = 5.0f;    

    private float startDelay = 2.0f; // Delay before the first spawn *after* spawning is started
    private float currentSpawnInterval = 1.5f; // Default, will be set by GameManager2D

    private bool isSpawningActive = false;

    void Start()
    {
        // Spawning is no longer started automatically here.
        // GameManager2D will call BeginSpawningEnemies().
        Debug.Log("SpawnManager2D Start(): Waiting for GameManager2D to initiate spawning.");
    }

    public void BeginSpawningEnemies(float interval)
    {
        if (GameManager2D.Instance == null || !GameManager2D.Instance.isGameActive)
        {
            Debug.LogWarning("SpawnManager2D: BeginSpawningEnemies called, but game is not active or GameManager2D is missing. Spawning aborted.");
            return;
        }

        currentSpawnInterval = interval;
        isSpawningActive = true;

        CancelInvoke("SpawnRandomEnemy"); // Stop any previous InvokeRepeating, just in case
        InvokeRepeating("SpawnRandomEnemy", startDelay, currentSpawnInterval);
        Debug.Log("SpawnManager2D: Began spawning enemies with interval: " + currentSpawnInterval);
    }

    public void StopSpawningEnemies()
    {
        isSpawningActive = false;
        CancelInvoke("SpawnRandomEnemy");
        Debug.Log("SpawnManager2D: Stopped spawning enemies.");
    }

    void SpawnRandomEnemy()
    {
        // Check instance and game state again, as InvokeRepeating continues until cancelled
        if (GameManager2D.Instance != null && GameManager2D.Instance.isGameActive && isSpawningActive)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("SpawnManager2D: Enemy prefabs array is not assigned or is empty.", gameObject);
                return;
            }

            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
            Vector3 spawnPos = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), spawnY, 0);
            
            if (enemyPrefabs[enemyIndex] != null)
            {
                Instantiate(enemyPrefabs[enemyIndex], spawnPos, enemyPrefabs[enemyIndex].transform.rotation);
            }
            else
            {
                Debug.LogWarning("SpawnManager2D: Enemy prefab at index " + enemyIndex + " is null.", gameObject);
            }
        }
        else if (!isSpawningActive)
        {
            // This case handles if StopSpawningEnemies was called.
            Debug.Log("SpawnManager2D: SpawnRandomEnemy called, but isSpawningActive is false. Cancelling further invokes.");
            CancelInvoke("SpawnRandomEnemy");
        }
        // No need to log error if GameManager2D.Instance is null here, as BeginSpawningEnemies should have handled it.
    }
}
