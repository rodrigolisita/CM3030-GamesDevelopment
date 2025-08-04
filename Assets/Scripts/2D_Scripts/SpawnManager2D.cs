using UnityEngine;

public class SpawnManager2D : MonoBehaviour
{
    public GameObject[] enemyPrefabs;

    // We removed the serialized fields for spawnRangeX and spawnY
    // as they are now calculated automatically.

    private float startDelay = 2.0f;
    private float defaultSpawnInterval = 1.5f;
    private bool isSpawningActive = false;

    void Start()
    {
        Debug.Log("SpawnManager2D Start(): Waiting for GameManager2D to initiate spawning.");
    }

    public void BeginSpawningEnemies(int difficulty)
    {
        if (GameManager2D.Instance == null || !(GameManager2D.Instance.gameState == GameState.Active))
        {
            Debug.LogWarning("SpawnManager2D: BeginSpawningEnemies called, but game is not active. Aborting.");
            return;
        }

        float currentSpawnInterval = defaultSpawnInterval / difficulty;
        isSpawningActive = true;

        CancelInvoke("SpawnRandomEnemy");
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
        if (GameManager2D.Instance != null && (GameManager2D.Instance.gameState == GameState.Active) && isSpawningActive)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("SpawnManager2D: Enemy prefabs array is not assigned or is empty.");
                return;
            }

            // Get the boundaries from the central manager
            float randomX = Random.Range(BoundaryManager.Instance.MinX, BoundaryManager.Instance.MaxX);
            float spawnY = BoundaryManager.Instance.PaddedMaxY;

            Vector3 spawnPos = new Vector3(randomX, spawnY, 0);
            

            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
            if (enemyPrefabs[enemyIndex] != null)
            {
                Instantiate(enemyPrefabs[enemyIndex], spawnPos, enemyPrefabs[enemyIndex].transform.rotation);
            }
            else
            {
                Debug.LogWarning("SpawnManager2D: Enemy prefab at index " + enemyIndex + " is null.");
            }
        }
        else
        {
            // MINIMAL CHANGE: Added a log to explain WHY it's not spawning.
            Debug.LogWarning("SpawnRandomEnemy SKIPPED. isGameActive=" + (GameManager2D.Instance.gameState == GameState.Active) + ", isSpawningActive=" + isSpawningActive);
        }
    }
}