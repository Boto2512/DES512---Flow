using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ToggleRenderScript : MonoBehaviour
{
    [Header("Renderer Setup")]
    [Tooltip("Assign your Universal Renderer Data asset here")]
    public UniversalRendererData rendererData;

    [Tooltip("Exact name of the X-ray Render Feature (e.g., XRayPass)")]
    public string xrayFeatureName = "XrayRenderer";

    private ScriptableRendererFeature xrayFeature;

    void Awake()
    {
        if (rendererData == null)
        {
            Debug.LogError("XRayToggle: rendererData is not assigned!");
            return;
        }

        // Find the X-ray feature by name
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature != null && feature.name == xrayFeatureName)
            {
                xrayFeature = feature;
                break;
            }
        }

        if (xrayFeature == null)
        {
            Debug.LogError($"XRayToggle: Could not find a feature named '{xrayFeatureName}'!");
        }

        ToggleXRay(false);
    }
    private void Start()
    {
        ToggleXRay(false);
    }

    void Update()
    {
        if (xrayFeature == null)
            return;

        // Toggle with T key
        if (Input.GetKeyDown(KeyCode.T))
        {
            //ToggleXRay(!xrayFeature.isActive);
        }
    }

    public void ToggleXRay(bool active)
    {
            xrayFeature.SetActive(active);

    }
}
