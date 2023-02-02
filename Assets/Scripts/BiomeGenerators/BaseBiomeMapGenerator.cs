using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBiomeMapGenerator : MonoBehaviour
{
    public virtual void Execute(ProcGenManager.GenerationData generationData)
    {
        Debug.LogError("No implementation of Execute function for " + gameObject.name);
    }
}
