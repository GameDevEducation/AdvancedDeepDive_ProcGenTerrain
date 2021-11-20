using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FeatureConfig
{
    public Texture2D HeightMap;
    public float Height;
    public int Radius;
    public int NumToSpawn = 1;
}

public class HeightMapModifier_Features : BaseHeightMapModifier
{
    [SerializeField] List<FeatureConfig> Features;

    protected void SpawnFeature(FeatureConfig feature, int spawnX, int spawnY,
                                int mapResolution, float[,] heightMap, Vector3 heightmapScale)
    {
        float averageHeight = 0f;
        int numHeightSamples = 0;

        // sum the height values under the feature
        for (int y = -feature.Radius; y <= feature.Radius; ++y)
        {
            for (int x = -feature.Radius; x <= feature.Radius; ++x)
            {
                // sum the heightmap values
                averageHeight += heightMap[x + spawnX, y +spawnY];
                ++numHeightSamples;
            }
        }

        // calculate the average height
        averageHeight /= numHeightSamples;

        float targetHeight = averageHeight + (feature.Height / heightmapScale.y);

        // apply the feature
        for (int y = -feature.Radius; y <= feature.Radius; ++y)
        {
            int workingY = y + spawnY;
            float textureY = Mathf.Clamp01((float)(y + feature.Radius) / (feature.Radius * 2f));
            for (int x = -feature.Radius; x <= feature.Radius; ++x)
            {
                int workingX = x + spawnX;
                float textureX = Mathf.Clamp01((float)(x + feature.Radius) / (feature.Radius * 2f));

                // sample the height map
                var pixelColour = feature.HeightMap.GetPixelBilinear(textureX, textureY);
                float strength = pixelColour.r;

                // blend based on strength
                heightMap[workingX, workingY] = Mathf.Lerp(heightMap[workingX, workingY], targetHeight, strength);
            }
        }
    }

    public override void Execute(ProcGenConfigSO globalConfig, int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        // traverse the features
        foreach(var feature in Features)
        {
            for (int featureIndex = 0; featureIndex < feature.NumToSpawn; ++ featureIndex)
            {
                int spawnX = Random.Range(feature.Radius, mapResolution - feature.Radius);
                int spawnY = Random.Range(feature.Radius, mapResolution - feature.Radius);

                SpawnFeature(feature, spawnX, spawnY, mapResolution, heightMap, heightmapScale);
            }
        }
    }
}
