using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyColorizer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // Get the SpriteRenderer component on this enemy.
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Public method to apply a new color tint to the sprite.
    /// </summary>
    public void ApplyColor(Color newColor)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = newColor;
        }
    }
}