using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float horizontalInput;
    public float speed = 10.0f;
    
    public float xRange = 24;

    public GameObject projectilePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Move player left and right
        horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * Time.deltaTime * speed);

        // Ensure the player stays within x bounds
        if(transform.position.x < -xRange){
            transform.position = new Vector3(-xRange, transform.position.y,transform.position.z);
        } 
        if(transform.position.x > xRange){
            transform.position = new Vector3(xRange, transform.position.y,transform.position.z);
        } 

        // Instantiate the projectile
        if(Input.GetKeyDown(KeyCode.Space))
        {
            // Launch a projectile from the player
            Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);
        }
        
    }
}
