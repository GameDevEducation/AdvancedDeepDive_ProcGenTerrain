using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome Config", menuName = "Procedural Generation/Biome Configuration", order = -1)]
public class BiomeConfigSO : ScriptableObject
{
    public string Name;

    [Range(0f, 1f)] public float MinIntensity = 0.5f;
    [Range(0f, 1f)] public float MaxIntensity = 1f;

    [Range(0f, 1f)] public float MinDecayRate = 0.01f;
    [Range(0f, 1f)] public float MaxDecayRate = 0.02f;

    public GameObject HeightModifier;
    public GameObject TerrainPainter;
    public GameObject ObjectPlacer;

    public List<TextureConfig> RetrieveTextures()
    {
        if (TerrainPainter == null)
            return null;

        // extract all textures from every painter
        List<TextureConfig> allTextures = new List<TextureConfig>();
        BaseTexturePainter[] allPainters = TerrainPainter.GetComponents<BaseTexturePainter>();
        foreach(var painter in allPainters)
        {
            var painterTextures = painter.RetrieveTextures();

            if (painterTextures == null || painterTextures.Count == 0)
                continue;

            allTextures.AddRange(painterTextures);
        }

        return allTextures;
    }
}
