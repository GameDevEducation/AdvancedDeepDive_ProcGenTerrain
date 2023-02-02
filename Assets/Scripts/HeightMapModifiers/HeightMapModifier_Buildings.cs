using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

[System.Serializable]
public class BuildingConfig
{
    public Texture2D HeightMap;
    public GameObject Prefab;
    public int Radius;
    public int NumToSpawn = 1;

    public bool HasHeightLimits = false;
    public float MinHeightToSpawn = 0f;
    public float MaxHeightToSpawn = 0f;

    public bool CanGoInWater = false;
    public bool CanGoAboveWater = true;
}

public class HeightMapModifier_Buildings : BaseHeightMapModifier
{
    [SerializeField] List<BuildingConfig> Buildings;

    protected void SpawnBuilding(ProcGenConfigSO globalConfig, BuildingConfig building, 
                                 int spawnX, int spawnY,
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

        if (!building.CanGoInWater)
            targetHeight = Mathf.Max(targetHeight, globalConfig.WaterHeight / heightmapScale.y);
        if (building.HasHeightLimits)
            targetHeight = Mathf.Clamp(targetHeight, building.MinHeightToSpawn / heightmapScale.y, building.MaxHeightToSpawn / heightmapScale.y);

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

        // instantiate the prefab
#if UNITY_EDITOR
        if (Application.isPlaying)
            Instantiate(building.Prefab, buildingLocation, Quaternion.identity, buildingRoot);
        else
        {
            var spawnedGO = PrefabUtility.InstantiatePrefab(building.Prefab, buildingRoot) as GameObject;
            spawnedGO.transform.position = buildingLocation;
            Undo.RegisterCreatedObjectUndo(spawnedGO, "Add building");
        }
#else
        Instantiate(building.Prefab, buildingLocation, Quaternion.identity, buildingRoot);
#endif // UNITY_EDITOR   
    }

    protected List<Vector2Int> GetSpawnLocationsForBuilding(ProcGenConfigSO globalConfig, int mapResolution, float[,] heightMap, Vector3 heightmapScale, BuildingConfig buildingConfig)
    {
        List<Vector2Int> locations = new List<Vector2Int>(mapResolution * mapResolution / 10);

        for (int y = buildingConfig.Radius; (y < mapResolution - buildingConfig.Radius); y += buildingConfig.Radius * 2)
        {
            for (int x = buildingConfig.Radius; (x < mapResolution - buildingConfig.Radius); x += buildingConfig.Radius * 2)
            {
                float height = heightMap[x, y] * heightmapScale.y;

                // height is invalid?
                if (height < globalConfig.WaterHeight && !buildingConfig.CanGoInWater)
                    continue;
                if (height >= globalConfig.WaterHeight && !buildingConfig.CanGoAboveWater)
                    continue;

                // skip if outside of height limits
                if (buildingConfig.HasHeightLimits && (height < buildingConfig.MinHeightToSpawn || height >= buildingConfig.MaxHeightToSpawn))
                    continue;

                locations.Add(new Vector2Int(x, y));
            }
        }

        return locations;
    }

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        var buildingRoot = generationData.ObjectRoot;

        // traverse the buildings
        foreach(var building in Buildings)
        {
            var spawnLocations = GetSpawnLocationsForBuilding(generationData.Config, generationData.MapResolution, generationData.HeightMap, generationData.HeightmapScale, building);

            for (int buildingIndex = 0; buildingIndex < building.NumToSpawn && spawnLocations.Count > 0; ++ buildingIndex)
            {
                int spawnIndex = generationData.Random(0, spawnLocations.Count);
                var spawnPos = spawnLocations[spawnIndex];
                spawnLocations.RemoveAt(spawnIndex);

                SpawnBuilding(generationData.Config, building, spawnPos.x, spawnPos.y, generationData.MapResolution, generationData.HeightMap, generationData.HeightmapScale, buildingRoot);
            }
        }
    }
}
