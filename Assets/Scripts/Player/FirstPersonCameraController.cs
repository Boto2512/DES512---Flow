using Unity.Cinemachine;
using UnityEditor;
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
    }

    // Update is called once per frame
    void Update() {
        FovChanges();
        SpeedlineChanges();
    }

    private void UpdateSensitivity() {
        var pan = inputAxisController.Controllers[0].Input;
        var tilt = inputAxisController.Controllers[1].Input;

        pan.Gain = (Config.InvertHorizontalInput ? -1 : 1) * Config.VerticalSensitivity / 2f;
        tilt.Gain = (Config.InvertVerticalInput ? 1 : -1) * Config.VerticalSensitivity / 2f;
        // vertical input is negated by default, so inversion makes it positive
    }

    #region Visual Effects
#if DEBUG
    private SpeedStageThreshold prevThreshold = null;
#endif
    private void SpeedlineChanges() {
        if (speedlines == null)
            return;

        UpdateSpeedlineIntensity(currentThreshold.HasSpeedLines, currentThreshold.SpeedRangeOfLines, currentThreshold.SpeedlineSpawnRate);

#if DEBUG
        if (prevThreshold != null &&
                currentThreshold != prevThreshold) {
            Debug.Log($"Going from speed threshold {prevThreshold.SpeedThreshold} " +
                $"to speed threshold {currentThreshold.SpeedThreshold}");
        }
        prevThreshold = currentThreshold;
#endif
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
        else if ( fovProgress == 1)
        { 
            fovProgress = 0.0f;
        }
        cinemachineCamera.Lens.FieldOfView = currentFOV;
        previousTargetFOV = targetFOV;
    }
}
