using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BiomeConfig
{
    public BiomeConfigSO Biome;

    [Range(0f, 1f)] public float Weighting = 1f;
}

[CreateAssetMenu(fileName = "ProcGen Config", menuName = "Procedural Generation/ProcGen Configuration", order = -1)]
public class ProcGenConfigSO : ScriptableObject
{
    public List<BiomeConfig> Biomes;

    [Range(0f, 1f)] public float BiomeSeedPointDensity = 0.1f;

    public int NumBiomes => Biomes.Count;

    public float TotalWeighting
    {
        get
        {
            float sum = 0f;

            foreach(var config in Biomes)
            {
                sum += config.Weighting;
            }

            return sum;
        }
    }
}
