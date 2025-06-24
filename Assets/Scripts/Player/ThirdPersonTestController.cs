using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonTestController : MonoBehaviour {

    #region Movement Variables

    [Header("Movement")]
    [SerializeField, Min(0f)] private float acceleration;
    [SerializeField, Min(0f)] private float airAccelerationMultiplier;
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
    private float coyoteTimer = 0f;

    private bool canJump => !hasJumped && (!inAir || coyoteTimer <= coyoteTime);
    private bool hasJumped = false;

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

    private PlayerGroundedState groundedState = PlayerGroundedState.None;
    private PlayerGroundedState prevGroundedState = PlayerGroundedState.None;
    private bool enableGroundedStateCheck = true;
    private bool inAir => groundedState == PlayerGroundedState.InAir;

    #endregion Ground & Slope Check

    #region Camera Variables

    [Header("Camera Settings")]
    [SerializeField, Range(0f, 15f)] private float cameraDistance = 3f;
    [SerializeField, Range(1, 120)] private int sensitivity = 60;
    private CinemachineInputAxisController inputAxisController;
    private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineDeoccluder deoccluder;
    private CinemachineCamera ccamera;
    private GameObject cameraObject;

    #endregion Camera Variables

    #region Input Variables

    private Vector2 movementInput;
    private bool attackPressed;
    private bool throwPressed;
    private bool jumpActivated;

    #endregion Input Variables

    private Rigidbody rb;
    private GameObject model;

    private void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        if (groundMask == 0) {
            groundMask = Globals.GROUND_MASK;
        }

        groundCheckTransform = this.transform.Find("Model").Find("GroundCheck");        // TODO: make this more general
        cameraObject = this.GetComponentInChildren<Camera>().gameObject;
        ccamera = this.GetComponentInChildren<CinemachineCamera>();

        orbitalFollow = this.GetComponentInChildren<CinemachineOrbitalFollow>();

        deoccluder = this.GetComponentInChildren<CinemachineDeoccluder>();
        deoccluder.CollideAgainst = Globals.OBSTACLE_MASK;
        deoccluder.TransparentLayers = ~Globals.OBSTACLE_MASK;

        inputAxisController = this.GetComponentInChildren<CinemachineInputAxisController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        rb = this.GetComponent<Rigidbody>();
        model = this.transform.Find("Model").gameObject;

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

        CoyoteUpdate();

        model.transform.rotation = Quaternion.Euler(0f, cameraObject.transform.rotation.eulerAngles.y, 0f);
        ccamera.Target.TrackingTarget.rotation = ccamera.transform.rotation;
        prevGroundedState = groundedState;
    }

    private void FixedUpdate() {
        Rotation();
        Movement();
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision == null)
            return;

        GameObject gameObject = collision.gameObject;
        if (gameObject == null)
            return;

        if (!enableGroundedStateCheck && Utility.DoesMaskContainLayer(groundMask, gameObject.layer)) {
            enableGroundedStateCheck = true;
        }
    }

    private void OnValidate() {
        this.GetComponentInChildren<CinemachineOrbitalFollow>().Radius = cameraDistance;
        this.GetComponentInChildren<CinemachineDeoccluder>().AvoidObstacles.DistanceLimit = cameraDistance;

        var tempInputAxisController = this.GetComponentInChildren<CinemachineInputAxisController>();
        tempInputAxisController.Controllers[0].Input.Gain = (1f / 75f) * sensitivity;
        tempInputAxisController.Controllers[1].Input.Gain = -(1f / 75f) * sensitivity;

        this.GetComponent<Rigidbody>().linearDamping = groundDrag;
    }

    private void OnDrawGizmos() {
        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(model.transform.position, model.transform.position + model.transform.forward);
        //Gizmos.DrawLine(model.transform.position, model.transform.position + ccamera.transform.forward);
    }

    #region Movement

    private void Movement() {
        // need to normalise movement since MoveInput() accumulates inputs
        movementInput = movementInput.normalized;

        VerticalMovement();
        HorizontalMovement();
    }

    private void VerticalMovement() {
        if (jumpActivated && canJump) {
            Jump();
            hasJumped = true;
        }
    }

    private void HorizontalMovement() {
        if (movementInput.x != 0) {
            float forwardMovement = 100 * movementInput.x * acceleration * Time.fixedDeltaTime;

            switch (groundedState) {
                case PlayerGroundedState.InAir:
                    forwardMovement *= airAccelerationMultiplier;
                    break;

                case PlayerGroundedState.OnSlope:
                    forwardMovement *= slopeSpeedMultiplier;
                    break;
            }

            rb.AddForce(forwardMovement * movementRight, ForceMode.Force);

        }

        if (movementInput.y != 0) {
            float rightMovement = 100 * movementInput.y * acceleration * Time.fixedDeltaTime;

            switch (groundedState) {
                case PlayerGroundedState.InAir:
                    rightMovement *= airAccelerationMultiplier;
                    break;

                case PlayerGroundedState.OnSlope:
                    rightMovement *= slopeSpeedMultiplier;
                    break;
            }

            rb.AddForce(rightMovement * movementForward, ForceMode.Force);
        }
        else {

        }
    }

    #endregion Movement

    #region Rotation

    private void Rotation() {
        movementForward = cameraObject.transform.forward.Horizontal().normalized;
        movementRight = Vector3.Cross(Vector3.up, movementForward).normalized;
    }

    #endregion Rotation

    #region Jump

    private void Jump() {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    private void CoyoteUpdate() {
        if (groundedState != PlayerGroundedState.InAir)
            return;

        if (coyoteTimer < coyoteTime) {
            coyoteTimer += Time.deltaTime;
        }
    }

    #endregion Jump

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
        if (groundedState == prevGroundedState)     // no transition necessary
            return;

        switch (groundedState) {
            case PlayerGroundedState.None:
                rb.linearDamping = groundDrag;
                hasJumped = false;
                break;

            case PlayerGroundedState.OnGround:
                rb.linearDamping = groundDrag;
                hasJumped = false;
                break;

            case PlayerGroundedState.OnSlope:
                rb.linearDamping = groundDrag;
                hasJumped = false;
                break;

            case PlayerGroundedState.InAir:
                rb.linearDamping = 0f;
                coyoteTimer = 0f;

                enableGroundedStateCheck = false;
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
