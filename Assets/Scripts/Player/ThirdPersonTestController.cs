using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonTestController : MonoBehaviour {

    #region Movement Variables

    [Header("Movement")]
    [SerializeField, Min(0f)] private float acceleration;
    [SerializeField, Min(0f)] private float deceleration;
    [SerializeField, Min(0f)] private float groundDrag;
    private float constGroundDrag;

    private Vector3 movementForward;
    private Vector3 movementBackward => -movementForward;
    private Vector3 movementRight;
    private Vector3 movementLeft => -movementRight;

    #endregion Movement Variables

    #region Jump Variables

    [Header("Jump")]
    [SerializeField, Min(0f)] private float jumpForce;
    [SerializeField, Min(0f)] private float coyoteTime;

    #endregion Jump Variables

    #region Ground & Slope Check

    [Header("Ground & Slope Check")]
    [SerializeField, Min(0f)] private float groundCheckRange = 0.25f;
    [SerializeField] private LayerMask groundMask;
    private Transform groundCheckTransform;
    private Vector3 groundCheckPosition => groundCheckTransform.position;

    [SerializeField, Min(0f)] private float minSlopeAngle;
    [SerializeField, Min(0f)] private float maxSlopeAngle;
    [SerializeField, Min(0f)] private float slopeSpeedMultiplier;
    private float currentSlopeMultiplier = 1f;

    private PlayerGroundedState groundedState = PlayerGroundedState.None;
    private bool enableGroundedStateCheck = true;

    #endregion Ground & Slope Check

    #region Camera Variables

    [Header("Camera Settings")]
    [SerializeField, Range(0f, 15f)] private float cameraDistance = 3f;
    [SerializeField, Range(1, 120)] private int sensitivity = 60;
    private CinemachineInputAxisController inputAxisController;
    private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineDeoccluder deoccluder;
    private GameObject cameraObject;

    #endregion Camera Variables

    #region Input Variables

    private Vector2 movementInput;
    private bool attackPressed;
    private bool throwPressed;
    private bool jumpActivated;

    #endregion Input Variables

    private Rigidbody rb;

    private void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        if (groundMask == 0) {
            groundMask = Globals.GROUND_MASK;
        }

        groundCheckTransform = this.transform.Find("Model").Find("GroundCheck");        // TODO: make this more general
        cameraObject = this.GetComponentInChildren<Camera>().gameObject;

        orbitalFollow = this.GetComponentInChildren<CinemachineOrbitalFollow>();

        deoccluder = this.GetComponentInChildren<CinemachineDeoccluder>();
        deoccluder.CollideAgainst = Globals.OBSTACLE_MASK;
        deoccluder.TransparentLayers = ~Globals.OBSTACLE_MASK;

        inputAxisController = this.GetComponentInChildren<CinemachineInputAxisController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        rb = this.GetComponent<Rigidbody>();
        UpdateSensitivity();
        orbitalFollow.Radius = cameraDistance;
        deoccluder.AvoidObstacles.DistanceLimit = cameraDistance;

        rb.linearDamping = groundDrag;
        constGroundDrag = groundDrag;
    }

    // Update is called once per frame
    void Update() {
        GroundedStateCheck();
        GroundedStateUpdates();
    }

    private void FixedUpdate() {
        Rotation();
        Movement();
    }

    private void OnValidate() {
        this.GetComponentInChildren<CinemachineOrbitalFollow>().Radius = cameraDistance;
        this.GetComponentInChildren<CinemachineDeoccluder>().AvoidObstacles.DistanceLimit = cameraDistance;

        var tempInputAxisController = this.GetComponentInChildren<CinemachineInputAxisController>();
        tempInputAxisController.Controllers[0].Input.Gain = (1f / 75f) * sensitivity;
        tempInputAxisController.Controllers[1].Input.Gain = -(1f / 75f) * sensitivity;

        this.GetComponent<Rigidbody>().linearDamping = groundDrag;
    }

    #region Movement

    private void Movement() {
        // need to normalise movement since MoveInput() accumulates inputs
        movementInput = movementInput.normalized;

        HorizontalMovement();
        VerticalMovement();
    }

    private void HorizontalMovement() {
        if (movementInput.x != 0) {
            rb.AddForce(300 * currentSlopeMultiplier * movementInput.x * Time.fixedDeltaTime * acceleration * movementRight, ForceMode.Force);
        }
        else {

        }

        if (movementInput.y != 0) {
            rb.AddForce(300 * currentSlopeMultiplier * movementInput.y * Time.fixedDeltaTime * acceleration * movementForward, ForceMode.Force);
        }
        else {

        }
    }

    private void VerticalMovement() {

    }

    private void Accelerate() {

    }

    private void Decelerate() {

    }

    #endregion Movement

    #region Rotation

    private void Rotation() {
        movementForward = cameraObject.transform.forward.Horizontal().normalized;
        movementRight = Vector3.Cross(Vector3.up, movementForward).normalized;
    }

    #endregion Rotation

    #region Ground & Slopes

    private void GroundedStateCheck() {
        if (!enableGroundedStateCheck)
            return;

        if (Physics.Raycast(groundCheckPosition, Vector3.down, out RaycastHit hitInfo, groundCheckRange, groundMask, QueryTriggerInteraction.Collide)) {
            float angle = Vector3.Angle(Vector3.up, hitInfo.normal);
            groundedState = (angle < maxSlopeAngle && angle > minSlopeAngle) ? PlayerGroundedState.OnSlope : PlayerGroundedState.OnGround;
        }
        else {
            groundedState = PlayerGroundedState.InAir;
        }
    }

    private void GroundedStateUpdates() {
        switch (groundedState) {
            case PlayerGroundedState.None:
                groundDrag = constGroundDrag;
                currentSlopeMultiplier = 1f;
                break;

            case PlayerGroundedState.OnGround:
                groundDrag = constGroundDrag;
                currentSlopeMultiplier = 1f;
                break;

            case PlayerGroundedState.OnSlope:
                groundDrag = 0f;
                currentSlopeMultiplier = slopeSpeedMultiplier;
                break;

            case PlayerGroundedState.InAir:
                groundDrag = 0f;
                currentSlopeMultiplier = 1f;
                break;

            default:
                break;
        }
    }

    #endregion Ground & Slopes

    #region Input

    public void MoveInput(InputAction.CallbackContext context) {
        Vector2 tempInput = context.ReadValue<Vector2>();

        if (tempInput.x == 0) {
            movementInput.x = 0;
        }
        else {
            movementInput.x += tempInput.x;
        }

        if (tempInput.y == 0) {
            movementInput.y = 0;
        }
        else {
            movementInput.y += tempInput.y;
        }
    }

    public void LookInput(InputAction.CallbackContext context) {
        // do nothing
    }

    public void AttackInput(InputAction.CallbackContext context) {
        if (context.started) {
            attackPressed = true;
        }
        else if (context.canceled) {
            attackPressed = false;
        }
    }

    public void ThrowInput(InputAction.CallbackContext context) {
        if (context.started) {
            throwPressed = true;
        }
        else if (context.canceled) {
            throwPressed = false;
        }
    }

    public void JumpInput(InputAction.CallbackContext context) {
        if (context.started) {
            jumpActivated = true;
        }
        else if (context.canceled) {
            jumpActivated = false;
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
