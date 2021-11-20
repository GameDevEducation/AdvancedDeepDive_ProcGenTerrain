using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProcGenDebugUI : MonoBehaviour
{
    [SerializeField] Button RegenerateButton;
    [SerializeField] TextMeshProUGUI StatusDisplay;
    [SerializeField] ProcGenManager TargetManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRegenerate()
    {
        RegenerateButton.interactable = false;

        StartCoroutine(PerformRegeneration());
    }

    IEnumerator PerformRegeneration()
    {
        yield return TargetManager.AsyncRegenerateWorld(OnStatusReported);

        RegenerateButton.interactable = true;

        yield return null;
    }

    void OnStatusReported(int step, int totalSteps, string status)
    {
        StatusDisplay.text = $"Step {step} of {totalSteps}: {status}";
    }
}
