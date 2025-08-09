using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns random island prefabs above the camera and scrolls them downward.
/// Includes a lightweight object pool, parallax-like speed variance,
/// optional horizontal drift, and spawn interval jitter.
/// Works with an Orthographic camera.
/// </summary>
public class IslandSpawner : MonoBehaviour
{
    [Header("Prefabs & Pool")]
    public GameObject[] islandPrefabs;      // Assign your island prefabs here
    public int initialPoolSize = 12;        // Pre-warmed pool size

    [Header("Spawn Timing")]
    public float spawnInterval = 1.75f;     // Average time between spawns (sec)
    public Vector2 spawnIntervalJitter = new Vector2(-0.6f, 0.6f); // Random offset added to interval

    [Header("Movement")]
    public float baseSpeed = 2.2f;          // Base downward speed (units/sec)
    public Vector2 speedVariance = new Vector2(0.9f, 1.15f); // Multiplies baseSpeed
    public float horizontalDrift = 0.25f;   // Sideways drift (units/sec). Set 0 for none.

    [Header("Look")]
    public Vector2 scaleRange = new Vector2(0.8f, 1.4f); // Random scale per island
    public bool randomRotation = true;                   // Subtle Z rotation
    public Vector2 rotationRange = new Vector2(-7f, 7f); // Degrees

    [Header("Bounds")]
    public float topOffset = 0.6f;          // Spawn this far above the top of camera
    public float recycleMargin = 2.5f;      // Recycle when below bottom by this margin
    public float edgePadding = 0.3f;        // Keep spawn away from screen edges

    Camera cam;
    float halfWidth, halfHeight;
    float topY, bottomY, minX, maxX;
    float nextSpawnAt;

    class Item
    {
        public GameObject go;
        public Rigidbody2D rb;
        public float vx;     // cached horizontal velocity
        public float vy;     // cached vertical velocity
    }

    readonly List<Item> actives = new List<Item>();
    readonly Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        cam = Camera.main;
        if (cam == null || !cam.orthographic)
            Debug.LogWarning("[IslandSpawner] Requires an Orthographic Main Camera.");

        // Warm up pool
        for (int i = 0; i < initialPoolSize; i++)
            pool.Enqueue(CreateOne());

        RecalcBounds();
        ScheduleNextSpawn();
    }

    void RecalcBounds()
    {
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        Vector3 c = cam.transform.position;
        minX = c.x - halfWidth + edgePadding;
        maxX = c.x + halfWidth - edgePadding;
        topY = c.y + halfHeight + topOffset;
        bottomY = c.y - halfHeight - recycleMargin;
    }

    GameObject CreateOne()
    {
        if (islandPrefabs == null || islandPrefabs.Length == 0)
        {
            Debug.LogError("[IslandSpawner] Assign islandPrefabs in Inspector.");
            return null;
        }

        var prefab = islandPrefabs[Random.Range(0, islandPrefabs.Length)];
        var go = Instantiate(prefab);
        go.SetActive(false);

        // Ensure there's a Rigidbody2D for velocity-based scrolling
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        rb.bodyType = RigidbodyType2D.Dynamic;

        return go;
    }

    void ScheduleNextSpawn()
    {
        float jitter = Random.Range(spawnIntervalJitter.x, spawnIntervalJitter.y);
        nextSpawnAt = Time.time + Mathf.Max(0.05f, spawnInterval + jitter);
    }

    void SpawnOne()
    {
        GameObject go = pool.Count > 0 ? pool.Dequeue() : CreateOne();
        if (go == null) return;

        // Random position across visible width
        float x = Random.Range(minX, maxX);
        go.transform.position = new Vector3(x, topY, 0f);

        // Random scale & rotation
        float s = Random.Range(scaleRange.x, scaleRange.y);
        //go.transform.localScale = new Vector3(s, s, 1f);

        if (randomRotation)
        {
            float z = Random.Range(rotationRange.x, rotationRange.y);
            go.transform.rotation = Quaternion.Euler(0, 0, z);
        }
        else go.transform.rotation = Quaternion.identity;

        // Randomized vertical speed (downwards) + horizontal drift
        float speedMult = Random.Range(speedVariance.x, speedVariance.y);
        float vy = -baseSpeed * speedMult;
        float vx = (horizontalDrift == 0f)
            ? 0f
            : Random.Range(-horizontalDrift, horizontalDrift);

        var rb = go.GetComponent<Rigidbody2D>();
        SetVelocity(rb, new Vector2(vx, vy));

        go.SetActive(true);
        actives.Add(new Item { go = go, rb = rb, vx = vx, vy = vy });
    }

    // Use linearVelocity if available (newer Unity), fallback to velocity for older versions.
    static void SetVelocity(Rigidbody2D rb, Vector2 v)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = v;
#else
        rb.velocity = v;
#endif
    }

    void Update()
    {
        // Recompute bounds in case the camera moved or resized
        RecalcBounds();

        // Spawn
        if (Time.time >= nextSpawnAt)
        {
            SpawnOne();
            ScheduleNextSpawn();
        }

        // Recycle when off-screen
        for (int i = actives.Count - 1; i >= 0; i--)
        {
            var it = actives[i];
            if (it.go == null) { actives.RemoveAt(i); continue; }

            if (it.go.transform.position.y < bottomY)
            {
                it.go.SetActive(false);
                // stop velocity before pooling (optional)
                SetVelocity(it.rb, Vector2.zero);
                actives.RemoveAt(i);
                pool.Enqueue(it.go);
            }
        }
    }

    // Optional: visualize the camera bounds while selected
    void OnDrawGizmosSelected()
    {
        if (Camera.main == null) return;
        var c = Camera.main;
        float hh = c.orthographicSize;
        float hw = hh * c.aspect;
        Vector3 center = c.transform.position;

        Gizmos.color = new Color(1, 1, 0, 0.25f);
        Gizmos.DrawWireCube(center, new Vector3(hw * 2, hh * 2, 0.1f));
    }
}
