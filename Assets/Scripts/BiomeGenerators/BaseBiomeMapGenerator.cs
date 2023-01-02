using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBiomeMapGenerator : MonoBehaviour
{
    public virtual void Execute(ProcGenConfigSO globalConfig, int mapResolution, byte[,] biomeMap, float[,] biomeStrengths)
    {
        Debug.LogError("No implementation of Execute function for " + gameObject.name);
    }
}
