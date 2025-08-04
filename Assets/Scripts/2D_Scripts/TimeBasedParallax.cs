using UnityEngine;
using System.Collections.Generic; // Added for List

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
    public bool hideStaticBackgrounds = true; // Hide backgrounds that appear static at the top
    
    [Header("Background Size Settings")]
    [Tooltip("背景高度（单位：Unity单位）。如果设为0，将自动计算；否则使用此值")]
    public float manualBackgroundHeight = 0f; // Manual background height setting
    [Tooltip("背景宽度（单位：Unity单位）。如果设为0，将自动计算；否则使用此值")]
    public float manualBackgroundWidth = 0f;  // Manual background width setting
    
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
    private List<GameObject> backgroundCopiesList; // Track all background copies
    
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
        // 如果手动设置了尺寸，使用手动设置的值；否则自动计算
        if (manualBackgroundWidth > 0f)
        {
            backgroundWidth = manualBackgroundWidth;
            if (showDebugInfo)
            {
                Debug.Log($"使用手动设置的背景宽度: {backgroundWidth}");
            }
        }
        else
        {
            backgroundWidth = spriteRenderer.bounds.size.x;
            if (showDebugInfo)
            {
                Debug.Log($"自动计算的背景宽度: {backgroundWidth}");
            }
        }
        
        if (manualBackgroundHeight > 0f)
        {
            backgroundHeight = manualBackgroundHeight;
            if (showDebugInfo)
            {
                Debug.Log($"使用手动设置的背景高度: {backgroundHeight}");
            }
        }
        else
        {
            backgroundHeight = spriteRenderer.bounds.size.y;
            if (showDebugInfo)
            {
                Debug.Log($"自动计算的背景高度: {backgroundHeight}");
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"=== 背景尺寸信息 ===");
            Debug.Log($"最终背景宽度: {backgroundWidth}");
            Debug.Log($"最终背景高度: {backgroundHeight}");
            Debug.Log($"起始位置: {startPosition}");
            Debug.Log($"循环结束位置: {startPosition.y - backgroundHeight}");
            Debug.Log($"隐藏阈值位置: {startPosition.y + backgroundHeight * 0.5f}");
        }
    }
    
    void CreateBackgroundCopies()
    {
        backgroundCopiesList = new List<GameObject>(); // Initialize the list
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
            backgroundCopiesList.Add(backgroundCopy); // Add to the list
        }
    }
    
    void Update()
    {
        // Check if pause is needed
        if (pauseOnGameOver && gameManager != null)
        {
            isPaused = !(GameManager2D.Instance.gameState == GameState.Active);
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
            
            // Hide backgrounds that are too high up (appear static)
            if (hideStaticBackgrounds)
            {
                float hideThreshold = startPosition.y + backgroundHeight * 0.5f; // Hide when above this threshold
                if (transform.position.y > hideThreshold)
                {
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = false;
                    }
                }
                else
                {
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = true;
                    }
                }
                
                // Handle background copies visibility
                if (backgroundCopiesList != null)
                {
                    foreach (GameObject copy in backgroundCopiesList)
                    {
                        if (copy != null)
                        {
                            SpriteRenderer copyRenderer = copy.GetComponent<SpriteRenderer>();
                            if (copyRenderer != null)
                            {
                                if (copy.transform.position.y > hideThreshold)
                                {
                                    copyRenderer.enabled = false;
                                }
                                else
                                {
                                    copyRenderer.enabled = true;
                                }
                            }
                        }
                    }
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
        
        // Ensure visibility is restored when resetting
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        // Restore visibility for all copies
        if (backgroundCopiesList != null)
        {
            foreach (GameObject copy in backgroundCopiesList)
            {
                if (copy != null)
                {
                    SpriteRenderer copyRenderer = copy.GetComponent<SpriteRenderer>();
                    if (copyRenderer != null)
                    {
                        copyRenderer.enabled = true;
                    }
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Reset to initial position: {gameObject.name}");
        }
    }
    
    // Public method: Set hide static backgrounds
    public void SetHideStaticBackgrounds(bool hide)
    {
        hideStaticBackgrounds = hide;
        if (showDebugInfo)
        {
            Debug.Log($"Hide static backgrounds setting updated: {gameObject.name}, hide: {hide}");
        }
    }
    
    // Public method: Set manual background height
    public void SetManualBackgroundHeight(float height)
    {
        manualBackgroundHeight = height;
        CalculateBackgroundSize(); // 重新计算背景尺寸
        if (showDebugInfo)
        {
            Debug.Log($"Manual background height updated: {gameObject.name}, new height: {height}");
        }
    }
    
    // Public method: Set manual background width
    public void SetManualBackgroundWidth(float width)
    {
        manualBackgroundWidth = width;
        CalculateBackgroundSize(); // 重新计算背景尺寸
        if (showDebugInfo)
        {
            Debug.Log($"Manual background width updated: {gameObject.name}, new width: {width}");
        }
    }
    
    // Public method: Get current background dimensions
    public Vector2 GetBackgroundDimensions()
    {
        return new Vector2(backgroundWidth, backgroundHeight);
    }
    
    // Public method: Get loop end position
    public float GetLoopEndPosition()
    {
        return startPosition.y - backgroundHeight;
    }
    
    // Displaying debugging information in the editor
    void OnDrawGizmosSelected()
    {
        if (showDebugInfo && spriteRenderer != null)
        {
            // Drawing background boundaries
            Gizmos.color = Color.cyan;
            Vector3 currentSize = new Vector3(backgroundWidth, backgroundHeight, 0.1f);
            Gizmos.DrawWireCube(transform.position, currentSize);
            
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
                Gizmos.DrawWireCube(loopPosition, currentSize);
                
                // Draw hide threshold
                if (hideStaticBackgrounds && moveVertically)
                {
                    Gizmos.color = Color.red;
                    Vector3 hidePosition = startPosition;
                    hidePosition.y += backgroundHeight * 0.5f;
                    Gizmos.DrawWireCube(hidePosition, currentSize);
                }
            }
        }
    }
} 