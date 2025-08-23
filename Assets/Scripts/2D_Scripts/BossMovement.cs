using UnityEngine;

public class BossMovement : MonoBehaviour
{
    
    public float verticalPositionPercentage = 80;
    private float minY;
    public float verticalSpeed = 0.5f;
    public float paddingPercent = 25f;
    private float minX;
    private float maxX;
    public float horizontalSpeed = 0.3f;
    public float changeDirectionDelayMin = 3f;
    public float changeDirectionDelayMax = 6f;
    private float nextSwapTime;
    private Vector3 horizontalVector = new Vector3(1, 0, 0);

    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        minY = (BoundaryManager.Instance.MaxY - BoundaryManager.Instance.MinY) * verticalPositionPercentage * 0.01f + BoundaryManager.Instance.MinY;
        minX = ((BoundaryManager.Instance.MaxX - BoundaryManager.Instance.MinX) * paddingPercent * 0.01f) + BoundaryManager.Instance.MinX;
        maxX = ((BoundaryManager.Instance.MaxX - BoundaryManager.Instance.MinX) * (100 - paddingPercent) * 0.01f) + BoundaryManager.Instance.MinX;
        if (Random.Range(0, 1) > 0.5)
        {
            swapDirections();
        }
        swapDirections();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y > minY)
        {
            transform.Translate(new Vector3(0, -1, 0) * Time.deltaTime * verticalSpeed, Space.World);
        }
        else if (transform.position.y < minY)
        {
            transform.position.Set(transform.position.x, minY, transform.position.z);
        }

        transform.Translate(horizontalVector * Time.deltaTime * horizontalSpeed, Space.World);

        if (transform.position.x > maxX && horizontalVector.x > 0)
        {
            swapDirections();
        } else if (transform.position.x < minX && horizontalVector.x < 0)
        {
            swapDirections();
        } else if (Time.time >= nextSwapTime)
        {
            swapDirections();
        }
    }

    public void swapDirections()
    {
        horizontalVector = horizontalVector * -1;
        nextSwapTime = Time.time + Random.Range(changeDirectionDelayMin, changeDirectionDelayMax);
    }
}
