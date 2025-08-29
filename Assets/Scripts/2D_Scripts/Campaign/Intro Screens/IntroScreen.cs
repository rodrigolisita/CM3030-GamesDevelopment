using UnityEngine;

[CreateAssetMenu(fileName = "New Intro Screen", menuName = "Skyfire/Campaign Content/IntroScreen")]
public class IntroScreen : ScriptableObject
{
    [SerializeField] public string title;
    [SerializeField] [TextArea(10, 30)] public string text;
}
