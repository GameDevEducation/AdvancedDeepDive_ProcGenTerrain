using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainDetailConfig : IEquatable<TerrainDetailConfig>
{
    [Header("Grass Billboard Configuration")]
    public Texture2D BillboardTexture;
    public Color HealthyColour = new Color(67f / 255f, 83f / 85f, 14f / 85f, 1f);
    public Color DryColour = new Color(41f / 51f, 188f / 255f, 26f / 255f, 1f);

    [Header("Detail Mesh Configuration")]
    public GameObject DetailPrefab;

    [Header("Common Configuration")]
    public float MinWidth = 1f;
    public float MaxWidth = 2f;
    public float MinHeight = 1f;
    public float MaxHeight = 2f;

    public int NoiseSeed = 0;
    public float NoiseSpread = 0.1f;
    [Range(0f, 1f)] public float HoleEdgePadding = 0f;

    public override bool Equals(object obj)
    {
        return Equals(obj as TerrainDetailConfig);
    }

    public bool Equals(TerrainDetailConfig other)
    {
        return other is not null &&
               EqualityComparer<Texture2D>.Default.Equals(BillboardTexture, other.BillboardTexture) &&
               HealthyColour.Equals(other.HealthyColour) &&
               DryColour.Equals(other.DryColour) &&
               EqualityComparer<GameObject>.Default.Equals(DetailPrefab, other.DetailPrefab) &&
               MinWidth == other.MinWidth &&
               MaxWidth == other.MaxWidth &&
               MinHeight == other.MinHeight &&
               MaxHeight == other.MaxHeight &&
               NoiseSeed == other.NoiseSeed &&
               NoiseSpread == other.NoiseSpread &&
               HoleEdgePadding == other.HoleEdgePadding;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(BillboardTexture);
        hash.Add(HealthyColour);
        hash.Add(DryColour);
        hash.Add(DetailPrefab);
        hash.Add(MinWidth);
        hash.Add(MaxWidth);
        hash.Add(MinHeight);
        hash.Add(MaxHeight);
        hash.Add(NoiseSeed);
        hash.Add(NoiseSpread);
        hash.Add(HoleEdgePadding);
        return hash.ToHashCode();
    }
}

public class BaseDetailPainter : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] protected float Strength = 1f;

    public virtual void Execute(ProcGenManager manager, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, List<int[,]> detailLayerMaps, int detailMapResolution, int maxDetailsPerPatch, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        Debug.LogError("No implementation of Execute function for " + gameObject.name);
    }

    public virtual List<TerrainDetailConfig> RetrieveTerrainDetails()
    {
        return null;
    }
}
