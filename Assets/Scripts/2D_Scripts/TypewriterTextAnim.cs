using UnityEngine;
using TMPro;

public class TypewriterTextAnim : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMesh;

    [SerializeField] float charsPerSecond;

    float startTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time;
        textMesh.maxVisibleCharacters = 0;
    }

    // Update is called once per frame
    void Update()
    {
        textMesh.maxVisibleCharacters = Mathf.RoundToInt(charsPerSecond * (Time.time - startTime));
    }

    public void RestartAnimation()
    {
        startTime = Time.time;
        textMesh.maxVisibleCharacters = 0;
    }
}
