using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailPainter_Slope : BaseDetailPainter
{
    [SerializeField] TerrainDetailConfig TerrainDetail;
    [SerializeField] AnimationCurve IntensityVsSlope;

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        int detailLayer = generationData.Manager.GetDetailLayerForTerrainDetail(TerrainDetail);

        for (int y = 0; y < generationData.DetailMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)generationData.MapResolution / (float)generationData.DetailMapResolution);

            for (int x = 0; x < generationData.DetailMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)generationData.MapResolution / (float)generationData.DetailMapResolution);

                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && generationData.BiomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                generationData.DetailLayerMaps[detailLayer][x, y] = Mathf.FloorToInt(Strength * IntensityVsSlope.Evaluate(1f - generationData.SlopeMap[x, y]) * generationData.MaxDetailsPerPatch);
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
