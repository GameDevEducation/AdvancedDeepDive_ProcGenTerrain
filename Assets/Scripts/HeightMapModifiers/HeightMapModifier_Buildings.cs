using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingConfig
{
    public Texture2D HeightMap;
    public GameObject Prefab;
    public int Radius;
    public int NumToSpawn = 1;
}

public class HeightMapModifier_Buildings : BaseHeightMapModifier
{
    [SerializeField] List<BuildingConfig> Buildings;

    protected void SpawnBuilding(BuildingConfig building, int spawnX, int spawnY,
                                 int mapResolution, float[,] heightMap, Vector3 heightmapScale,
                                 Transform buildingRoot)
    {
        float averageHeight = 0f;
        int numHeightSamples = 0;

        // sum the height values under the building
        for (int y = -building.Radius; y <= building.Radius; ++y)
        {
            for (int x = -building.Radius; x <= building.Radius; ++x)
            {
                // sum the heightmap values
                averageHeight += heightMap[x + spawnX, y +spawnY];
                ++numHeightSamples;
            }
        }

        // calculate the average height
        averageHeight /= numHeightSamples;

        float targetHeight = averageHeight;

        // apply the building heightmap
        for (int y = -building.Radius; y <= building.Radius; ++y)
        {
            int workingY = y + spawnY;
            float textureY = Mathf.Clamp01((float)(y + building.Radius) / (building.Radius * 2f));
            for (int x = -building.Radius; x <= building.Radius; ++x)
            {
                int workingX = x + spawnX;
                float textureX = Mathf.Clamp01((float)(x + building.Radius) / (building.Radius * 2f));

                // sample the height map
                var pixelColour = building.HeightMap.GetPixelBilinear(textureX, textureY);
                float strength = pixelColour.r;

                // blend based on strength
                heightMap[workingX, workingY] = Mathf.Lerp(heightMap[workingX, workingY], targetHeight, strength);
            }
        }

        // Spawn the building
        Vector3 buildingLocation = new Vector3(spawnY * heightmapScale.z, 
                                               heightMap[spawnX, spawnY] * heightmapScale.y, 
                                               spawnX * heightmapScale.x);
        Instantiate(building.Prefab, buildingLocation, Quaternion.identity, buildingRoot);
    }

    public override void Execute(int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        var buildingRoot = FindObjectOfType<ProcGenManager>().transform;

        // traverse the buildings
        foreach(var building in Buildings)
        {
            for (int buildingIndex = 0; buildingIndex < building.NumToSpawn; ++ buildingIndex)
            {
                int spawnX = Random.Range(building.Radius, mapResolution - building.Radius);
                int spawnY = Random.Range(building.Radius, mapResolution - building.Radius);

                SpawnBuilding(building, spawnX, spawnY, mapResolution, heightMap, heightmapScale, buildingRoot);
            }
        }
    }
}
