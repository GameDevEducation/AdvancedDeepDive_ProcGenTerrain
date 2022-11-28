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

    public override void Execute(ProcGenManager manager, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, List<int[,]> detailLayerMaps, int detailMapResolution, int maxDetailsPerPatch, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        for (int y = 0; y < detailMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)mapResolution / (float)detailMapResolution);

            for (int x = 0; x < detailMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)mapResolution / (float)detailMapResolution);

                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                // perform the painting
                foreach (var config in PaintingConfigs)
                {
                    // check if noise test passed?
                    float noiseValue = Mathf.PerlinNoise(x * config.NoiseScale, y * config.NoiseScale);
                    if (Random.Range(0f, 1f) >= noiseValue)
                    {
                        int layer = manager.GetDetailLayerForTerrainDetail(config.DetailToPaint);
                        detailLayerMaps[layer][x, y] = Mathf.FloorToInt(Strength * config.IntensityModifier * maxDetailsPerPatch);
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
