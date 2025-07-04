using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.VFX;

public class FirstPersonCameraController : MonoBehaviour {

    [SerializeField] private PlayerCameraConfig Config;

    #region Cinemachine Game Objects

    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineInputAxisController inputAxisController;

    #endregion Cinemachine Game Object

    [SerializeField] private VisualEffect runningLines;

    private IHasSpeedThresholds thresholdHolder;

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

    #region Visual Effects

    /// <summary>
    /// Controls the intensity of the running lines visual effect
    /// </summary>
    /// <param name="speedOfLines"> how fast the speedlines will go </param>
    /// <param name="spawnRate"> how fast speed lines will spawn </param>
    private void RunningLinesIntensity(Vector2 speedOfLines, float spawnRate) {
        runningLines.enabled = true;
        if (runningLines.HasVector2("SpeedOfLines")) {
            runningLines.SetVector2("SpeedOfLines", speedOfLines);
        }
        if (runningLines.HasFloat("SpawnRate")) {
            runningLines.SetFloat("SpawnRate", spawnRate);
        }
    }

    #endregion Visual Effects
}
