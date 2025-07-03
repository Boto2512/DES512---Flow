using Unity.Cinemachine;
using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour {

    [SerializeField] private PlayerCameraConfig Config;

    #region Cinemachine Game Objects

    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineInputAxisController inputAxisController;

    #endregion Cinemachine Game Objects

    public Transform Orientation => cinemachineCamera.transform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        UpdateSensitivity();
    }

    // Update is called once per frame
    void Update() {

    }

    private void UpdateSensitivity() {
        var pan = inputAxisController.Controllers[0].Input;
        var tilt = inputAxisController.Controllers[1].Input;

        pan.Gain = (Config.InvertHorizontalInput ? -1 : 1) * Config.VerticalSensitivity / 2f;
        tilt.Gain = (Config.InvertVerticalInput ? 1 : -1) * Config.VerticalSensitivity / 2f;
        // vertical input is negated by default, so inversion makes it positive
    }
}
