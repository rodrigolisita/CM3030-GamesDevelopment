using UnityEngine;

public class SpawnManager2D : MonoBehaviour
{
    public GameObject[] enemyPrefabs;

    [SerializeField]
    private float spawnRangeX;
    [SerializeField]
    private float spawnY;

    private float startDelay = 2;
    private float spawnInterval = 1.5f;

    GameManager2D gameManager; // To stop the game when the state is game over

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("SpawnRandomEnemy", startDelay, spawnInterval);
        gameManager = GameObject.Find("GameManager2D").GetComponent<GameManager2D>();

        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)){
            SpawnRandomEnemy();
            
            
        }
    }

    void SpawnRandomEnemy(){

        if(gameManager.isGameActive){
            int enemyIndex = Random.Range(0,enemyPrefabs.Length);
            Vector3 spawnPos = new Vector3(Random.Range(-spawnRangeX,spawnRangeX), spawnY, 0);
            Instantiate(enemyPrefabs[enemyIndex], spawnPos, enemyPrefabs[enemyIndex].transform.rotation);
        }
        
        
        
    }
}
