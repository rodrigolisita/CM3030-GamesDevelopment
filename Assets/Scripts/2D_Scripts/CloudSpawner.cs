using UnityEngine;

/// <summary>
/// Spawns clouds just above the camera view at random X positions.
/// No pooling and no Rigidbody are used for simplicity.
/// - Timed spawning with jitter
/// - Random scale and slight initial Z rotation
/// - Assigns CloudMover speed and bottom despawn bound
/// Requires an orthographic Main Camera.
/// </summary>
public class CloudSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] cloudPrefabs;     // Put 2¨C5 different cloud prefabs here

    [Header("Spawn Timing")]
    public float spawnInterval = 2.0f;    // Average seconds between spawns
    public Vector2 spawnIntervalJitter = new Vector2(-0.6f, 0.6f);

    [Header("Movement")]
    public float baseSpeed = 0.8f;        // Base downward speed (units/sec)
    public Vector2 speedVariance = new Vector2(0.9f, 1.1f); // Multiplier range

    [Header("Look")]
    public Vector2 scaleRange = new Vector2(1.2f, 2.2f);    // Random scale per cloud
    public bool randomRotationOnSpawn = true;
    public Vector2 zRotationRange = new Vector2(-5f, 5f);   // Degrees

    [Header("Bounds")]
    public float topOffset = 0.8f;        // Spawn this far above the camera top
    public float recycleMargin = 1.5f;    // Despawn this far below the camera bottom
    public float edgePadding = 0.4f;      // Keep spawn away from left/right edges

    Camera cam;
    float halfW, halfH, minX, maxX, topY, bottomY;
    float nextSpawnAt;

    void Awake()
    {
        cam = Camera.main;
        if (cam == null || !cam.orthographic)
            Debug.LogWarning("[CloudSpawner] Requires an orthographic Main Camera.");

        RecalcBounds();
        ScheduleNextSpawn();
    }

    void RecalcBounds()
    {
        halfH = cam.orthographicSize;
        halfW = halfH * cam.aspect;

        Vector3 c = cam.transform.position;
        minX = c.x - halfW + edgePadding;
        maxX = c.x + halfW - edgePadding;
        topY = c.y + halfH + topOffset;
        bottomY = c.y - halfH - recycleMargin;
    }

    void ScheduleNextSpawn()
    {
        float jitter = Random.Range(spawnIntervalJitter.x, spawnIntervalJitter.y);
        nextSpawnAt = Time.time + Mathf.Max(0.05f, spawnInterval + jitter);
    }

    void Update()
    {
        RecalcBounds();

        if (Time.time >= nextSpawnAt)
        {
            SpawnOne();
            ScheduleNextSpawn();
        }
    }

    void SpawnOne()
    {
        if (cloudPrefabs == null || cloudPrefabs.Length == 0)
        {
            Debug.LogError("[CloudSpawner] Assign cloudPrefabs in the Inspector.");
            return;
        }

        // Position: random X above the top of the screen
        float x = Random.Range(minX, maxX);
        var prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        //var go = Instantiate(prefab, new Vector3(x, topY, 0f), Quaternion.identity);
        var go = Instantiate(prefab, new Vector3(x, topY, 0f), Quaternion.identity, transform);

        // Appearance: random scale + slight initial Z rotation
        float s = Random.Range(scaleRange.x, scaleRange.y);
        go.transform.localScale = prefab.transform.localScale * s;

        if (randomRotationOnSpawn)
        {
            float z = Random.Range(zRotationRange.x, zRotationRange.y);
            go.transform.rotation = Quaternion.Euler(0f, 0f, z);
        }

        // Ensure CloudMover exists and set movement params
        var mover = go.GetComponent<CloudMover>();
        if (mover == null) mover = go.AddComponent<CloudMover>();
        mover.verticalSpeed = baseSpeed * Random.Range(speedVariance.x, speedVariance.y);
        mover.despawnY = bottomY;
    }

    // Visualize the camera bounds while selected (for tuning)
    void OnDrawGizmosSelected()
    {
        if (Camera.main == null) return;
        var c = Camera.main;
        float hh = c.orthographicSize;
        float hw = hh * c.aspect;
        Vector3 center = c.transform.position;

        Gizmos.color = new Color(0.6f, 0.8f, 1f, 0.25f);
        Gizmos.DrawWireCube(center, new Vector3(hw * 2, hh * 2, 0.1f));
    }
}
