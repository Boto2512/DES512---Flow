using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Settings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private PlayerCameraConfig cameraConfig;
    [Space]
    [SerializeField] private FirstPersonCameraController cameraController;
    [Space]
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider soundEffectVolume;
    [Space]
    [SerializeField] private Slider horizontalSensitivity;
    [SerializeField] private Slider verticalSensitivity;
    [SerializeField] private Toggle invertHorizontal;
    [SerializeField] private Toggle invertVertical;
    [Space]
    [SerializeField] private GameObject firstSelectedClose;

    public void Awake() {
        float mVolume;
        audioMixer.GetFloat("Master", out mVolume);
        mVolume = Mathf.Pow(10, mVolume/20f);
        masterVolume.value  = mVolume;


        audioMixer.GetFloat("Music", out mVolume);
        mVolume = Mathf.Pow(10, mVolume / 20f);
        musicVolume.value = mVolume;

        audioMixer.GetFloat("SFX", out mVolume);
        mVolume = Mathf.Pow(10, mVolume / 20f);
        soundEffectVolume.value = mVolume;

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
        if (cameraConfig != null) { 
            cameraConfig.InvertVerticalInput = isInverted;
            if (cameraController != null)
            {

                Debug.Log("Updating Settings");
                cameraController.UpdateSensitivity();
            }
        } 
    }
    public void SetVerticalSensititvity(float sensitivity) { 
        if (cameraConfig != null) {
            cameraConfig.VerticalSensitivity = sensitivity;
            if (cameraController != null)
            {

                Debug.Log("Updating Settings");
                cameraController.UpdateSensitivity();
            }
        } 
    }

    public void InvertHorizontal(bool isInverted) {

        if (cameraConfig != null) {
            cameraConfig.InvertHorizontalInput = isInverted;
            if (cameraController != null)
            {
                Debug.Log("Updating Settings");
                cameraController.UpdateSensitivity();
            }
        }
    }
    public void SetHorizontalSensititvity(float sensitivity) {
        if (cameraConfig != null) {
            cameraConfig.HorizontalSensitivity = sensitivity;
            if (cameraController != null) {
                Debug.Log("Updating Settings");
                    cameraController.UpdateSensitivity(); 
            }
        }
    }

    public void SetMasterVolume(float volume) { 
        audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20); 
        if(volume == 0)
        {
            audioMixer.SetFloat("Master", -80f);
        }
    
    }
    public void SetSoundEffectVolume(float volume) { audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        if (volume == 0)
        {
            audioMixer.SetFloat("SFX", -80f);
        }
    }
    public void SetMusicVolume(float volume) { audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20); 
        if (volume == 0)
        {
            audioMixer.SetFloat("Music", -80f);
        }
    }

    public void Close()
    {
        EventSystem.current.SetSelectedGameObject(firstSelectedClose);
        gameObject.SetActive(false);
    }

}