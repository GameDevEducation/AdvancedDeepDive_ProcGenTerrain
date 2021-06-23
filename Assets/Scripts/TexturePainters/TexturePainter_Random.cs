using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePainter_Random : BaseTexturePainter
{
    [SerializeField] List<TextureConfig> Textures;

    public override void Execute(ProcGenManager manager, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        for (int y = 0; y < alphaMapResolution; ++y)
        {
            int heightMapY = Mathf.FloorToInt((float)y * (float)mapResolution / (float)alphaMapResolution);

            for (int x = 0; x < alphaMapResolution; ++x)
            {
                int heightMapX = Mathf.FloorToInt((float)x * (float)mapResolution / (float)alphaMapResolution);

                // skip if we have a biome and this is not our biome
                if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex)
                    continue;

                var randomTexture = Textures[Random.Range(0, Textures.Count)];

                alphaMaps[x, y, manager.GetLayerForTexture(randomTexture)] = Strength;
            }
        }        
    }

    public override List<TextureConfig> RetrieveTextures()
    {
        return Textures;
    }       
}
