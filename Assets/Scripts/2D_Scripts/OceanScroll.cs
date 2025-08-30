using UnityEngine;

public class OceanScroll : MonoBehaviour
{
    public float scrollSpeed = 0.1f; 
    private Material mat;
    private Vector2 offset;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        offset.y += scrollSpeed * Time.deltaTime;
        mat.mainTextureOffset = offset;
    }

    public void UpdateMaterial(Material newMaterial)
    {
        // Get the Renderer component on this object.
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Assign the new material to the renderer.
            renderer.material = newMaterial;
            // Update our internal reference to the new material.
            mat = renderer.material;
            // Reset the scroll offset to prevent a visual jump.
            offset = Vector2.zero;
        }
    }          
    
}
