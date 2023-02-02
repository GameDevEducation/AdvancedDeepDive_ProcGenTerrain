using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProcGenManager;

public class BiomeGenerator_OozeBased : BaseBiomeMapGenerator
{
    public enum EBiomeMapBaseResolution
    {
        Size_64x64 = 64,
        Size_128x128 = 128,
        Size_256x256 = 256,
        Size_512x512 = 512
    }

    [Range(0f, 1f)] public float BiomeSeedPointDensity = 0.1f;
    public EBiomeMapBaseResolution BiomeMapResolution = EBiomeMapBaseResolution.Size_64x64;

    byte[,] BiomeMap_LowResolution;
    float[,] BiomeStrengths_LowResolution;

    public override void Execute(ProcGenManager.GenerationData generationData)
    {
        Perform_BiomeGeneration_LowResolution(generationData, (int)BiomeMapResolution);

        Perform_BiomeGeneration_HighResolution(generationData.Config, (int)BiomeMapResolution, generationData.MapResolution, generationData.BiomeMap, generationData.BiomeStrengths);
    }

    void Perform_BiomeGeneration_LowResolution(ProcGenManager.GenerationData generationData, int mapResolution)
    {
        // allocate the biome map and strength map
        BiomeMap_LowResolution = new byte[mapResolution, mapResolution];
        BiomeStrengths_LowResolution = new float[mapResolution, mapResolution];

        // setup space for the seed points
        int numSeedPoints = Mathf.FloorToInt(mapResolution * mapResolution * BiomeSeedPointDensity);
        List<byte> biomesToSpawn = new List<byte>(numSeedPoints);

        // populate the biomes to spawn based on weightings
        float totalBiomeWeighting = generationData.Config.TotalWeighting;
        for (int biomeIndex = 0; biomeIndex < generationData.Config.NumBiomes; ++biomeIndex)
        {
            int numEntries = Mathf.RoundToInt(numSeedPoints * generationData.Config.Biomes[biomeIndex].Weighting / totalBiomeWeighting);

            for (int entryIndex = 0; entryIndex < numEntries; ++entryIndex)
            {
                biomesToSpawn.Add((byte)biomeIndex);
            }
        }

        // spawn the individual biomes
        while (biomesToSpawn.Count > 0)
        {
            // pick a random seed point
            int seedPointIndex = generationData.Random(0, biomesToSpawn.Count);

            // extract the biome index
            byte biomeIndex = biomesToSpawn[seedPointIndex];

            // remove seed point
            biomesToSpawn.RemoveAt(seedPointIndex);

            Perform_SpawnIndividualBiome(generationData, biomeIndex, mapResolution);
        }

#if UNITY_EDITOR
        // save out the biome map
        Texture2D biomeMapTexture = new Texture2D(mapResolution, mapResolution, TextureFormat.RGB24, false);
        for (int y = 0; y < mapResolution; ++y)
        {
            for (int x = 0; x < mapResolution; ++x)
            {
                float hue = ((float)BiomeMap_LowResolution[x, y] / (float)generationData.Config.NumBiomes);

                biomeMapTexture.SetPixel(x, y, Color.HSVToRGB(hue, 0.75f, 0.75f));
            }
        }
        biomeMapTexture.Apply();

        System.IO.File.WriteAllBytes("BiomeMap_LowResolution.png", biomeMapTexture.EncodeToPNG());
#endif // UNITY_EDITOR
    }

    Vector2Int[] NeighbourOffsets = new Vector2Int[] {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
    };

    /*
    Use Ooze based generation from here: https://www.procjam.com/tutorials/en/ooze/
    */
    void Perform_SpawnIndividualBiome(ProcGenManager.GenerationData generationData, byte biomeIndex, int mapResolution)
    {
        // cache biome config
        BiomeConfigSO biomeConfig = generationData.Config.Biomes[biomeIndex].Biome;

        // pick spawn location
        Vector2Int spawnLocation = new Vector2Int(generationData.Random(0, mapResolution), generationData.Random(0, mapResolution));

        // pick the starting intensity
        float startIntensity = generationData.Random(biomeConfig.MinIntensity, biomeConfig.MaxIntensity);

        // setup working list
        Queue<Vector2Int> workingList = new Queue<Vector2Int>();
        workingList.Enqueue(spawnLocation);

        // setup the visted map and target intensity map
        bool[,] visited = new bool[mapResolution, mapResolution];
        float[,] targetIntensity = new float[mapResolution, mapResolution];

        // set the starting intensity
        targetIntensity[spawnLocation.x, spawnLocation.y] = startIntensity;

        // let the oozing begin
        while (workingList.Count > 0)
        {
            Vector2Int workingLocation = workingList.Dequeue();

            // set the biome
            BiomeMap_LowResolution[workingLocation.x, workingLocation.y] = biomeIndex;
            visited[workingLocation.x, workingLocation.y] = true;
            BiomeStrengths_LowResolution[workingLocation.x, workingLocation.y] = targetIntensity[workingLocation.x, workingLocation.y];

            // traverse the neighbours
            for (int neighbourIndex = 0; neighbourIndex < NeighbourOffsets.Length; ++neighbourIndex)
            {
                Vector2Int neighbourLocation = workingLocation + NeighbourOffsets[neighbourIndex];

                // skip if invalid
                if (neighbourLocation.x < 0 || neighbourLocation.y < 0 || neighbourLocation.x >= mapResolution || neighbourLocation.y >= mapResolution)
                    continue;

                // skip if visited
                if (visited[neighbourLocation.x, neighbourLocation.y])
                    continue;

                // flag as visited
                visited[neighbourLocation.x, neighbourLocation.y] = true;

                // work out and store neighbour strength;
                float decayAmount = generationData.Random(biomeConfig.MinDecayRate, biomeConfig.MaxDecayRate) * NeighbourOffsets[neighbourIndex].magnitude;
                float neighbourStrength = targetIntensity[workingLocation.x, workingLocation.y] - decayAmount;
                targetIntensity[neighbourLocation.x, neighbourLocation.y] = neighbourStrength;

                // if the strength is too low - stop
                if (neighbourStrength <= 0)
                {
                    continue;
                }

                workingList.Enqueue(neighbourLocation);
            }
        }
    }

    byte CalculateHighResBiomeIndex(int lowResMapSize, int lowResX, int lowResY, float fractionX, float fractionY)
    {
        float A = BiomeMap_LowResolution[lowResX, lowResY];
        float B = (lowResX + 1) < lowResMapSize ? BiomeMap_LowResolution[lowResX + 1, lowResY] : A;
        float C = (lowResY + 1) < lowResMapSize ? BiomeMap_LowResolution[lowResX, lowResY + 1] : A;
        float D = 0;

        if ((lowResX + 1) >= lowResMapSize)
            D = C;
        else if ((lowResY + 1) >= lowResMapSize)
            D = B;
        else
            D = BiomeMap_LowResolution[lowResX + 1, lowResY + 1];

        // perform bilinear filtering
        float filteredIndex = A * (1 - fractionX) * (1 - fractionY) + B * fractionX * (1 - fractionY) *
                              C * fractionY * (1 - fractionX) + D * fractionX * fractionY;

        // build an array of the possible biomes based on the values used to interpolate
        float[] candidateBiomes = new float[] { A, B, C, D };

        // find the neighbouring biome closest to the interpolated biome
        float bestBiome = -1f;
        float bestDelta = float.MaxValue;
        for (int biomeIndex = 0; biomeIndex < candidateBiomes.Length; ++biomeIndex)
        {
            float delta = Mathf.Abs(filteredIndex - candidateBiomes[biomeIndex]);

            if (delta < bestDelta)
            {
                bestDelta = delta;
                bestBiome = candidateBiomes[biomeIndex];
            }
        }

        return (byte)Mathf.RoundToInt(bestBiome);
    }

    void Perform_BiomeGeneration_HighResolution(ProcGenConfigSO config, int lowResMapSize, int highResMapSize, byte[,] biomeMap, float[,] biomeStrengths)
    {
        // calculate map scale
        float mapScale = (float)lowResMapSize / (float)highResMapSize;

        // calculate the high res map
        for (int y = 0; y < highResMapSize; ++y)
        {
            int lowResY = Mathf.FloorToInt(y * mapScale);
            float yFraction = y * mapScale - lowResY;

            for (int x = 0; x < highResMapSize; ++x)
            {
                int lowResX = Mathf.FloorToInt(x * mapScale);
                float xFraction = x * mapScale - lowResX;

                biomeMap[x, y] = CalculateHighResBiomeIndex(lowResMapSize, lowResX, lowResY, xFraction, yFraction);

                // this would do no interpolation - ie. point based
                //BiomeMap[x, y] = BiomeMap_LowResolution[lowResX, lowResY];
            }
        }

#if UNITY_EDITOR
        // save out the biome map
        Texture2D biomeMapTexture = new Texture2D(highResMapSize, highResMapSize, TextureFormat.RGB24, false);
        for (int y = 0; y < highResMapSize; ++y)
        {
            for (int x = 0; x < highResMapSize; ++x)
            {
                float hue = ((float)biomeMap[x, y] / (float)config.NumBiomes);

                biomeMapTexture.SetPixel(x, y, Color.HSVToRGB(hue, 0.75f, 0.75f));
            }
        }
        biomeMapTexture.Apply();

        System.IO.File.WriteAllBytes("BiomeMap_HighResolution.png", biomeMapTexture.EncodeToPNG());
#endif // UNITY_EDITOR
    }
}
