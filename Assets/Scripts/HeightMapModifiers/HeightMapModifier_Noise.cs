using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeightNoisePass
{
    public float HeightDelta = 1f;
    public float NoiseScale = 1f;
}

public class HeightMapModifier_Noise : BaseHeightMapModifier
{
    [SerializeField] List<HeightNoisePass> Passes;

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        foreach (var pass in Passes)
        {
            for (int y = 0; y < generationData.MapResolution; ++y)
            {
                for (int x = 0; x < generationData.MapResolution; ++x)
                {
                    // skip if we have a biome and this is not our biome
                    if (biomeIndex >= 0 && generationData.BiomeMap[x, y] != biomeIndex)
                        continue;

                    float noiseValue = (Mathf.PerlinNoise(x * pass.NoiseScale, y * pass.NoiseScale) * 2f) - 1f;

                    // calculate the new height
                    float newHeight = generationData.HeightMap[x, y] + (noiseValue * pass.HeightDelta / generationData.HeightmapScale.y);

                    // blend based on strength
                    generationData.HeightMap[x, y] = Mathf.Lerp(generationData.HeightMap[x, y], newHeight, Strength);
                }
            }
        }
    }  
}
