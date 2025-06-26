using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonTestController : MonoBehaviour {

    #region Momentum Storage

    [SerializeField, Min(0f)] private float momentumStorageTime;
    private readonly List<StoredMomentum> storedMomentums = new();
    private StoredMomentum fastestMomentum = new(Vector3.zero, 0f);

    [SerializeField, Min(0f)] private float slideTime;
    private bool slideEnded = true;

    private readonly struct StoredMomentum {
        public readonly Vector3 Momentum { get; }
        public readonly float TimeStamp { get; }

        public StoredMomentum(Vector3 Momentum, float TimeStamp) {
            this.Momentum = Momentum;
            this.TimeStamp = TimeStamp;
        }

        public void Deconstruct(out Vector3 momentum, out float timeStamp) {
            momentum = Momentum;
            timeStamp = TimeStamp;
        }

        public static StoredMomentum Compare(StoredMomentum a, StoredMomentum b) {
            float aSqrMag = a.Momentum.sqrMagnitude;
            float bSqrMag = b.Momentum.sqrMagnitude;
            if (aSqrMag == bSqrMag) {
                return a.TimeStamp > b.TimeStamp ? a : b;
            }
            else {
                return aSqrMag > bSqrMag ? a : b;
            }
        }
    }

    #endregion Momentum Storage

    #region Movement Variables

    [Header("Movement")]
    [SerializeField, Min(0f)] private float acceleration;
    [SerializeField, Min(0f)] private float airAcceleration;
    [SerializeField, Min(0f)] private float airDrag;
    [SerializeField, Min(0f)] private float groundDrag;
    [SerializeField, Min(0f)] private float noMovementDrag;

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
    private float timeSpentGrounded = 0f;
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

    #region Debug Variables

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI debugSpeedText;

    #endregion Debug Variables

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
    }

    // Update is called once per frame
    void Update() {
        GroundedStateCheck();
        GroundedStateUpdates();

        model.transform.rotation = Quaternion.Euler(0f, cameraObject.transform.rotation.eulerAngles.y, 0f);
        ccamera.Target.TrackingTarget.rotation = ccamera.transform.rotation;
        prevGroundedState = groundedState;

        debugSpeedText.text = $"Forward: {Vector3.Dot(rb.linearVelocity, movementForward):0.####}\n" +
            $"Right: {Vector3.Dot(rb.linearVelocity, movementRight):0.####}\n" +
            $"Up: {Vector3.Dot(rb.linearVelocity, Vector3.up):0.####}\n" +
            $"Overall: {rb.linearVelocity.magnitude:0.####}";
    }

    private void FixedUpdate() {
        Rotation();
        Movement();
        UpdateMomentumStorage();


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
        bool hasInputX = movementInput.x != 0;
        bool hasInputY = movementInput.y != 0;

        if (!hasInputX && !hasInputY) {     // no input
            if (!inAir) {                   // on ground
                if (slideEnded) {
                    rb.linearDamping = noMovementDrag;
                }
                else {
                    rb.linearDamping = 0f;
                }
            }
            else {
                rb.linearDamping = 0f;
            }
        }
        else if (hasInputX || hasInputY) {
            rb.linearDamping = inAir || !slideEnded ? airDrag : groundDrag;

            Vector3 worldMovement = GenerateWorldMovement();
            rb.AddForce(worldMovement, ForceMode.Force);
        }
    }

    private Vector3 GenerateWorldMovement() {
        Vector3 relativeMovement = new(movementInput.y, 0f, movementInput.x);
        relativeMovement.Normalize();
        relativeMovement *= inAir ? airAcceleration : acceleration;

        relativeMovement *= GetGroundedStateMovementModifier();

        Vector3 worldMovement = new(Vector3.Dot(relativeMovement, movementForward), 0f, Vector3.Dot(relativeMovement, movementRight));
        return worldMovement;
    }

    #endregion Movement

    #region Momentum Storage

    private void UpdateMomentumStorage() {
        StoredMomentum currentMomentumToStore = new(rb.linearVelocity, Time.fixedTime);

        if (fastestMomentum.Momentum.sqrMagnitude < currentMomentumToStore.Momentum.sqrMagnitude) {
            fastestMomentum = currentMomentumToStore;
            storedMomentums.Clear();

            storedMomentums.Add(currentMomentumToStore);
        }
        else {
            storedMomentums.Add(currentMomentumToStore);

            float earliestAllowedTimeStamp = currentMomentumToStore.TimeStamp - momentumStorageTime;
            bool anyRemoved = false;

            while (storedMomentums.Any()) {
                if (storedMomentums.First().TimeStamp < earliestAllowedTimeStamp) {
                    storedMomentums.RemoveAt(0);
                    anyRemoved = true;
                }
                else {
                    break;
                }
            }

            if (anyRemoved) {
                fastestMomentum = storedMomentums.Aggregate((sm1, sm2) => StoredMomentum.Compare(sm1, sm2));
            }
        }
    }

    #endregion Momentum Storage

    #region Rotation

    private void Rotation() {
        movementForward = cameraObject.transform.forward.Horizontal().normalized;
        movementRight = Vector3.Cross(Vector3.up, movementForward).normalized;
    }

    #endregion Rotation

    #region Jump

    private void Jump() {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void CoyoteUpdate() {
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
        if (groundedState == prevGroundedState) {
            GroundedStateConserved();
        }
        else {
            GroundedStateChanged();
        }
    }

    private void GroundedStateChanged() {
        switch (groundedState) {
            case PlayerGroundedState.None:
                break;

            case PlayerGroundedState.OnGround:
                slideEnded = false;
                this.InvokeExclusive("End Slide", () => { /*rb.linearDamping = groundDrag;*/ slideEnded = true; }, slideTime);
                hasJumped = false;
                break;

            case PlayerGroundedState.OnSlope:
                slideEnded = false;
                this.InvokeExclusive("End Slide", () => { /*rb.linearDamping = groundDrag;*/ slideEnded = true; }, slideTime);
                hasJumped = false;
                break;

            case PlayerGroundedState.InAir:
                rb.linearDamping = airDrag;
                coyoteTimer = 0f;
                timeSpentGrounded = 0f;

                enableGroundedStateCheck = false;
                this.InvokeCancel("End Slide");
                slideEnded = true;
                break;

            default:
                break;
        }
    }

    private void GroundedStateConserved() {
        switch (groundedState) {
            case PlayerGroundedState.None:
                break;

            case PlayerGroundedState.OnGround:
                timeSpentGrounded += Time.deltaTime;
                break;

            case PlayerGroundedState.OnSlope:
                timeSpentGrounded += Time.deltaTime;
                break;

            case PlayerGroundedState.InAir:
                CoyoteUpdate();
                break;

            default:
                break;
        }
    }

    private float GetGroundedStateMovementModifier() {
        return groundedState switch {
            PlayerGroundedState.OnSlope => slopeSpeedMultiplier,
            _ => 1f
        };
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