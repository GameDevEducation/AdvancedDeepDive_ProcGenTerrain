using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RandomDetailPainterConfig
{
    public TerrainDetailConfig DetailToPaint;
    [Range(0f, 1f)] public float IntensityModifier = 1f;

    public float NoiseScale;
    [Range(0f, 1f)] public float NoiseThreshold;
}

public class DetailPainter_Random : BaseDetailPainter
{
    [SerializeField] List<RandomDetailPainterConfig> PaintingConfigs = new List<RandomDetailPainterConfig>()
    {
        new RandomDetailPainterConfig()
    };

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        for (int y = 0; y < generationData.DetailMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)generationData.MapResolution / (float)generationData.DetailMapResolution);

            for (int x = 0; x < generationData.DetailMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)generationData.MapResolution / (float)generationData.DetailMapResolution);

                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && generationData.BiomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                // perform the painting
                foreach (var config in PaintingConfigs)
                {
                    // check if noise test passed?
                    float noiseValue = Mathf.PerlinNoise(x * config.NoiseScale, y * config.NoiseScale);
                    if (Random.Range(0f, 1f) >= noiseValue)
                    {
                        int layer = generationData.Manager.GetDetailLayerForTerrainDetail(config.DetailToPaint);
                        generationData.DetailLayerMaps[layer][x, y] = Mathf.FloorToInt(Strength * config.IntensityModifier * generationData.MaxDetailsPerPatch);
                    }
                }
            }
        }
    }

    [System.NonSerialized] List<TerrainDetailConfig> CachedTerrainDetails = null;

    public override List<TerrainDetailConfig> RetrieveTerrainDetails()
    {
        if (CachedTerrainDetails == null)
        {
            CachedTerrainDetails = new List<TerrainDetailConfig>();
            foreach (var config in PaintingConfigs)
                CachedTerrainDetails.Add(config.DetailToPaint);
        }

        return CachedTerrainDetails;
    }
}
