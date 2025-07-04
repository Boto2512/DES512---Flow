using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.VFX;

public class FirstPersonCameraController : MonoBehaviour {

    [SerializeField] private PlayerCameraConfig Config;
    private IHasSpeedThresholds thresholdHolder;
    private IMomentumModifiable momentumHolder;
    private SpeedStageThreshold currentThreshold => thresholdHolder.CurrentThreshold;
    private float currentFOV;


    #region Cinemachine Game Objects

    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineInputAxisController inputAxisController;

    #endregion Cinemachine Game Object

    [SerializeField] private VisualEffect runningLines;

    public Transform Orientation => cinemachineCamera.transform;

    private void Awake() {

        thresholdHolder = this.GetComponent<IHasSpeedThresholds>();
        momentumHolder = this.GetComponent<IMomentumModifiable>();
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;

        UpdateSensitivity();

        currentFOV = currentThreshold.FieldOfView;
        Debug.LogWarning($"currentFOV {currentFOV}");
    }

    // Update is called once per frame
    void Update() {
        FovChanges();
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

    private void FovChanges() {
        float speed = momentumHolder.GetMomentum().magnitude;

        float targetFOV = currentThreshold.FieldOfView;
        currentFOV = Mathf.MoveTowards(currentFOV, targetFOV, Config.fovChangeSpeed * Time.deltaTime);
        Debug.Log($"targetFOV {targetFOV}\n currentFOV {currentFOV}");
        cinemachineCamera.Lens.FieldOfView = currentFOV;
    }
}
