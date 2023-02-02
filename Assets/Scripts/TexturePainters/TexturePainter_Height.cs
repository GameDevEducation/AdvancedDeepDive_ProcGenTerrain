using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePainter_Height : BaseTexturePainter
{
    [SerializeField] TextureConfig Texture;
    [SerializeField] float StartHeight;
    [SerializeField] float EndHeight;
    [SerializeField] AnimationCurve Intensity;
    [SerializeField] bool SuppressOtherTextures = false;
    [SerializeField] AnimationCurve SuppressionIntensity;

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        int textureLayer = generationData.Manager.GetLayerForTexture(Texture);

        float heightMapStart = StartHeight / generationData.HeightmapScale.y;
        float heightMapEnd = EndHeight / generationData.HeightmapScale.y;
        float heightMapRangeInv = 1f / (heightMapEnd - heightMapStart);

        int numAlphaMaps = generationData.AlphaMaps.GetLength(2);

        for (int y = 0; y < generationData.AlphaMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)generationData.MapResolution / (float)generationData.AlphaMapResolution);

            for (int x = 0; x < generationData.AlphaMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)generationData.MapResolution / (float)generationData.AlphaMapResolution);

                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && generationData.BiomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                float height = generationData.HeightMap[heightMapX, heightMapY];

                // outside of height range
                if (height < heightMapStart || height > heightMapEnd)
                    continue;

                float heightPercentage = (height - heightMapStart) * heightMapRangeInv;
                generationData.AlphaMaps[x, y, textureLayer] = Strength * Intensity.Evaluate(heightPercentage);

                // if suppression of other textures is on then update the other layers
                if (SuppressOtherTextures)
                {
                    float suppression = SuppressionIntensity.Evaluate(heightPercentage);

                    // apply suppression to other layers
                    for (int layerIndex = 0; layerIndex < numAlphaMaps; ++layerIndex)
                    {
                        if (layerIndex == textureLayer)
                            continue;

                        generationData.AlphaMaps[x, y, layerIndex] *= suppression;
                    }
                }
            }
        }        
    }

    public override List<TextureConfig> RetrieveTextures()
    {
        List<TextureConfig> allTextures = new List<TextureConfig>(1);
        allTextures.Add(Texture);

        return allTextures;
    }    
}
