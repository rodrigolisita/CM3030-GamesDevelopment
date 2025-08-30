using UnityEngine;
using System.Collections.Generic;

public class ParallaxManager : MonoBehaviour
{
    [Header("Parallax Background Layers")]
    public TimeBasedParallax[] timeBasedLayers;       // Time-based parallax layers
    
    [Header("Parallax Effect Settings")]
    [Range(0f, 1f)]
    public float[] parallaxEffects = { 0.3f, 0.5f, 0.8f }; // Parallax strength for each layer (Cloud, Mountain, Ground)
    
    [Header("Game State Control")]
    public bool pauseOnGameOver = true;               // Pause when game over
    public bool adjustSpeedForDifficulty = true;      // Adjust speed based on difficulty
    public bool hideStaticBackgrounds = true;         // Hide static backgrounds at the top

    [Header("Debug Info")]
    public bool showDebugInfo = false;                // Show debug information
    
    private GameManager2D gameManager;
    private List<TimeBasedParallax> allTimeLayers;
    
    void Start()
    {
        InitializeParallaxManager();
    }
    
    public void InitializeParallaxManager()
    {
        // Get game manager
        gameManager = GameManager2D.Instance;
        
        // Initialize list
        allTimeLayers = new List<TimeBasedParallax>();
        
        // Auto-find parallax backgrounds in scene
        if (timeBasedLayers == null || timeBasedLayers.Length == 0)
        {
            timeBasedLayers = FindObjectsOfType<TimeBasedParallax>();
        }
        
        // Add to list and set parallax effects
        SetupParallaxLayers();
        
        if (showDebugInfo)
        {
            Debug.Log($"ParallaxManager initialization complete: time layers={allTimeLayers.Count}");
        }
    }
    
    void SetupParallaxLayers()
    {
        // Setup time-based parallax layers
        for (int i = 0; i < timeBasedLayers.Length && i < parallaxEffects.Length; i++)
        {
            if (timeBasedLayers[i] != null)
            {
                allTimeLayers.Add(timeBasedLayers[i]);
                timeBasedLayers[i].SetParallaxMultiplier(parallaxEffects[i]);
                timeBasedLayers[i].showDebugInfo = showDebugInfo;
                timeBasedLayers[i].pauseOnGameOver = pauseOnGameOver;
                timeBasedLayers[i].SetHideStaticBackgrounds(hideStaticBackgrounds);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Setup time parallax layer: {timeBasedLayers[i].name}, multiplier: {parallaxEffects[i]}");
                }
            }
        }
    }
    
    void Update()
    {
        // Control all parallax layers based on game state
        if (pauseOnGameOver && gameManager != null)
        {
            bool shouldPause = !(GameManager2D.Instance.gameState == GameState.Active);
            SetAllLayersPaused(shouldPause);
        }
    }
    
    // Set pause state for all layers
    public void SetAllLayersPaused(bool paused)
    {
        // Pause time-based layers
        foreach (TimeBasedParallax layer in allTimeLayers)
        {
            if (layer != null)
            {
                layer.SetPaused(paused);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"All parallax layers pause state: {paused}");
        }
    }
    
    // Adjust speed for all layers based on difficulty
    public void AdjustAllLayersForDifficulty(int difficultyLevel)
    {
        if (!adjustSpeedForDifficulty) return;
        
        // Adjust time-based layer speeds
        foreach (TimeBasedParallax layer in allTimeLayers)
        {
            if (layer != null)
            {
                layer.AdjustSpeedForDifficulty(difficultyLevel);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Adjust all parallax layer speeds based on difficulty {difficultyLevel}");
        }
    }
    
    // Create depth parallax effect
    public void CreateDepthParallaxEffect()
    {
        for (int i = 0; i < allTimeLayers.Count; i++)
        {
            if (allTimeLayers[i] != null)
            {
                // Farther background layers move slower
                float depthEffect = 1f - (i * 0.2f);
                float parallaxMultiplier = parallaxEffects[Mathf.Min(i, parallaxEffects.Length - 1)] * depthEffect;
                allTimeLayers[i].SetParallaxMultiplier(parallaxMultiplier);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("Depth parallax effect creation complete");
        }
    }
    
    // Reset all layers to initial position
    public void ResetAllLayers()
    {
        CleanupBackgroundCopies();
        foreach (TimeBasedParallax layer in allTimeLayers)
        {
            if (layer != null)
            {
                layer.ResetToStartPosition();
            }
        }
        if (showDebugInfo)
        {
            Debug.Log("Reset all parallax layers to initial position");
        }
    }
    
    // Set debug info for all layers
    public void SetAllLayersDebug(bool debug)
    {
        showDebugInfo = debug;
        
        foreach (TimeBasedParallax layer in allTimeLayers)
        {
            if (layer != null)
            {
                layer.showDebugInfo = debug;
            }
        }
    }
    
    // Set hide static backgrounds for all layers
    public void SetAllLayersHideStaticBackgrounds(bool hide)
    {
        hideStaticBackgrounds = hide;
        
        foreach (TimeBasedParallax layer in allTimeLayers)
        {
            if (layer != null)
            {
                layer.SetHideStaticBackgrounds(hide);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"All parallax layers hide static backgrounds: {hide}");
        }
    }
    
    // Get parallax layer information
    public string GetParallaxInfo()
    {
        string info = $"Parallax Manager Info:\n";
        info += $"Time layers count: {allTimeLayers.Count}\n";
        info += $"Pause state: {pauseOnGameOver}\n";
        info += $"Difficulty adjustment: {adjustSpeedForDifficulty}\n";
        info += $"Hide static backgrounds: {hideStaticBackgrounds}\n";
        
        return info;
    }
    
    // Show debug info in editor
    void OnDrawGizmosSelected()
    {
        if (showDebugInfo)
        {
            // Draw positions of all parallax layers
            Gizmos.color = Color.blue;
            
            foreach (TimeBasedParallax layer in allTimeLayers)
            {
                if (layer != null)
                {
                    Gizmos.DrawWireSphere(layer.transform.position, 0.3f);
                }
            }
        }
    }
    
    // 清理所有背景副本对象
    public void CleanupBackgroundCopies()
    {
        var backgrounds = GameObject.Find("Backgrounds");
        if (backgrounds != null)
        {
            // 用临时列表避免遍历时修改集合
            var toDelete = new List<Transform>();
            foreach (Transform child in backgrounds.transform)
            {
                if (child.name.Contains("_Copy_"))
                {
                    toDelete.Add(child);
                }
            }
            foreach (var t in toDelete)
            {
                Destroy(t.gameObject);
            }
        }
    }
} 