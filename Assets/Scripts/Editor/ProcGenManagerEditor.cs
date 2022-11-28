using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(ProcGenManager))]
public class ProcGenManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Regenerate Textures"))
        {
            ProcGenManager targetManager = serializedObject.targetObject as ProcGenManager;
            targetManager.RegenerateTextures();
        }

        if (GUILayout.Button("Regenerate Detail Prototypes"))
        {
            ProcGenManager targetManager = serializedObject.targetObject as ProcGenManager;
            targetManager.RegenerateDetailPrototypes();
        }

        if (GUILayout.Button("Regenerate World"))
        {
            ProcGenManager targetManager = serializedObject.targetObject as ProcGenManager;
            EditorCoroutineUtility.StartCoroutine(PerformRegeneration(targetManager), this);
        }
    }

    int ProgressID;
    IEnumerator PerformRegeneration(ProcGenManager targetManager)
    {
        ProgressID = Progress.Start("Regenerating terrain");

        yield return targetManager.AsyncRegenerateWorld(OnStatusReported);

        Progress.Remove(ProgressID);

        yield return null;
    }

    void OnStatusReported(EGenerationStage currentStage, string status)
    {
        Progress.Report(ProgressID, (int)currentStage, (int)EGenerationStage.NumStages, status);
    }
}
