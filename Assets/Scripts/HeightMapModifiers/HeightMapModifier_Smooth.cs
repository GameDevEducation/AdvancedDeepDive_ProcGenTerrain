using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapModifier_Smooth : BaseHeightMapModifier
{
    [SerializeField] int SmoothingKernelSize = 5;

    [SerializeField] bool UseAdaptiveKernel = false;
    [SerializeField] [Range(0f, 1f)] float MaxHeightThreshold = 0.5f;
    [SerializeField] int MinKernelSize = 2;
    [SerializeField] int MaxKernelSize = 7;

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        if (generationData.BiomeMap != null)
        {
            Debug.LogError("HeightMapModifier_Smooth is not supported as a per biome modifier [" + gameObject.name + "]");
            return;
        }

        float[,] smoothedHeights = new float[generationData.MapResolution, generationData.MapResolution];

        for (int y = 0; y < generationData.MapResolution; ++y)
        {
            for (int x = 0; x < generationData.MapResolution; ++x)
            {
                float heightSum = 0f;
                int numValues = 0;

                // set the kernell size
                int kernelSize = SmoothingKernelSize;
                if (UseAdaptiveKernel)
                {
                    kernelSize = Mathf.RoundToInt(Mathf.Lerp(MaxKernelSize, MinKernelSize, generationData.HeightMap[x, y] / MaxHeightThreshold));
                }

                // sum the neighbouring values
                for (int yDelta = -kernelSize; yDelta <= kernelSize; ++yDelta)
                {
                    int workingY = y + yDelta;
                    if (workingY < 0 || workingY >= generationData.MapResolution)
                        continue;

                    for (int xDelta = -kernelSize; xDelta <= kernelSize; ++xDelta)
                    {
                        int workingX = x + xDelta;
                        if (workingX < 0 || workingX >= generationData.MapResolution)
                            continue;

                        heightSum += generationData.HeightMap[workingX, workingY];
                        ++numValues;
                    }                    
                }

                // store the smoothed (aka average) height
                smoothedHeights[x, y] = heightSum / numValues;
            }
        }

        for (int y = 0; y < generationData.MapResolution; ++y)
        {
            for (int x = 0; x < generationData.MapResolution; ++x)
            {
                // blend based on strength
                generationData.HeightMap[x, y] = Mathf.Lerp(generationData.HeightMap[x, y], smoothedHeights[x, y], Strength);
            }
        }        
    }
}
