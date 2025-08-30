using UnityEngine;

[CreateAssetMenu(fileName = "New Victory Screen", menuName = "Skyfire/Campaign Content/VictoryScreen")]
public class VictoryScreen : ScriptableObject
{
    [SerializeField] public string title;
    [SerializeField] [TextArea(10, 30)] public string text;
}
