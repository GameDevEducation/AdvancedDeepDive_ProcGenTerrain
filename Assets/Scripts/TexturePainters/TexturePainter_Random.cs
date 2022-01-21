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

    public override void Execute(ProcGenManager manager, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        int baseTextureLayer = manager.GetLayerForTexture(BaseTexture);

        for (int y = 0; y < alphaMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)mapResolution / (float)alphaMapResolution);

            for (int x = 0; x < alphaMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)mapResolution / (float)alphaMapResolution);

                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                // perform the painting
                foreach(var config in PaintingConfigs)
                {
                    // check if noise test passed?
                    float noiseValue = Mathf.PerlinNoise(x * config.NoiseScale, y * config.NoiseScale);
                    if (Random.Range(0f, 1f) >= noiseValue)
                    {
                        int layer = manager.GetLayerForTexture(config.TextureToPaint);
                        alphaMaps[x, y, layer] = Strength * config.IntensityModifier;
                    }
                }

                alphaMaps[x, y, baseTextureLayer] = Strength;
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
