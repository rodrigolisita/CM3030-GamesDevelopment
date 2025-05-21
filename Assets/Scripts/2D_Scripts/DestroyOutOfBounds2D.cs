using UnityEngine;

public class DestroyOutOfBounds2D : MonoBehaviour
{
    private double topBound = 3;
    private double lowerBound = -0.5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y > topBound){
            Destroy(gameObject);
        }
        else if(transform.position.y < lowerBound){
            Destroy(gameObject);
        }
        
    }
}
