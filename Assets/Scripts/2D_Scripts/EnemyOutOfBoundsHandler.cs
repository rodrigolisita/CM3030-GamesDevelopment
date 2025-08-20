using UnityEngine;

public class EnemyOutOfBoundsHandler : MonoBehaviour
{
    // Cached boundary values for efficiency
    private float bottomBoundY;
    private float topBoundY;
    private float leftBoundX;
    private float rightBoundX;

    // Extra padding to ensure the enemy is fully off-screen before destruction.   
    private float horizontalPadding = 1f;

    void Start()
    {
        // Get all the boundary values from the manager once at the start.
        if (BoundaryManager.Instance != null)
        {
            bottomBoundY = BoundaryManager.Instance.MinY - 2f;
            topBoundY = BoundaryManager.Instance.PaddedMaxY;
            leftBoundX = BoundaryManager.Instance.MinX;
            rightBoundX = BoundaryManager.Instance.MaxX;
            
        }
    }

    void Update()
    {
        // --- REPOSITION LOGIC ---
        // If the enemy goes off the bottom of the screen...
        if (transform.position.y < bottomBoundY)
        {
            // ...reposition it to the top at a new random X position.
            float randomX = Random.Range(leftBoundX, rightBoundX);
            transform.position = new Vector3(randomX, topBoundY, transform.position.z);
       
        }

        // --- DESTROY LOGIC ---
        // If the enemy goes off the left or right sides of the screen...
        if (transform.position.x < leftBoundX - horizontalPadding || transform.position.x > rightBoundX + horizontalPadding)
        {
            if (SpawnManager2D.Instance != null)
            {
                SpawnManager2D.Instance.OnEnemyDestroyed();
            }
            Destroy(gameObject);
        }

    }
}