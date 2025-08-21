using UnityEngine;

public class DestroyOutOfBounds2D : MonoBehaviour
{
    void Update()
    {
        // Check if the BoundaryManager instance exists before using it
        if (BoundaryManager.Instance == null)
        {
            return;
        }

        // Destroy the object if it goes ABOVE the padded top boundary
        if (transform.position.y > BoundaryManager.Instance.PaddedMaxY)
        {
            Destroy(gameObject);
        }
        // Destroy the object if it goes BELOW the padded bottom boundary
        else if (transform.position.y < BoundaryManager.Instance.PaddedMinY)
        {
            Destroy(gameObject);
        }

        if(transform.position.x > BoundaryManager.Instance.MaxX || transform.position.x < BoundaryManager.Instance.MinX)
        {
            Destroy(gameObject);
        }
    }
}