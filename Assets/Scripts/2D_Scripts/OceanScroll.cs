using UnityEngine;

public class OceanScroll : MonoBehaviour
{
    public float scrollSpeed = 0.1f; // 控制滚动速度
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
}
