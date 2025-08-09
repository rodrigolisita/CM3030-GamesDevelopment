using UnityEngine;

public class IslandMover : MonoBehaviour
{
    public float verticalSpeed = 2f;   // Downward speed

    void Update()
    {
        // Move downward every frame
        transform.Translate(Vector3.down * verticalSpeed * Time.deltaTime);

        // Destroy when out of view
        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }
}
