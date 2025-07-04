using Unity.Cinemachine;
using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour {

    [SerializeField] private PlayerCameraConfig Config;
    private IHasSpeedThresholds thresholdHolder;
    private IMomentumModifiable momentumHolder; 
    private SpeedStageThreshold currentThreshold => thresholdHolder.CurrentThreshold;
    private float currentFOV;


    #region Cinemachine Game Objects

    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineInputAxisController inputAxisController;

    #endregion Cinemachine Game Objects

    public Transform Orientation => cinemachineCamera.transform;

    private void Awake()
    {
        
        thresholdHolder = this.GetComponent<IHasSpeedThresholds>();
        momentumHolder = this.GetComponent<IMomentumModifiable>();
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;

        UpdateSensitivity();

        currentFOV = currentThreshold.FieldOfView;
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

    private void FovChanges() {
        float speed = momentumHolder.GetMomentum().magnitude;  

        float targetFOV = currentThreshold.FieldOfView;
        currentFOV = Mathf.MoveTowards(currentFOV, targetFOV, Config.fovChangeSpeed * Time.deltaTime);
        cinemachineCamera.Lens.FieldOfView = currentFOV;
    }
}
