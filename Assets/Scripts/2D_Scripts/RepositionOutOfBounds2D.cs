using UnityEngine;

public class RepositionOutOfBounds2D : MonoBehaviour
{
    private float minY;
    private float maxX;
    private float minX;

    void Start()
    {
        // Cache boundaries from BoundaryManager
        minY = BoundaryManager.Instance.MinY;
        maxX = BoundaryManager.Instance.MaxX;
        minX = BoundaryManager.Instance.MinX;
    }

    void Update()
    {
        if (transform.position.y < minY)
        {
            RepositionAboveScreen();
        }
    }

    private void RepositionAboveScreen()
    {
        float randomX = Random.Range(minX, maxX);
        float newY = BoundaryManager.Instance.PaddedMaxY;
        
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}