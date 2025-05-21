using UnityEngine;

public class MoveForward2D : MonoBehaviour
{
    public float speed = 40.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0, 1, 0) * Time.deltaTime * speed);
        
    }
}
