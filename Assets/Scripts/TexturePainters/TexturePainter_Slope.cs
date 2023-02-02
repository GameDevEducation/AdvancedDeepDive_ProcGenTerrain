using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePainter_Slope : BaseTexturePainter
{
    [SerializeField] TextureConfig Texture;
    [SerializeField] AnimationCurve IntensityVsSlope;

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        int textureLayer = generationData.Manager.GetLayerForTexture(Texture);

        for (int y = 0; y < generationData.AlphaMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)generationData.MapResolution / (float)generationData.AlphaMapResolution);

            for (int x = 0; x < generationData.AlphaMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)generationData.MapResolution / (float)generationData.AlphaMapResolution);

                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && generationData.BiomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                generationData.AlphaMaps[x, y, textureLayer] = Strength * IntensityVsSlope.Evaluate(1f - generationData.SlopeMap[x, y]);
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
