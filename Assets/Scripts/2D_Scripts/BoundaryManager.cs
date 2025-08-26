using UnityEngine;

public class BoundaryManager : MonoBehaviour
{
    public static BoundaryManager Instance { get; private set; }

    // Public properties for other scripts to read
    public float MinX { get; private set; }
    public float MaxX { get; private set; }
    public float MinY { get; private set; }
    public float MaxY { get; private set; }
    
    // Separate properties for padded boundaries, used for spawning
    public float PaddedMinX { get; private set; }
    public float PaddedMaxX { get; private set; }
    public float PaddedMaxY { get; private set; }
    public float PaddedMinY { get; private set; }

    [Header("Settings")]
    [Tooltip("Padding from the screen edges in world units.")]
    [SerializeField] private float horizontalPadding = 0.5f;
    [SerializeField] private float verticalPadding = 2.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CalculateBoundaries();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CalculateBoundaries()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("BoundaryManager Error: No camera found with 'MainCamera' tag.");
            return;
        }

        // --- THIS IS THE CORRECTED LOGIC ---
        // 1. Calculate the raw, un-padded screen edges first.
        MinX = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        MaxX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        MinY = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        MaxY = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;

        // 2. Now, create the separate padded values for spawning/despawning.
        PaddedMinX = MinX + horizontalPadding;
        PaddedMaxX = MaxX - horizontalPadding;
        PaddedMinY = MinY - verticalPadding;
        PaddedMaxY = MaxY + verticalPadding;

        Debug.Log("Boundaries Calculated: Full X Range (" + MinX + " to " + MaxX + ")");
    }
}