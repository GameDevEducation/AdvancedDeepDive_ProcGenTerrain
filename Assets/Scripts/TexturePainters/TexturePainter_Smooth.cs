using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePainter_Smooth : BaseTexturePainter
{
    [SerializeField] int SmoothingKernelSize = 5;
    
    public override void Execute(ProcGenManager manager, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        if (biomeMap != null)
        {
            Debug.LogError("TexturePainter_Smooth is not supported as a per biome modifier [" + gameObject.name + "]");
            return;
        }

        for (int layer = 0; layer < alphaMaps.GetLength(2); ++layer)
        {
            float[,] smoothedAlphaMap = new float[alphaMapResolution, alphaMapResolution];

            for (int y = 0; y < alphaMapResolution; ++y)
            {
                for (int x = 0; x < alphaMapResolution; ++x)
                {
                    float alphaSum = 0f;
                    int numValues = 0;

                    // sum the neighbouring values
                    for (int yDelta = -SmoothingKernelSize; yDelta <= SmoothingKernelSize; ++yDelta)
                    {
                        int workingY = y + yDelta;
                        if (workingY < 0 || workingY >= alphaMapResolution)
                            continue;

                        for (int xDelta = -SmoothingKernelSize; xDelta <= SmoothingKernelSize; ++xDelta)
                        {
                            int workingX = x + xDelta;
                            if (workingX < 0 || workingX >= alphaMapResolution)
                                continue;

                            alphaSum += alphaMaps[workingX, workingY, layer];
                            ++numValues;
                        }                    
                    }

                    // store the smoothed (aka average) alpha
                    smoothedAlphaMap[x, y] = alphaSum / numValues;
                }
            }

            for (int y = 0; y < alphaMapResolution; ++y)
            {
                for (int x = 0; x < alphaMapResolution; ++x)
                {
                    // blend based on strength
                    alphaMaps[x, y, layer] = Mathf.Lerp(alphaMaps[x, y, layer], smoothedAlphaMap[x, y], Strength);
                }
            }  
        }
    
    }
}
