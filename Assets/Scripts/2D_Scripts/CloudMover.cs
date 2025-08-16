using UnityEngine;

/// <summary>
/// Per-cloud movement: vertical fall + gentle horizontal sway (+ optional slow rotation).
/// No Rigidbody required; destroys itself after falling below a given Y.
/// </summary>
public class CloudMover : MonoBehaviour
{
    [Header("Motion")]
    public float verticalSpeed = 0.8f;   // Downward speed in units/sec
    public float swayAmplitude = 0.5f;   // Horizontal sway amplitude in units
    public float swayFrequency = 0.25f;  // Horizontal sway frequency in Hz

    [Header("Slow Rotation")]
    public bool slowRotate = true;
    public float rotateSpeedDeg = 2f;    // Degrees per second

    [HideInInspector] public float despawnY = -10f; // Set by the spawner at runtime

    float _x0;     // Initial X (sway center)
    float _phase;  // Random phase so clouds don't sway in sync

    void OnEnable()
    {
        _x0 = transform.position.x;
        _phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        // Vertical fall
        Vector3 p = transform.position;
        p.y -= verticalSpeed * Time.deltaTime;

        // Horizontal sinusoidal sway around initial X
        float dx = swayAmplitude * Mathf.Sin((Time.time + _phase) * Mathf.PI * 2f * swayFrequency);
        p.x = _x0 + dx;
        transform.position = p;

        // Optional ultra-slow rotation
        if (slowRotate)
            transform.Rotate(0f, 0f, rotateSpeedDeg * Time.deltaTime);

        // Despawn below the bottom bound
        if (transform.position.y < despawnY)
            Destroy(gameObject);
    }
}
