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

    public override void Execute(ProcGenManager manager, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        int textureLayer = manager.GetLayerForTexture(Texture);

        float heightMapStart = StartHeight / heightmapScale.y;
        float heightMapEnd = EndHeight / heightmapScale.y;
        float heightMapRangeInv = 1f / (heightMapEnd - heightMapStart);

        int numAlphaMaps = alphaMaps.GetLength(2);

        for (int y = 0; y < alphaMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)mapResolution / (float)alphaMapResolution);

            for (int x = 0; x < alphaMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)mapResolution / (float)alphaMapResolution);

                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                float height = heightMap[heightMapX, heightMapY];

                // outside of height range
                if (height < heightMapStart || height > heightMapEnd)
                    continue;

                float heightPercentage = (height - heightMapStart) * heightMapRangeInv;
                alphaMaps[x, y, textureLayer] = Strength * Intensity.Evaluate(heightPercentage);

                // if suppression of other textures is on then update the other layers
                if (SuppressOtherTextures)
                {
                    float suppression = SuppressionIntensity.Evaluate(heightPercentage);

                    // apply suppression to other layers
                    for (int layerIndex = 0; layerIndex < numAlphaMaps; ++layerIndex)
                    {
                        if (layerIndex == textureLayer)
                            continue;

                        alphaMaps[x, y, layerIndex] *= suppression;
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
