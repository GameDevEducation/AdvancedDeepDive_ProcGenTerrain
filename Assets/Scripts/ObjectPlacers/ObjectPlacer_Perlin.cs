using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer_Perlin : BaseObjectPlacer
{
    [SerializeField] float TargetDensity = 0.1f;
    [SerializeField] int MaxSpawnCount = 1000;
    [SerializeField] Vector2 NoiseScale = new Vector2(1f / 128f, 1f / 128f);
    [SerializeField] float NoiseThreshold = 0.5f;
    [SerializeField] GameObject Prefab;

    List<Vector3> GetFilteredLocationsForBiome(int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap, int biomeIndex)
    {
        List<Vector3> locations = new List<Vector3>(mapResolution * mapResolution / 10);

        for (int y = 0; y < mapResolution; ++y)
        {
            for (int x = 0; x < mapResolution; ++x)
            {
                if (biomeMap[x, y] != biomeIndex)
                    continue;

                // calculte the noise value
                float noiseValue = Mathf.PerlinNoise(x * NoiseScale.x, y * NoiseScale.y);

                // noise must be above the threshold to be considered a candidate point
                if (noiseValue < NoiseThreshold)
                    continue;

                locations.Add(new Vector3(y * heightmapScale.z, heightMap[x, y] * heightmapScale.y, x * heightmapScale.x));
            }
        }

        return locations;
    }

    public override void Execute(Transform objectRoot, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        // get potential spawn location
        List<Vector3> candidateLocations = GetFilteredLocationsForBiome(mapResolution, heightMap, heightmapScale, biomeMap, biomeIndex);

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
