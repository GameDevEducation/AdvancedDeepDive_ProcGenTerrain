using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TextureConfig : System.IEquatable<TextureConfig>
{
    public Texture2D Diffuse;
    public Texture2D NormalMap;

    public override bool Equals(object other)
    {
        return Equals(other as TextureConfig);
    }

    public bool Equals(TextureConfig other)
    {
        return other != null && other.Diffuse == Diffuse && other.NormalMap == NormalMap;
    }

    public override int GetHashCode()
    {
        int hash = 17;

        if (Diffuse != null)
            hash = hash * 23 + Diffuse.GetHashCode();
        if (NormalMap != null)
            hash = hash * 23 + NormalMap.GetHashCode();

        return hash;
    }
}

public class BaseTexturePainter : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] protected float Strength = 1f;

    public virtual void Execute(ProcGenManager manager, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        Debug.LogError("No implementation of Execute function for " + gameObject.name);
    }

    public virtual List<TextureConfig> RetrieveTextures()
    {
        return null;
    }
}
