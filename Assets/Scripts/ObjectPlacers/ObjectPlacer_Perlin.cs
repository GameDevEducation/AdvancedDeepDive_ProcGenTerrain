using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class ObjectPlacer_Perlin : BaseObjectPlacer
{
    [SerializeField] Vector2 NoiseScale = new Vector2(1f / 128f, 1f / 128f);
    [SerializeField] float NoiseThreshold = 0.5f;

    List<Vector3> GetFilteredLocationsForBiome(ProcGenManager.GenerationData generationData, int biomeIndex)
    {
        List<Vector3> locations = new List<Vector3>(generationData.MapResolution * generationData.MapResolution / 10);

        for (int y = 0; y < generationData.MapResolution; ++y)
        {
            for (int x = 0; x < generationData.MapResolution; ++x)
            {
                if (generationData.BiomeMap[x, y] != biomeIndex)
                    continue;

                // calculte the noise value
                float noiseValue = Mathf.PerlinNoise(x * NoiseScale.x, y * NoiseScale.y);

                // noise must be above the threshold to be considered a candidate point
                if (noiseValue < NoiseThreshold)
                    continue;

                float height = generationData.HeightMap[x, y] * generationData.HeightmapScale.y;

                locations.Add(new Vector3(y * generationData.HeightmapScale.z, height, x * generationData.HeightmapScale.x));
            }
        }

        return locations;
    }

    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        base.Execute(generationData, biomeIndex, biome);

        // get potential spawn location
        List<Vector3> candidateLocations = GetFilteredLocationsForBiome(generationData, biomeIndex);

        ExecuteSimpleSpawning(generationData.Config, generationData.ObjectRoot, candidateLocations);
    }
}