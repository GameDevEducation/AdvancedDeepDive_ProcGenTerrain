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

    public override void Execute(ProcGenConfigSO globalConfig, int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        foreach(var pass in Passes)
        {
            for (int y = 0; y < mapResolution; ++y)
            {
                for (int x = 0; x < mapResolution; ++x)
                {
                    // skip if we have a biome and this is not our biome
                    if (biomeIndex >= 0 && biomeMap[x, y] != biomeIndex)
                        continue;

                    float noiseValue = (Mathf.PerlinNoise(x * pass.NoiseScale, y * pass.NoiseScale) * 2f) - 1f;

                    // calculate the new height
                    float newHeight = heightMap[x, y] + (noiseValue * pass.HeightDelta / heightmapScale.y);

                    // blend based on strength
                    heightMap[x, y] = Mathf.Lerp(heightMap[x, y], newHeight, Strength);
                }
            }
        }
    }  
}
