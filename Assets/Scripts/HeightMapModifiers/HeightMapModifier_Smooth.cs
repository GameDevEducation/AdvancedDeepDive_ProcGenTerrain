using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapModifier_Smooth : BaseHeightMapModifier
{
    [SerializeField] int SmoothingKernelSize = 5;

    public override void Execute(ProcGenConfigSO globalConfig, int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        if (biomeMap != null)
        {
            Debug.LogError("HeightMapModifier_Smooth is not supported as a per biome modifier [" + gameObject.name + "]");
            return;
        }

        float[,] smoothedHeights = new float[mapResolution, mapResolution];

        for (int y = 0; y < mapResolution; ++y)
        {
            for (int x = 0; x < mapResolution; ++x)
            {
                float heightSum = 0f;
                int numValues = 0;

                // sum the neighbouring values
                for (int yDelta = -SmoothingKernelSize; yDelta <= SmoothingKernelSize; ++yDelta)
                {
                    int workingY = y + yDelta;
                    if (workingY < 0 || workingY >= mapResolution)
                        continue;

                    for (int xDelta = -SmoothingKernelSize; xDelta <= SmoothingKernelSize; ++xDelta)
                    {
                        int workingX = x + xDelta;
                        if (workingX < 0 || workingX >= mapResolution)
                            continue;

                        heightSum += heightMap[workingX, workingY];
                        ++numValues;
                    }                    
                }

                // store the smoothed (aka average) height
                smoothedHeights[x, y] = heightSum / numValues;
            }
        }

        for (int y = 0; y < mapResolution; ++y)
        {
            for (int x = 0; x < mapResolution; ++x)
            {
                // blend based on strength
                heightMap[x, y] = Mathf.Lerp(heightMap[x, y], smoothedHeights[x, y], Strength);
            }
        }        
    }
}
