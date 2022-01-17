using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

[System.Serializable]
public class PlaceableObjectConfig
{
    public bool HasHeightLimits = false;
    public float MinHeightToSpawn = 0f;
    public float MaxHeightToSpawn = 0f;

    public bool CanGoInWater = false;
    public bool CanGoAboveWater = true;

    [Range(0f, 1f)] public float Weighting = 1f;
    public List<GameObject> Prefabs;

    public float NormalisedWeighting { get; set; } = 0f;
}

public class BaseObjectPlacer : MonoBehaviour
{
    [SerializeField] protected List<PlaceableObjectConfig> Objects;
    [SerializeField] protected float TargetDensity = 0.1f;
    [SerializeField] protected int MaxSpawnCount = 1000;
    [SerializeField] protected int MaxInvalidLocationSkips = 10;
    [SerializeField] protected float MaxPositionJitter = 0.15f;

    protected List<Vector3> GetAllLocationsForBiome(ProcGenConfigSO globalConfig, int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap, int biomeIndex)
    {
        List<Vector3> locations = new List<Vector3>(mapResolution * mapResolution / 10);

        for (int y = 0; y < mapResolution; ++y)
        {
            for (int x = 0; x < mapResolution; ++x)
            {
                if (biomeMap[x, y] != biomeIndex)
                    continue;

                float height = heightMap[x, y] * heightmapScale.y;

                locations.Add(new Vector3(y * heightmapScale.z, height, x * heightmapScale.x));
            }
        }

        return locations;
    }

    public virtual void Execute(ProcGenConfigSO globalConfig, Transform objectRoot, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        // validate the configs
        foreach(var config in Objects)
        {
            if (!config.CanGoInWater && !config.CanGoAboveWater)
                throw new System.InvalidOperationException($"Object placer forbids both in and out of water. Cannot run!");
        }

        // normalise the weightings
        float weightSum = 0f;
        foreach (var config in Objects)
            weightSum += config.Weighting;
        foreach (var config in Objects)
            config.NormalisedWeighting = config.Weighting / weightSum;
    }
    
    protected virtual void ExecuteSimpleSpawning(ProcGenConfigSO globalConfig, Transform objectRoot, List<Vector3> candidateLocations)
    {
        foreach (var spawnConfig in Objects)
        {
            // pick a random prefab
            var prefab = spawnConfig.Prefabs[Random.Range(0, spawnConfig.Prefabs.Count)];

            // determine the spawn count
            float baseSpawnCount = Mathf.Min(MaxSpawnCount, candidateLocations.Count * TargetDensity);
            int numToSpawn = Mathf.FloorToInt(spawnConfig.NormalisedWeighting * baseSpawnCount);

            int skipCount = 0;
            int numPlaced = 0;
            for (int index = 0; index < numToSpawn; ++index)
            {
                // pick a random location to spawn at
                int randomLocationIndex = Random.Range(0, candidateLocations.Count);
                Vector3 spawnLocation = candidateLocations[randomLocationIndex];

                // height is invalid?
                bool isValid = true;
                if (spawnLocation.y < globalConfig.WaterHeight && !spawnConfig.CanGoInWater)
                    isValid = false;
                if (spawnLocation.y >= globalConfig.WaterHeight && !spawnConfig.CanGoAboveWater)
                    isValid = false;

                // skip if outside of height limits
                if (spawnConfig.HasHeightLimits && (spawnLocation.y < spawnConfig.MinHeightToSpawn ||
                                                    spawnLocation.y >= spawnConfig.MaxHeightToSpawn))
                    isValid = false;

                // location is not valid?
                if (!isValid)
                {
                    ++skipCount;
                    --index;

                    if (skipCount >= MaxInvalidLocationSkips)
                        break;

                    continue;
                }
                skipCount = 0;
                ++numPlaced;

                // remove the location if chosen
                candidateLocations.RemoveAt(randomLocationIndex);

                SpawnObject(prefab, spawnLocation, objectRoot);
            }

            Debug.Log($"Placed {numPlaced} objects out of {numToSpawn}");
        }
    }

    protected virtual void SpawnObject(GameObject prefab, Vector3 spawnLocation, Transform objectRoot)
    {
        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Vector3 positionOffset = new Vector3(Random.Range(-MaxPositionJitter, MaxPositionJitter),
                                             0,
                                             Random.Range(-MaxPositionJitter, MaxPositionJitter));

        // instantiate the prefab
#if UNITY_EDITOR
        if (Application.isPlaying)
            Instantiate(prefab, spawnLocation + positionOffset, spawnRotation, objectRoot);
        else
        {
            var spawnedGO = PrefabUtility.InstantiatePrefab(prefab, objectRoot) as GameObject;
            spawnedGO.transform.position = spawnLocation + positionOffset;
            spawnedGO.transform.rotation = spawnRotation;
            Undo.RegisterCreatedObjectUndo(spawnedGO, "Placed object");
        }
#else
        Instantiate(Prefab, spawnLocation + positionOffset, spawnRotation, objectRoot);
#endif // UNITY_EDITOR 
    }
}
