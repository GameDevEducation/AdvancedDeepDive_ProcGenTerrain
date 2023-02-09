using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProcGenManager;

public class BaseBiomeMapGenerator : MonoBehaviour
{
    public enum EBiomeSelectionMode
    {
        PureRandom,
        WeightedRandom,
        ZoneBasedWeightedRandom
    }

    [System.Serializable]
    public class ZoneConfig
    {
        public BiomeConfigSO Biome;

        [Range(0f, 1f)] public float MinDistance = 0f;
        [Range(0f, 1f)] public float MaxDistance = 1f;
        public AnimationCurve BiomeWeightingVsDistance;

        [Range(0f, 360f)] public float StartAngle = 0f;
        [Range(0f, 360f)] public float EndAngle = 360f;
        public bool InsideOfAngle = true;

        public float GetWeightForLocation(Vector2 normalisedLocation)
        {
            float distance = Mathf.Clamp01(normalisedLocation.magnitude);

            if (distance < MinDistance || distance > MaxDistance)
                return -1f;

            float angle = Mathf.Atan2(normalisedLocation.x, normalisedLocation.y) * Mathf.Rad2Deg;
            if (angle < 0f)
                angle += 360f;

            if (InsideOfAngle)
            {
                if (angle < StartAngle || angle > EndAngle)
                    return -1f;
            }
            else
            {
                if (angle >= StartAngle && angle <= EndAngle)
                    return -1f;
            }

            return BiomeWeightingVsDistance.Evaluate(Mathf.InverseLerp(MinDistance, MaxDistance, distance));
        }
    }

    [SerializeField] EBiomeSelectionMode Mode = EBiomeSelectionMode.PureRandom;
    [SerializeField] List<ZoneConfig> Zones = new(); 

    List<byte> BiomesToSpawn = null;

    public virtual void Execute(ProcGenManager.GenerationData generationData)
    {
        Debug.LogError("No implementation of Execute function for " + gameObject.name);
    }

    protected void PrepareToSpawnBiomes(ProcGenManager.GenerationData generationData, int numSeedPoints)
    {
        if (Mode != EBiomeSelectionMode.WeightedRandom)
        {
            BiomesToSpawn = null;
            return;
        }

        BiomesToSpawn = new List<byte>(numSeedPoints);

        // populate the biomes to spawn based on weightings
        float totalBiomeWeighting = generationData.Config.TotalWeighting;
        for (int biomeIndex = 0; biomeIndex < generationData.Config.NumBiomes; ++biomeIndex)
        {
            int numEntries = Mathf.RoundToInt(numSeedPoints * generationData.Config.Biomes[biomeIndex].Weighting / totalBiomeWeighting);

            for (int entryIndex = 0; entryIndex < numEntries; ++entryIndex)
            {
                BiomesToSpawn.Add((byte)biomeIndex);
            }
        }
    }

    protected byte PickBiomeType(ProcGenManager.GenerationData generationData, Vector2 normalisedLocation)
    {
        if (Mode == EBiomeSelectionMode.WeightedRandom && BiomesToSpawn != null && BiomesToSpawn.Count > 0)
            return PickBiomeType_WeightedRandom(generationData, normalisedLocation);
        else if (Mode == EBiomeSelectionMode.ZoneBasedWeightedRandom && Zones != null && Zones.Count > 0)
            return PickBiomeType_ZoneBasedWeightedRandom(generationData, normalisedLocation);

        return (byte) generationData.Random(0, generationData.Config.NumBiomes);
    }

    byte PickBiomeType_WeightedRandom(ProcGenManager.GenerationData generationData, Vector2 normalisedLocation)
    {
        // pick a random seed point
        int seedPointIndex = generationData.Random(0, BiomesToSpawn.Count);

        // extract the biome index
        byte biomeIndex = BiomesToSpawn[seedPointIndex];

        // remove seed point
        BiomesToSpawn.RemoveAt(seedPointIndex);

        return biomeIndex;
    }

    byte PickBiomeType_ZoneBasedWeightedRandom(ProcGenManager.GenerationData generationData, Vector2 normalisedLocation)
    {
        List<Tuple<byte, float>> weightedBiomeOptions = new();
        float totalWeight = 0f;

        foreach(var zone in Zones)
        {
            float weight = zone.GetWeightForLocation(normalisedLocation);
            if (weight <= 0f) 
                continue;

            byte biomeIndex = generationData.Config.GetIndexForBiome(zone.Biome);
            if (biomeIndex == byte.MaxValue)
                continue;

            weightedBiomeOptions.Add(new Tuple<byte, float>(biomeIndex, weight + totalWeight));

            totalWeight += weight;
        }

        if (totalWeight > 0f) 
        {
            float roll = generationData.Random(0f, totalWeight);
            foreach (var weightedBiome in weightedBiomeOptions)
            {
                if (roll <= weightedBiome.Item2)
                    return weightedBiome.Item1;
            }
        }

        return (byte)generationData.Random(0, generationData.Config.NumBiomes);
    }
}
