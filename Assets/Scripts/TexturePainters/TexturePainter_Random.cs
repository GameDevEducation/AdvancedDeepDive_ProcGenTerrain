using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RandomPainterConfig
{
    public TextureConfig TextureToPaint;
    [Range(0f, 1f)] public float IntensityModifier = 1f;

    public float NoiseScale;
    [Range(0f, 1f)] public float NoiseThreshold;
}

public class TexturePainter_Random : BaseTexturePainter
{
    [SerializeField] TextureConfig BaseTexture;
    [SerializeField] List<RandomPainterConfig> PaintingConfigs;

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        int baseTextureLayer = generationData.Manager.GetLayerForTexture(BaseTexture);

        for (int y = 0; y < generationData.AlphaMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)generationData.MapResolution / (float)generationData.AlphaMapResolution);

            for (int x = 0; x < generationData.AlphaMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)generationData.MapResolution / (float)generationData.AlphaMapResolution);

                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && generationData.BiomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                // perform the painting
                foreach(var config in PaintingConfigs)
                {
                    // check if noise test passed?
                    float noiseValue = Mathf.PerlinNoise(x * config.NoiseScale, y * config.NoiseScale);
                    if (Random.Range(0f, 1f) >= noiseValue)
                    {
                        int layer = generationData.Manager.GetLayerForTexture(config.TextureToPaint);
                        generationData.AlphaMaps[x, y, layer] = Strength * config.IntensityModifier;
                    }
                }

                generationData.AlphaMaps[x, y, baseTextureLayer] = Strength;
            }
        }        
    }

    [System.NonSerialized] List<TextureConfig> CachedTextures = null;

    public override List<TextureConfig> RetrieveTextures()
    {
        if (CachedTextures == null)
        {
            CachedTextures = new List<TextureConfig>();
            CachedTextures.Add(BaseTexture);
            foreach(var config in PaintingConfigs)
                CachedTextures.Add(config.TextureToPaint);
        }

        return CachedTextures;
    }       
}
