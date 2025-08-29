using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Skyfire/Campaign Content/Mission")]
public class Mission : ScriptableObject
{
    [SerializeField] private IntroScreen[] introScreens;
    [SerializeField] private int waves;
    [SerializeField] private GameObject bossPrefab;
    

    public IntroScreen GetIntro(int index)
    {
        if (index >= introScreens.Length)
        {
            return null;
        }

        return introScreens[index];
    }
}
