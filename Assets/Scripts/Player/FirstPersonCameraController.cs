using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.VFX;

public class FirstPersonCameraController : MonoBehaviour {

    [SerializeField] private PlayerCameraConfig Config;
    private IHasSpeedThresholds thresholdHolder;
    private IMomentumModifiable momentumHolder;
    private SpeedStageThreshold currentThreshold => thresholdHolder.CurrentThreshold;
    private float currentFOV;
    private float previousTargetFOV;
    private float fovProgress;

    private SpeedStageThreshold prevThreshold;

    #region Cinemachine Game Objects

    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineInputAxisController inputAxisController;

    #endregion Cinemachine Game Object

    [SerializeField] private VisualEffect speedlines;

    public Transform Orientation => cinemachineCamera.transform;

    private void Awake() {

        thresholdHolder = this.GetComponent<IHasSpeedThresholds>();
        momentumHolder = this.GetComponent<IMomentumModifiable>();
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;

        UpdateSensitivity();

        currentFOV = currentThreshold.FieldOfView;
        previousTargetFOV = currentFOV;

        prevThreshold = thresholdHolder.CurrentThreshold;
    }

    // Update is called once per frame
    void Update() {
        FovChanges();
        SpeedlineChanges();
    }

    public void UpdateSensitivity() {
        var pan = inputAxisController.Controllers[0].Input;
        var tilt = inputAxisController.Controllers[1].Input;

        pan.Gain = (Config.InvertHorizontalInput ? -1 : 1) * Config.VerticalSensitivity / 2f;
        tilt.Gain = (Config.InvertVerticalInput ? 1 : -1) * Config.VerticalSensitivity / 2f;

        Debug.Log("03  updated setings aiua-");
        // vertical input is negated by default, so inversion makes it positive
    }

    #region Visual Effects

    private void SpeedlineChanges() {
        if (speedlines == null)
            return;


        if (currentThreshold != prevThreshold) {
            UpdateSpeedlineIntensity(currentThreshold.HasSpeedLines, currentThreshold.SpeedRangeOfLines, currentThreshold.SpeedlineSpawnRate);
#if DEBUG
            //Debug.Log($"Going from speed threshold {prevThreshold.SpeedThreshold}, speedlines: {prevThreshold.HasSpeedLines}\n" +
            //    $"to speed threshold {currentThreshold.SpeedThreshold}, speedlines: {currentThreshold.HasSpeedLines}");
#endif
        }
        prevThreshold = currentThreshold;
    }

    /// <summary>
    /// Controls the intensity of the running lines visual effect
    /// </summary>
    /// <param name="speedOfLines"> how fast the speedlines will go </param>
    /// <param name="spawnRate"> how fast speed lines will spawn </param>
    private void UpdateSpeedlineIntensity(bool hasSpeedlines, Vector2 speedOfLines, float spawnRate) {
        speedlines.enabled = hasSpeedlines;
        if (speedlines.HasVector2("SpeedOfLines")) {
            speedlines.SetVector2("SpeedOfLines", speedOfLines);
        }
        if (speedlines.HasFloat("SpawnRate")) {
            speedlines.SetFloat("SpawnRate", spawnRate);
        }
    }

    #endregion Visual Effects

    private void FovChanges() {

        float targetFOV = currentThreshold.FieldOfView;
        if (targetFOV != previousTargetFOV) { fovProgress = 0; }

        if (currentFOV != targetFOV) {

            currentFOV = Mathf.Lerp(currentFOV, targetFOV, fovProgress);
            fovProgress += Config.fovChangeSpeed * Time.deltaTime;
        }
        else if (fovProgress == 1) {
            fovProgress = 0.0f;
        }
        cinemachineCamera.Lens.FieldOfView = currentFOV;
        previousTargetFOV = targetFOV;
    }
}
