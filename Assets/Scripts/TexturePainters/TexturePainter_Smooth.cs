using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePainter_Smooth : BaseTexturePainter
{
    [SerializeField] int SmoothingKernelSize = 5;

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        if (generationData.BiomeMap != null)
        {
            Debug.LogError("TexturePainter_Smooth is not supported as a per biome modifier [" + gameObject.name + "]");
            return;
        }

        for (int layer = 0; layer < generationData.AlphaMaps.GetLength(2); ++layer)
        {
            float[,] smoothedAlphaMap = new float[generationData.AlphaMapResolution, generationData.AlphaMapResolution];

            for (int y = 0; y < generationData.AlphaMapResolution; ++y)
            {
                for (int x = 0; x < generationData.AlphaMapResolution; ++x)
                {
                    float alphaSum = 0f;
                    int numValues = 0;

                    // sum the neighbouring values
                    for (int yDelta = -SmoothingKernelSize; yDelta <= SmoothingKernelSize; ++yDelta)
                    {
                        int workingY = y + yDelta;
                        if (workingY < 0 || workingY >= generationData.AlphaMapResolution)
                            continue;

                        for (int xDelta = -SmoothingKernelSize; xDelta <= SmoothingKernelSize; ++xDelta)
                        {
                            int workingX = x + xDelta;
                            if (workingX < 0 || workingX >= generationData.AlphaMapResolution)
                                continue;

                            alphaSum += generationData.AlphaMaps[workingX, workingY, layer];
                            ++numValues;
                        }                    
                    }

                    // store the smoothed (aka average) alpha
                    smoothedAlphaMap[x, y] = alphaSum / numValues;
                }
            }

            for (int y = 0; y < generationData.AlphaMapResolution; ++y)
            {
                for (int x = 0; x < generationData.AlphaMapResolution; ++x)
                {
                    // blend based on strength
                    generationData.AlphaMaps[x, y, layer] = Mathf.Lerp(generationData.AlphaMaps[x, y, layer], smoothedAlphaMap[x, y], Strength);
                }
            }  
        }
    
    }
}
