using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BiomeTexture
{
    public string UniqueID;
    public Texture2D Diffuse;
    public Texture2D NormalMap;
}

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

    public List<BiomeTexture> Textures;
}
