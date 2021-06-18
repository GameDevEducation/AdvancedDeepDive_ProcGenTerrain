using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapModifier_Random : BaseHeightMapModifier
{
    [SerializeField] float HeightDelta;

    public override void Execute(int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        for (int y = 0; y < mapResolution; ++y)
        {
            for (int x = 0; x < mapResolution; ++x)
            {
                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && biomeMap[x, y] != biomeIndex)
                    continue;
                
                // calculate the new height
                float newHeight = heightMap[x, y] + (Random.Range(-HeightDelta, HeightDelta) / heightmapScale.y);

                // blend based on strength
                heightMap[x, y] = Mathf.Lerp(heightMap[x, y], newHeight, Strength);
            }
        }
    }        
}
