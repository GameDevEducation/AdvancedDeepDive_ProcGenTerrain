using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailPainter_Height : BaseDetailPainter
{
    [SerializeField] TerrainDetailConfig TerrainDetail;
    [SerializeField] float StartHeight;
    [SerializeField] float EndHeight;
    [SerializeField] AnimationCurve Intensity;
    [SerializeField] bool SuppressOtherDetails = false;
    [SerializeField] AnimationCurve SuppressionIntensity;

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        int detailLayer = generationData.Manager.GetDetailLayerForTerrainDetail(TerrainDetail);

        float heightMapStart = StartHeight / generationData.HeightmapScale.y;
        float heightMapEnd = EndHeight / generationData.HeightmapScale.y;
        float heightMapRangeInv = 1f / (heightMapEnd - heightMapStart);

        int numDetailLayers = generationData.DetailLayerMaps.Count;

        for (int y = 0; y < generationData.DetailMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)generationData.MapResolution / (float)generationData.DetailMapResolution);

            for (int x = 0; x < generationData.DetailMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)generationData.MapResolution / (float)generationData.DetailMapResolution);

                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && generationData.BiomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                float height = generationData.HeightMap[heightMapX, heightMapY];

                // outside of height range
                if (height < heightMapStart || height > heightMapEnd)
                    continue;

                float heightPercentage = (height - heightMapStart) * heightMapRangeInv;
                generationData.DetailLayerMaps[detailLayer][x, y] = Mathf.FloorToInt(Strength * Intensity.Evaluate(heightPercentage) * generationData.MaxDetailsPerPatch);

                // if suppression of other details is on then update the other layers
                if (SuppressOtherDetails)
                {
                    float suppression = SuppressionIntensity.Evaluate(heightPercentage);

                    // apply suppression to other layers
                    for (int layerIndex = 0; layerIndex < numDetailLayers; ++layerIndex)
                    {
                        if (layerIndex == detailLayer)
                            continue;

                        generationData.DetailLayerMaps[detailLayer][x, y] = Mathf.FloorToInt(generationData.DetailLayerMaps[detailLayer][x, y] * suppression);
                    }
                }
            }
        }
    }

    public override List<TerrainDetailConfig> RetrieveTerrainDetails()
    {
        List<TerrainDetailConfig> allTerrainDetails = new List<TerrainDetailConfig>(1);
        allTerrainDetails.Add(TerrainDetail);

        return allTerrainDetails;
    }
}
