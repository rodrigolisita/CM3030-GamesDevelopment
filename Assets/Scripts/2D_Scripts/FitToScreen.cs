using UnityEngine;

// This script should be attached to your main background Quad or Plane.
public class FitToScreen : MonoBehaviour
{
    void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Get the world dimensions of the orthographic camera view.
        float cameraHeight = mainCamera.orthographicSize * 3.5f;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Set the scale of this background object to match the camera's dimensions.
        // A standard Quad in Unity is 1x1 unit, so this works perfectly.
        transform.localScale = new Vector3(cameraWidth, cameraHeight, 1);
        
        Debug.Log("Background Quad has been scaled to fit the screen.");
    }
}