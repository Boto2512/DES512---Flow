using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonTestController : MonoBehaviour {

    #region Cinemachine Variables

    [SerializeField, Range(1, 120)] private int sensitivity = 60;
    private CinemachineInputAxisController inputAxisController;
    private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineDeoccluder deoccluder;

    #endregion Cinemachine Variables

    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool attackPressed;
    private bool throwPressed;
    private bool jumpInput;

    private Rigidbody rb;

    private void Awake() {
        Cursor.lockState = CursorLockMode.Locked;

        rb = this.GetComponent<Rigidbody>();

        orbitalFollow = this.GetComponentInChildren<CinemachineOrbitalFollow>();
        if (orbitalFollow == null) Debug.Log("orbital follow is null");

        deoccluder = this.GetComponentInChildren<CinemachineDeoccluder>();
        if (deoccluder == null) Debug.Log("deoccluder follow is null");
        deoccluder.CollideAgainst = Globals.OBSTACLE_MASK;
        deoccluder.TransparentLayers = ~Globals.OBSTACLE_MASK;

        inputAxisController = this.GetComponentInChildren<CinemachineInputAxisController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        UpdateSensitivity();
        deoccluder.AvoidObstacles.DistanceLimit = orbitalFollow.Radius;
    }

    // Update is called once per frame
    void Update() {
        
    }

    private void OnValidate() {
        if (Application.isPlaying) {
            UpdateSensitivity();
        }
    }

    #region Input

    public void Move(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context) {

        //cameraStand.Rotate(lookInput.y * 0.02f * sensivity, lookInput.x * 0.02f * sensivity, 0f);
    }

    public void Attack(InputAction.CallbackContext context) {
        if (context.started) {
            attackPressed = true;
        }
        else if (context.canceled) {
            attackPressed = false;
        }
    }

    public void Throw(InputAction.CallbackContext context) {
        if (context.started) {
            throwPressed = true;
        }
        else if (context.canceled) {
            throwPressed = false;
        }
    }

    public void Jump(InputAction.CallbackContext context) {
        if (context.started) {
            jumpInput = true;
        }
        else if (context.canceled) {
            jumpInput = false;
        }
    }

    #endregion Input

    #region Sensitivity

    private void UpdateSensitivity() {
        if (inputAxisController == null)
            return;

        if (inputAxisController.Controllers == null)
            return;

        if (inputAxisController.Controllers.Count < 2)
            return;

        inputAxisController.Controllers[0].Input.Gain = (1f / 75f) * sensitivity;
        inputAxisController.Controllers[1].Input.Gain = -(1f / 75f) * sensitivity;
    }

    #endregion Sensitivity
}
