using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailPainter_Slope : BaseDetailPainter
{
    [SerializeField] TerrainDetailConfig TerrainDetail;
    [SerializeField] AnimationCurve IntensityVsSlope;

    public override void Execute(ProcGenManager manager, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, List<int[,]> detailLayerMaps, int detailMapResolution, int maxDetailsPerPatch, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        int detailLayer = manager.GetDetailLayerForTerrainDetail(TerrainDetail);

        for (int y = 0; y < detailMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)mapResolution / (float)detailMapResolution);

            for (int x = 0; x < detailMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)mapResolution / (float)detailMapResolution);

                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                detailLayerMaps[detailLayer][x, y] = Mathf.FloorToInt(Strength * IntensityVsSlope.Evaluate(1f - slopeMap[x, y]) * maxDetailsPerPatch);
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
