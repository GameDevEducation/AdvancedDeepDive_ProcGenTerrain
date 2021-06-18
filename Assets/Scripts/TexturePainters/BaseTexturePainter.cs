using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTexturePainter : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] protected float Strength = 1f;

    public virtual void Execute(ProcGenManager manager, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        Debug.LogError("No implementation of Execute function for " + gameObject.name);
    }
}
