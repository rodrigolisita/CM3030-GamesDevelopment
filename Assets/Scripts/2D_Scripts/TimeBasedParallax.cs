using UnityEngine;

public class TimeBasedParallax : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;          // Base movement speed
    [Range(0f, 1f)]
    public float parallaxMultiplier = 0.5f; // Parallax multiplier
    
    [Header("Movement Direction")]
    public bool moveHorizontally = false; // Horizontal movement
    public bool moveVertically = true;    // Vertical movement
    
    [Header("Loop Settings")]
    public bool loopBackground = true;    // Whether to loop background
    public int backgroundCopies = 2;      // Number of background copies
    
    [Header("Game State Control")]
    public bool pauseOnGameOver = true;   // Pause when game over
    
    [Header("Debug Info")]
    public bool showDebugInfo = false;    // Show debug information
    
    private Vector3 startPosition;
    private float backgroundWidth;
    private float backgroundHeight;
    private SpriteRenderer spriteRenderer;
    private GameManager2D gameManager;
    private bool isPaused = false;
    
    void Start()
    {
        InitializeTimeBasedParallax();
    }
    
    void InitializeTimeBasedParallax()
    {
        // Get game manager
        gameManager = GameManager2D.Instance;
        
        // Record initial position
        startPosition = transform.position;
        
        // Get SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("TimeBasedParallax: SpriteRenderer component not found!", this);
            return;
        }
        
        // Calculate background size
        CalculateBackgroundSize();
        
        // Create background copies (if looping is needed)
        if (loopBackground)
        {
            CreateBackgroundCopies();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"TimeBasedParallax initialization complete: {gameObject.name}, movement speed: {moveSpeed * parallaxMultiplier}");
        }
    }
    
    void CalculateBackgroundSize()
    {
        backgroundWidth = spriteRenderer.bounds.size.x;
        backgroundHeight = spriteRenderer.bounds.size.y;
        
        if (showDebugInfo)
        {
            Debug.Log($"Background size: width={backgroundWidth}, height={backgroundHeight}");
        }
    }
    
    void CreateBackgroundCopies()
    {
        for (int i = 1; i < backgroundCopies; i++)
        {
            Vector3 offset = Vector3.zero;
            
            if (moveVertically)
            {
                offset = Vector3.up * backgroundHeight * i;
            }
            else if (moveHorizontally)
            {
                offset = Vector3.right * backgroundWidth * i;
            }
            
            GameObject backgroundCopy = Instantiate(gameObject, 
                startPosition + offset, 
                transform.rotation, 
                transform.parent);
            
            // Rename copy
            backgroundCopy.name = $"{gameObject.name}_Copy_{i}";
            
            // Remove TimeBasedParallax script from copy to avoid duplicate creation
            Destroy(backgroundCopy.GetComponent<TimeBasedParallax>());
            
            if (showDebugInfo)
            {
                Debug.Log($"Created background copy: {backgroundCopy.name} position: {backgroundCopy.transform.position}");
            }
        }
    }
    
    void Update()
    {
        // Check if pause is needed
        if (pauseOnGameOver && gameManager != null)
        {
            isPaused = !gameManager.isGameActive;
        }
        
        if (isPaused) return;
        
        // Calculate movement distance
        float actualSpeed = moveSpeed * parallaxMultiplier;
        float moveDistance = actualSpeed * Time.deltaTime;
        
        // Apply movement
        ApplyMovement(moveDistance);
        
        // Handle looping
        if (loopBackground)
        {
            HandleLooping();
        }
    }
    
    void ApplyMovement(float moveDistance)
    {
        Vector3 newPosition = transform.position;
        
        // Vertical movement (move down, simulate flying up)
        if (moveVertically)
        {
            newPosition.y -= moveDistance;
        }
        
        // Horizontal movement (move left, simulate flying right)
        if (moveHorizontally)
        {
            newPosition.x -= moveDistance;
        }
        
        transform.position = newPosition;
        
        if (showDebugInfo && Time.frameCount % 60 == 0) // Output once every 60 frames to avoid too many logs
        {
            Debug.Log($"Background movement: {gameObject.name}, position: {newPosition}");
        }
    }
    
    void HandleLooping()
    {
        // Vertical looping
        if (moveVertically)
        {
            if (transform.position.y <= startPosition.y - backgroundHeight)
            {
                transform.position = new Vector3(transform.position.x, startPosition.y, transform.position.z);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Vertical loop reset: {gameObject.name}");
                }
            }
        }
        
        // Horizontal looping
        if (moveHorizontally)
        {
            if (transform.position.x <= startPosition.x - backgroundWidth)
            {
                transform.position = new Vector3(startPosition.x, transform.position.y, transform.position.z);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Horizontal loop reset: {gameObject.name}");
                }
            }
        }
    }
    
    // Public method: Set movement speed
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
        if (showDebugInfo)
        {
            Debug.Log($"Movement speed updated: {gameObject.name}, new speed: {moveSpeed * parallaxMultiplier}");
        }
    }
    
    // Public method: Set parallax multiplier
    public void SetParallaxMultiplier(float multiplier)
    {
        parallaxMultiplier = Mathf.Clamp01(multiplier);
        if (showDebugInfo)
        {
            Debug.Log($"Parallax multiplier updated: {gameObject.name}, new multiplier: {parallaxMultiplier}");
        }
    }
    
    // Public method: Pause/Resume movement
    public void SetPaused(bool paused)
    {
        isPaused = paused;
        if (showDebugInfo)
        {
            Debug.Log($"Movement state updated: {gameObject.name}, paused: {paused}");
        }
    }
    
    // Public method: Adjust speed based on difficulty
    public void AdjustSpeedForDifficulty(int difficultyLevel)
    {
        float speedMultiplier = 1f;
        
        switch (difficultyLevel)
        {
            case 1: // Easy
                speedMultiplier = 0.8f;
                break;
            case 2: // Medium
                speedMultiplier = 1.2f;
                break;
            case 3: // Hard
                speedMultiplier = 1.5f;
                break;
        }
        
        SetMoveSpeed(moveSpeed * speedMultiplier);
    }
    
    // Public method: Reset to initial position
    public void ResetToStartPosition()
    {
        transform.position = startPosition;
        if (showDebugInfo)
        {
            Debug.Log($"Reset to initial position: {gameObject.name}");
        }
    }
    
    // Displaying debugging information in the editor
    void OnDrawGizmosSelected()
    {
        if (showDebugInfo && spriteRenderer != null)
        {
            // Drawing background boundaries
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, spriteRenderer.bounds.size);
            
            // Drawing the direction of movement
            Gizmos.color = Color.green;
            if (moveHorizontally)
            {
                Gizmos.DrawRay(transform.position, Vector3.left * 3f);
            }
            if (moveVertically)
            {
                Gizmos.DrawRay(transform.position, Vector3.down * 3f);
            }
            
            // Drawing Loop Boundaries
            if (loopBackground)
            {
                Gizmos.color = Color.yellow;
                Vector3 loopPosition = startPosition;
                if (moveVertically)
                {
                    loopPosition.y -= backgroundHeight;
                }
                else if (moveHorizontally)
                {
                    loopPosition.x -= backgroundWidth;
                }
                Gizmos.DrawWireCube(loopPosition, spriteRenderer.bounds.size);
            }
        }
    }
} 