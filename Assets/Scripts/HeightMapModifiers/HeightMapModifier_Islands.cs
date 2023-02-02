using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapModifier_Islands : BaseHeightMapModifier
{
    [SerializeField][Range(1, 100)] int NumIslands = 100;
    [SerializeField] float MinIslandSize = 20f;
    [SerializeField] float MaxIslandSize = 80f;
    [SerializeField] float MinIslandHeight = 10f;
    [SerializeField] float MaxIslandHeight = 40f;
    [SerializeField] float AngleNoiseScale = 1f;
    [SerializeField] float DistanceNoiseScale = 1f;
    [SerializeField] float NoiseHeightDelta = 5f;
    [SerializeField] AnimationCurve IslandShapeCurve;

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        for (int island = 0; island < NumIslands; ++island)
        {
            PlaceIsland(generationData.Config, generationData.MapResolution, generationData.HeightMap, generationData.HeightmapScale, generationData.BiomeMap, biomeIndex, biome);
        }
    }

    void PlaceIsland(ProcGenConfigSO globalConfig, int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        int workingIslandSize = Mathf.RoundToInt(Random.Range(MinIslandSize, MaxIslandSize) / heightmapScale.x);
        float workingIslandHeight = (Random.Range(MinIslandHeight, MaxIslandHeight) + globalConfig.WaterHeight) / heightmapScale.y;

        int centreX = Random.Range(workingIslandSize, mapResolution - workingIslandSize);
        int centreY = Random.Range(workingIslandSize, mapResolution - workingIslandSize);

        for (int islandY = -workingIslandSize; islandY <= workingIslandSize; islandY++)
        {
            int y = centreY + islandY;

            if (y < 0 || y >= mapResolution)
                continue;

            for (int islandX = -workingIslandSize; islandX <= workingIslandSize; islandX++)
            {
                int x = centreX + islandX;

                if (x < 0 || x >= mapResolution)
                    continue;

                float normalisedDistance = Mathf.Sqrt(islandX * islandX + islandY * islandY) / workingIslandSize;
                if (normalisedDistance > 1)
                    continue;

                float normalisedAngle = Mathf.Clamp01((Mathf.Atan2(islandY, islandX) + Mathf.PI) / (2 * Mathf.PI));
                float noise = Mathf.PerlinNoise(normalisedAngle * AngleNoiseScale, normalisedDistance * DistanceNoiseScale);

                float noiseHeightDelta = ((noise - 0.5f) * 2f) * NoiseHeightDelta / heightmapScale.y;

                float height = workingIslandHeight * IslandShapeCurve.Evaluate(normalisedDistance) + noiseHeightDelta;

                heightMap[x, y] = Mathf.Max(heightMap[x, y], height);
            }
        }
    }
}
