using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave Definition", menuName = "Skyfire/New Wave Definition")]
public class WaveSO : ScriptableObject
{
    [Header("Wave Composition")]
    [Tooltip("The pool of enemy prefabs that can be randomly chosen for this wave.")]
    public List<GameObject> enemyPrefabs;

    [Header("Wave Size & Scaling")]
    [Tooltip("The starting number of enemies to spawn.")]
    public int initialWaveSize = 2;
    [Tooltip("The maximum number of enemies this wave can scale up to.")]
    public int maxWaveSize = 15;
    [Tooltip("For every X points the player scores, add one more enemy to this wave.")]
    public int scoreStepForWaveIncrease = 100;

    [Header("Wave Timing & Scaling")]
    [Tooltip("The starting time in seconds between waves.")]
    public float initialWaveInterval = 3.0f;
    [Tooltip("The fastest possible time in seconds between waves.")]
    public float minimumWaveInterval = 1.0f;
    [Tooltip("How much to reduce the wave interval per speed-up step.")]
    public float intervalReductionPerStep = 0.1f;
    [Tooltip("For every X points the player scores, the time between waves gets shorter.")]
    public int scoreStepForSpeedUp = 100;

    [Header("Intra-Wave Spawning")]
    [Tooltip("The time in seconds between each enemy group spawn within this wave.")]
    public float spawnInterval = 0.5f;
}