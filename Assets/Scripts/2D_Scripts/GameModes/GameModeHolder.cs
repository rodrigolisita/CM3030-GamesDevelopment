using UnityEngine;
using System;

[Serializable]
public enum GameMode
{
    Arcade,
    Campaign
}

[CreateAssetMenu(fileName = "GameModeHolder", menuName = "Skyfire/GameModeHolder")]
public class GameModeHolder : ScriptableObject
{
    public GameMode gameMode;
}
