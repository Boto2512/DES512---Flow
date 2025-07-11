using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private PlayerCameraConfig cameraConfig;
    [Space]
    [SerializeField] private FirstPersonCameraController cameraController;
    [Space]
    [SerializeField] private Slider horizontalSensitivity;
    [SerializeField] private Slider verticalSensitivity;
    [SerializeField] private Toggle invertHorizontal;
    [SerializeField] private Toggle invertVertical;


    public void Awake() {
        horizontalSensitivity.value = cameraConfig.HorizontalSensitivity;
        verticalSensitivity.value = cameraConfig.VerticalSensitivity;

        invertHorizontal.isOn = cameraConfig.InvertHorizontalInput;
        invertVertical.isOn = cameraConfig.InvertVerticalInput;
    }
    private void Start()
    {
        if (FindFirstObjectByType<FirstPersonCameraController>()) {
            cameraController = FindFirstObjectByType<FirstPersonCameraController>();
        }
    }

    public void InvertVertical(bool isInverted) {
        Debug.Log($"Inverting Vertical = {isInverted}");
        if (cameraConfig != null) { 
            cameraConfig.InvertVerticalInput = isInverted;
            if (cameraController != null)
            {
                cameraController.UpdateSensitivity();
            }
        } 
    }
    public void SetVerticalSensititvity(float sensitivity) { 
        if (cameraConfig != null) {
            cameraConfig.VerticalSensitivity = sensitivity;
            if (cameraController != null)
            {
                cameraController.UpdateSensitivity();
            }
        } 
    }

    public void InvertHorizontal(bool isInverted) {

        if (cameraConfig != null) {
            cameraConfig.InvertHorizontalInput = isInverted;
            if (cameraController != null)
            {
                cameraController.UpdateSensitivity();
            }
        }
    }
    public void SetHorizontalSensititvity(float sensitivity) {
        if (cameraConfig != null) {
            cameraConfig.HorizontalSensitivity = sensitivity;
            if (cameraController != null) { 
                    cameraController.UpdateSensitivity(); 
            }
        }
    }
    
    public void Close()
    {
        gameObject.SetActive(false);
    }

}