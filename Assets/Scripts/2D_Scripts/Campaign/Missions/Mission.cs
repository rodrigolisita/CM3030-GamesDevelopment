using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Skyfire/Campaign Content/Mission")]
public class Mission : ScriptableObject
{
    [Header("Mission Details")]
    [SerializeField] private string missionName;
    [SerializeField] private List<IntroScreen> introScreens;

    [Header("Gameplay Definition")]
    [Tooltip("The Wave Definition asset that contains all the rules for this mission.")]
    [SerializeField] private WaveSO waveDefinition;
    
    // Audio
    [Header("Mission Audio")]
    [Tooltip("Optional: If assigned, this music will play instead of the default active music.")]
    [SerializeField] private AudioClip missionMusic;
    
    [Header("Mission Finale")]
    [SerializeField] private GameObject bossPrefab;


    public string GetMissionName() { return missionName; }
    public IntroScreen GetIntro(int index)
    {
        if (index >= introScreens.Count) return null;
        return introScreens[index];
    }
    public WaveSO GetWaveDefinition() { return waveDefinition; }
    
    // --- NEW GETTER ---
    public AudioClip GetMissionMusic() { return missionMusic; }
    public GameObject GetBoss() { return bossPrefab; }
}