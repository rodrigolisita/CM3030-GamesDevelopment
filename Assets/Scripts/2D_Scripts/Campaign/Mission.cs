using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Skyfire/Campaign Content/Mission")]
public class Mission : ScriptableObject
{
    [Header("Mission Details")]
    [SerializeField] private string missionName;
    [SerializeField] private List<IntroScreen> introScreens;
    [SerializeField] private List<VictoryScreen> victoryScreens;
    [SerializeField] private Mission nextMission;

    [Header("Gameplay Definition")]
    [Tooltip("The Wave Definition asset that contains all the rules for this mission.")]
    [SerializeField] private WaveSO waveDefinition;

    [Header("Mission Finale")]
    [SerializeField] private GameObject bossPrefab;
    
    [Header("Optional Mission Overrides")]
    // Audio
    [Header("Mission Audio")]
    [Tooltip("Optional: If assigned, this music will play instead of the default active music.")]
    [SerializeField] private AudioClip missionMusic;

    [Header("Mission Environment")]
    [Tooltip("The complete environment prefab for this mission (includes background, parallax, spawners, etc.).")]
    public GameObject environmentPrefab;

    [Header("Mission Visuals")]
    [Tooltip("Check this box to apply a custom ambient light color to the mission.")]
    public bool overrideAmbientLight = false;
    [Tooltip("The color tint to apply to the scene (e.g., dark blue for night, orange for evening).")]
    public Color ambientLightColor = new Color(0, 0, 0, 0); // Default is fully transparent


    
    
    



    public string GetMissionName() { return missionName; }
    public IntroScreen GetIntro(int index)
    {
        if (index >= introScreens.Count) return null;
        return introScreens[index];
    }

    public bool HasVictoryScreen(int index)
    {
        return (index < victoryScreens.Count);
    }

    public VictoryScreen GetVictoryScreen(int index)
    {
        if (index >= victoryScreens.Count) return null;
        return victoryScreens[index];
    }

    public WaveSO GetWaveDefinition() { return waveDefinition; }
    
    // GETTER functions
    public AudioClip GetMissionMusic() { return missionMusic; }
    public GameObject GetBoss() { return bossPrefab; }

    //public Material GetBackgroundMaterial() 
    //{ 
    //    return backgroundMaterial; 
    //}

    public GameObject GetEnvironmentPrefab() 
    { 
        return environmentPrefab; 
    }

    // To play with light
    public bool ShouldOverrideAmbientLight()
    {
        return overrideAmbientLight;
    }

    public Color GetAmbientLightColor()
    {
        return ambientLightColor;
    }

    public Mission GetNextMission()
    {
        return nextMission;
    }
    
}