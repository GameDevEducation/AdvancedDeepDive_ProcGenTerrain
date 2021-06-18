using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer_Random : BaseObjectPlacer
{
    [SerializeField] float TargetDensity = 0.1f;
    [SerializeField] int MaxSpawnCount = 1000;
    [SerializeField] GameObject Prefab;

    public override void Execute(Transform objectRoot, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        // get potential spawn location
        List<Vector3> candidateLocations = GetAllLocationsForBiome(mapResolution, heightMap, heightmapScale, biomeMap, biomeIndex);

        int numToSpawn = Mathf.FloorToInt(Mathf.Min(MaxSpawnCount, candidateLocations.Count * TargetDensity));
        for (int index = 0; index < numToSpawn; ++index)
        {
            // pick a random location to spawn at
            int randomLocationIndex = Random.Range(0, candidateLocations.Count);
            Vector3 spawnLocation = candidateLocations[randomLocationIndex];
            candidateLocations.RemoveAt(randomLocationIndex);

            // instantiate the prefab
            GameObject newObject = Instantiate(Prefab, spawnLocation, Quaternion.identity, objectRoot);
        }
    }
}
