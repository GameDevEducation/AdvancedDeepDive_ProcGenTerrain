using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer_Random : BaseObjectPlacer
{
    public override void Execute(ProcGenConfigSO globalConfig, Transform objectRoot, int mapResolution, float[,] heightMap, 
                                 Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, 
                                 byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        base.Execute(globalConfig, objectRoot, mapResolution, heightMap, heightmapScale, slopeMap, alphaMaps, alphaMapResolution,
                     biomeMap, biomeIndex, biome);

        // get potential spawn location
        List<Vector3> candidateLocations = GetAllLocationsForBiome(globalConfig, mapResolution, heightMap, heightmapScale, 
                                                                   biomeMap, biomeIndex);

        ExecuteSimpleSpawning(globalConfig, objectRoot, candidateLocations);
    }
}