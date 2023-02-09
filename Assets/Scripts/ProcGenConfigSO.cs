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

    public GameObject BiomeGenerators;
    public GameObject InitialHeightModifier;
    public GameObject HeightPostProcessingModifier;

    public GameObject PaintingPostProcessingModifier;
    public GameObject DetailPaintingPostProcessingModifier;

    public float WaterHeight = 15f;

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

    public byte GetIndexForBiome(BiomeConfigSO biome)
    {
        for (int index = 0; index < Biomes.Count; ++index)
        {
            if (Biomes[index].Biome == biome)
                return (byte)index;
        }

        return byte.MaxValue;
    }
}
