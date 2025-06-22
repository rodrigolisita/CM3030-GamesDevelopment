using UnityEngine;

public class MoveHorizontal2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float minSpeed = -0.10f;
    [SerializeField] float maxSpeed = 0.10f;

    [Header("Rotation Settings")]
    [Tooltip("How much the enemy tilts when moving. In degrees.")]
    [SerializeField] float yTurnAngle = 25.0f;
    [Tooltip("How quickly the enemy tilts. Higher is faster.")]
    [SerializeField] float turnSpeed = 5f;

    [Header("Change Behavior")]
    [Tooltip("Time in seconds to wait before changing movement. Set to 0 to never change.")]
    [SerializeField] float changeIntervalSeconds = 5.0f;

    private float currentSpeed;
    private float timer;

    // Start is called once before the first execution of Update
    void Start()
    {
        // Set the initial movement when the game starts.
        ChangeMovement();
    }

    // Update is called once per frame
    void Update()
    {

        // Rotation Logic
        HandleRotation();

        // This part handles the movement every frame.
        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);

        // This part handles the logic for changing the movement over time.
        HandleMovementChange();
    }

    void ChangeMovement()
    {
        currentSpeed = Random.Range(minSpeed, maxSpeed);
    }

    void HandleMovementChange()
    {
        // If the interval is 0 or less, we never change movement.
        if (changeIntervalSeconds <= 0)
        {
            return;
        }

        // Count down the timer.
        timer -= Time.deltaTime;

        // If the timer has run out, it's time to change.
        if (timer <= 0)
        {
            // Assign a new movement direction and speed.
            ChangeMovement();

            // Reset the timer back to the full interval.
            timer = changeIntervalSeconds;
        }
    }

    // handling rotation
    void HandleRotation()
    {
                                                                                       
                
    }
}