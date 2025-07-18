using UnityEngine;

public class BackgroundTestController : MonoBehaviour
{
    [Header("Background Control")]
    public ParallaxManager parallaxManager;
    
    [Header("Test Controls")]
    public KeyCode toggleHideStaticKey = KeyCode.H;
    public KeyCode resetBackgroundsKey = KeyCode.R;
    
    void Start()
    {
        // Auto-find ParallaxManager if not assigned
        if (parallaxManager == null)
        {
            parallaxManager = FindObjectOfType<ParallaxManager>();
        }
        
        if (parallaxManager == null)
        {
            Debug.LogError("BackgroundTestController: No ParallaxManager found in scene!");
        }
    }
    
    void Update()
    {
        if (parallaxManager == null) return;
        
        // Toggle hide static backgrounds
        if (Input.GetKeyDown(toggleHideStaticKey))
        {
            bool currentState = !parallaxManager.hideStaticBackgrounds;
            parallaxManager.SetAllLayersHideStaticBackgrounds(currentState);
            Debug.Log($"Toggled hide static backgrounds: {currentState}");
        }
        
        // Reset all backgrounds
        if (Input.GetKeyDown(resetBackgroundsKey))
        {
            parallaxManager.ResetAllLayers();
            Debug.Log("Reset all background layers");
        }
    }
    
    void OnGUI()
    {
        if (parallaxManager == null) return;
        
        // Display controls info
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label("Background Controls:");
        GUILayout.Label($"Press {toggleHideStaticKey} to toggle hide static backgrounds");
        GUILayout.Label($"Press {resetBackgroundsKey} to reset all backgrounds");
        GUILayout.Label($"Hide Static: {parallaxManager.hideStaticBackgrounds}");
        GUILayout.EndArea();
    }
} 