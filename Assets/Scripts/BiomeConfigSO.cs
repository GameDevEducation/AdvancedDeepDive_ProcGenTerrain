using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome Config", menuName = "Procedural Generation/Biome Configuration", order = -1)]
public class BiomeConfigSO : ScriptableObject
{
    public string Name;

    [Range(0f, 1f)] public float MinIntensity = 0.5f;
    [Range(0f, 1f)] public float MaxIntensity = 1f;

    [Range(0f, 1f)] public float MinDecayRate = 0.01f;
    [Range(0f, 1f)] public float MaxDecayRate = 0.02f;
}
