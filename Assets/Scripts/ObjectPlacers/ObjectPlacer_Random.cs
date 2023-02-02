using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer_Random : BaseObjectPlacer
{
    public override void Execute(ProcGenManager.GenerationData generationData, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        base.Execute(generationData, biomeIndex, biome);

        // get potential spawn location
        List<Vector3> candidateLocations = GetAllLocationsForBiome(generationData, biomeIndex);

        ExecuteSimpleSpawning(generationData, generationData.ObjectRoot, candidateLocations);
    }
}