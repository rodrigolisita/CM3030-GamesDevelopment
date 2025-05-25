using UnityEngine;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour
{
    private Button button;
    public float spawnIntervalForThisDifficulty = 1.5f; // Set this in Inspector for each button

    void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("DifficultyButton: Button component not found on this GameObject.", gameObject);
            return;
        }
        button.onClick.AddListener(OnButtonClick);

        if (GameManager2D.Instance == null)
        {
            Debug.LogError("DifficultyButton: GameManager2D.Instance is not available at Start. Ensure GameManager2D is in the scene and initialized before this button becomes active.", gameObject);
        }
    }

    void OnButtonClick()
    {
        Debug.Log(gameObject.name + " was clicked. Setting spawn interval to: " + spawnIntervalForThisDifficulty);
        if (GameManager2D.Instance != null)
        {
            GameManager2D.Instance.StartGame(spawnIntervalForThisDifficulty);
            
            // Optional: Deactivate the title screen or just this button after starting
            // This might be better handled by GameManager2D.UpdateAllUIDisplays()
            // if (transform.parent != null && transform.parent.name == GameManager2D.Instance.titleScreenName)
            // {
            //    transform.parent.gameObject.SetActive(false);
            // }
            // else
            // {
            //    gameObject.SetActive(false);
            // }
        }
        else
        {
            Debug.LogError("DifficultyButton: GameManager2D.Instance is null. Cannot start game.", gameObject);
        }
    }
}
