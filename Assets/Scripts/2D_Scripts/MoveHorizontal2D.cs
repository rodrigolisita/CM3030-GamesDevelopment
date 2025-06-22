using UnityEngine;

public class MoveHorizontal2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float minSpeed = -0.10f;
    [SerializeField] float maxSpeed = 0.10f;

    [Header("Rotation Settings")]
    [Tooltip("The base angle of the enemy. Set to 180 if your sprite needs to face down.")]
    [SerializeField] float baseAngle = 180f;

    [Tooltip("How much the enemy tilts. Keep this value small (e.g., 10-20).")]
    [SerializeField] float tiltAngle = 0.0f;

    [Tooltip("How quickly the enemy tilts. Higher is faster.")]
    [SerializeField] float tiltSpeed = 3f;

    [Header("Change Behavior")]
    [Tooltip("Time in seconds to wait before changing movement. Set to 0 to never change.")]
    [SerializeField] float changeIntervalSeconds = 5.0f;

    private float currentSpeed;
    private float timer;

    // Start is called once before the first execution of Update
    void Start()
    {
        transform.eulerAngles = new Vector3(0, 0, baseAngle);
        // Set the initial movement when the game starts.
        ChangeMovement();
    }

    // Update is called once per frame
    void Update()
    {

        // Rotation Logic
        HandleRotation();

        // Handle the movement logic
        // Using Space.World ensures it always moves along the world's X-axis
        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime, Space.World);

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
        // 1. Determine the desired tilt based on speed
        float tilt = 0.0f;

        if (currentSpeed > 0.001f) // Using a small dead zone for speed
        {
            tilt = tiltAngle; 
        }
        else if (currentSpeed < -0.001f) // Moving left
        {
            tilt = -tiltAngle;
        }

        // 2. Calculate the final target angle (Base Direction + Tilt)
        float targetAngle = baseAngle + tilt;

        // Get the current angle and check if it's already very close to the target.
        float currentZ = transform.eulerAngles.z;
        
        // Mathf.DeltaAngle calculates the shortest difference between two angles.
        // If the difference is tiny (less than 0.1 degrees), we do nothing.
        // This prevents the constant "wobble" when the tilt is not changing.
        if (Mathf.Abs(Mathf.DeltaAngle(currentZ, targetAngle)) < 0.1f)
        {
            return; // Exit the function, no rotation needed.
        }

        // 3. If we are not close enough, then smoothly interpolate.
        float newZ = Mathf.LerpAngle(currentZ, targetAngle, tiltSpeed * Time.deltaTime);

        // 4. Apply the new rotation only to the Z-axis
        transform.eulerAngles = new Vector3(0, 0, newZ);
    }
}