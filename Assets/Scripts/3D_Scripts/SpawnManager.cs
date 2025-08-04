using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    private float spawnRangeX = 24.0f;
    private float spawnRangeZ = 12.0f;

    private float startDelay = 2;
    private float spawnInterval = 1.5f;

    GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Create enemmies at timed intervals
        InvokeRepeating("SpawnRandomEnemy", startDelay, spawnInterval);
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
      
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SpawnRandomEnemy(){

        if((GameManager2D.Instance.gameState == GameState.Active))
        {
            int enemyIndex = Random.Range(0,enemyPrefabs.Length);
            Vector3 spawnPos = new Vector3(Random.Range(-spawnRangeX,spawnRangeX), 1.2f, spawnRangeZ);
            Instantiate(enemyPrefabs[enemyIndex], spawnPos, enemyPrefabs[enemyIndex].transform.rotation);
        }
        
        
        
    }
}
