using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour, IMomentumModifiable {

    [SerializeField] private PlayerMovementConfig Config;

    #region Momentum Storage [NOT IN USE RIGHT NOW]

    //[SerializeField, Min(0f)] private float momentumStorageTime;
    //private readonly List<StoredMomentum> storedMomentums = new();
    //private StoredMomentum fastestMomentum = new(Vector3.zero, 0f);


    //private readonly struct StoredMomentum {
    //    public readonly Vector3 Momentum { get; }
    //    public readonly float TimeStamp { get; }

    //    public StoredMomentum(Vector3 Momentum, float TimeStamp) {
    //        this.Momentum = Momentum;
    //        this.TimeStamp = TimeStamp;
    //    }

    //    public void Deconstruct(out Vector3 momentum, out float timeStamp) {
    //        momentum = Momentum;
    //        timeStamp = TimeStamp;
    //    }

    //    public static StoredMomentum Compare(StoredMomentum a, StoredMomentum b) {
    //        float aSqrMag = a.Momentum.sqrMagnitude;
    //        float bSqrMag = b.Momentum.sqrMagnitude;
    //        if (aSqrMag == bSqrMag) {
    //            return a.TimeStamp > b.TimeStamp ? a : b;
    //        }
    //        else {
    //            return aSqrMag > bSqrMag ? a : b;
    //        }
    //    }
    //}

    #endregion Momentum Storage
    private bool slideEnded = true;

    #region Movement Variables

    private Vector3 movementForward;
    private Vector3 movementBackward => -movementForward;
    private Vector3 movementRight;
    private Vector3 movementLeft => -movementRight;

    private float currentHighestAccelerationSquared = 0f;
    private bool waitingToDecelerate = false;
    private bool decelerating = false;

    private bool hasReportedMovement = false;

    #endregion Movement Variables

    #region Jump Variables

    private float coyoteTimer = 0f;
    private float fallTimer = 0f;
    private bool canJump => !hasJumped && (!inAir || coyoteTimer <= Config.CoyoteTime);
    private bool hasJumped = false;

    #endregion Jump Variables

    #region Ground & Slope Check

    [Header("Ground Check")]
    [SerializeField, Min(0f)] private float groundCheckRange = 0.25f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheckTransform;
    private Vector3 groundCheckPosition => groundCheckTransform.position;

    private PlayerGroundedState groundedState = PlayerGroundedState.None;
    private PlayerGroundedState prevGroundedState = PlayerGroundedState.None;
    private bool enableGroundedStateCheck = true;
    private float timeSpentGrounded = 0f;
    private bool inAir => groundedState == PlayerGroundedState.InAir;

    private Vector3 inverseSlopeNormal = Vector3.up;


    #endregion Ground & Slope Check

    #region Input Variables

    private Vector2 movementInput;
    private bool jumpActivated;
    private bool prevJumpActivated;
    private bool jumpOnlyJustActivated => !prevJumpActivated && jumpActivated;
    private bool hasInput => movementInput.x != 0 || movementInput.y != 0;

    #endregion Input Variables

    #region Misc Child Objects

    [Header("Misc Child Objects")]
    [SerializeField] private Transform momentumPosition;
    [SerializeField] private Transform movementDirectionTransform;

    #endregion Misc Child Objects

    #region Debug Objects

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI debugSpeedText;

    #endregion Debug Objects


    private Rigidbody rb;

    #region MonoBehaviour Functions

    private void Awake() {
        rb = this.GetComponent<Rigidbody>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start() {
        if (groundMask == 0) {
            groundMask = Globals.GROUND_MASK;
        }
    }

    // Update is called once per frame
    private void Update() {
        GroundedStateCheck();
        GroundedStateUpdates();

        prevGroundedState = groundedState;
        prevJumpActivated = jumpActivated;

        debugSpeedText.text = $"Horizontal: {rb.linearVelocity.Horizontal().magnitude:0.####}\n" +
            $"Up: {Vector3.Dot(rb.linearVelocity, Vector3.up):0.####}\n" +
            $"Overall: {rb.linearVelocity.magnitude:0.####}";
    }

    private void FixedUpdate() {
        Rotation();
        Movement();
        //UpdateMomentumStorage();
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
        this.GetComponent<Rigidbody>().linearDamping = Config.GroundDrag;
    }

    #endregion MonoBehaviour Functions

    #region Movement

    private void Movement() {

        if (!hasReportedMovement && (movementInput.x != 0 || movementInput.y != 0)) {
            hasReportedMovement = true;
            TutorialEvents.OnPlayerMoved?.Invoke();
        }

        // need to normalise movement since MoveInput() accumulates inputs
        movementInput = movementInput.normalized;

        VerticalMovement();
        HorizontalMovement();
        //rb.linearVelocity = new Vector3(Mathf.Clamp(rb.linearVelocity.x, -Config.MaxAcceleration, Config.MaxAcceleration), rb.linearVelocity.y, Mathf.Clamp(rb.linearVelocity.z, -Config.MaxAcceleration, Config.MaxAcceleration));
        //Decelerate();
    }

    private void VerticalMovement() {
        if (jumpActivated && canJump) {
            Jump();
            enableGroundedStateCheck = false;
            groundedState = PlayerGroundedState.InAir;
            hasJumped = true;
        }

        if (inAir) {
            if (rb.linearVelocity.y < 0f) {
                if (fallTimer < Config.MaxFallAccelerationTime) {
                    fallTimer += Time.fixedDeltaTime;
                    if (fallTimer > Config.MaxFallAccelerationTime)
                        fallTimer = Config.MaxFallAccelerationTime;
                }
                float currentFallingAcceleration = Mathf.Lerp(Config.MinFallAcceleration, Config.MaxFallAcceleration, fallTimer / Config.MaxFallAccelerationTime);
                rb.AddForce(new(0f, -currentFallingAcceleration, 0f), ForceMode.Acceleration);
            }
            else {
                fallTimer = 0f;
            }
        }
    }

    private void HorizontalMovement() {
        //bool hasInputX = movementInput.x != 0;
        //bool hasInputY = movementInput.y != 0;
        if (!hasInput) {     // no input
            if (!inAir && slideEnded) {
                rb.linearDamping = Config.StoppingDrag;
            }
            else {
                rb.linearDamping = 0f;
            }
        }
        else {
            //rb.linearDamping = inAir || !slideEnded ? Config.AirDrag : Config.GroundDrag;

            Vector3 worldMovement = GenerateWorldMovement();

            if (inAir) {
                if (KeepMomentumDespiteInputCheck(worldMovement.Flattened())) {
                    worldMovement = Vector3.ProjectOnPlane(worldMovement, rb.linearVelocity.Horizontal());
                    rb.linearDamping = 0f;
                }
                else {
                    rb.linearDamping = Config.AirDrag;
                }
            }
            else if (slideEnded) {
                rb.linearDamping = Config.GroundDrag;
            }

            rb.AddForce(worldMovement, ForceMode.Acceleration);
        }
    }

    private Vector3 GenerateWorldMovement() {
        Vector3 relativeMovement = new(movementInput.y, 0f, movementInput.x);
        relativeMovement.Normalize();
        relativeMovement *= inAir ? Config.AirAcceleration : Config.Acceleration;

        relativeMovement *= GetGroundedStateMovementModifier();

        Vector3 worldMovement = new(Vector3.Dot(relativeMovement, movementForward), 0f, Vector3.Dot(relativeMovement, movementRight));
        if (IsMovementACounterStrafe(worldMovement.Flattened())) {
            worldMovement *= Config.CounterStrafeMultiplier;
        }

        return worldMovement;
    }

    private bool IsMovementACounterStrafe(Vector2 desiredHorizontalMovement) {
        float angleFromInverse = 180f - Vector2.Angle(rb.linearVelocity.Flattened(), desiredHorizontalMovement);

        return angleFromInverse <= Config.CounterStrafeAngleError;
    }

    private bool KeepMomentumDespiteInputCheck(Vector2 desiredHorizontalMovement) {
        Vector2 currentHorizontalVelocity = rb.linearVelocity.Flattened();

        // if desired movement isn't somewhat in the same direction as the current movement
        if (Vector2.Dot(currentHorizontalVelocity, desiredHorizontalMovement) <= 0f) {
            return false;
        }

        // if current movement is within maximum air acceleration when unassisted (just by jumping)
        float maxUnassistedAirAcceleration = Config.AirAcceleration / Config.AirDrag;
        if (currentHorizontalVelocity.sqrMagnitude <= maxUnassistedAirAcceleration * maxUnassistedAirAcceleration) {
            return false;
        }

        // if the angle between the desired movement and current movement is too large
        if (Vector2.Angle(desiredHorizontalMovement, currentHorizontalVelocity) > Config.KeepMomentumDespiteInputAngle) {
            return false;
        }

        return true;
    }

    private void Decelerate() {
        if (!inAir)
            return;

        float accelerationSquared = rb.linearVelocity.HorizontalSqrMagnitude();

        if (decelerating && accelerationSquared <= Config.MaxAcceleration) {
            decelerating = false;
        }

        if (accelerationSquared > Config.MaxAcceleration * Config.MaxAcceleration) {
            if (accelerationSquared <= currentHighestAccelerationSquared)
                return;
            currentHighestAccelerationSquared = accelerationSquared;

            waitingToDecelerate = true;
            Debug.Log("waiting to decelerate");
            this.InvokeOverwrite("decelerate", () => {
                decelerating = true;
                rb.linearDamping = rb.linearVelocity.HorizontalMagnitude() / Config.MaxAcceleration;
                waitingToDecelerate = false;
                currentHighestAccelerationSquared = 0f;
                Debug.Log("decelerating");
            }, Config.TimeToDecelerate);
        }
        else {
            if (waitingToDecelerate) {
                Debug.Log("cancelling deceleration");
                this.InvokeCancel("decelerate");
                waitingToDecelerate = false;
            }
        }
    }

    #endregion Movement

    #region Momentum Storage [NOT IN USE RIGHT NOW]

    //private void UpdateMomentumStorage() {
    //    StoredMomentum currentMomentumToStore = new(rb.linearVelocity, Time.fixedTime);

    //    if (fastestMomentum.Momentum.sqrMagnitude < currentMomentumToStore.Momentum.sqrMagnitude) {
    //        fastestMomentum = currentMomentumToStore;
    //        storedMomentums.Clear();

    //        storedMomentums.Add(currentMomentumToStore);
    //    }
    //    else {
    //        storedMomentums.Add(currentMomentumToStore);

    //        float earliestAllowedTimeStamp = currentMomentumToStore.TimeStamp - momentumStorageTime;
    //        bool anyRemoved = false;

    //        while (storedMomentums.Any()) {
    //            if (storedMomentums.First().TimeStamp < earliestAllowedTimeStamp) {
    //                storedMomentums.RemoveAt(0);
    //                anyRemoved = true;
    //            }
    //            else {
    //                break;
    //            }
    //        }

    //        if (anyRemoved) {
    //            fastestMomentum = storedMomentums.Aggregate((sm1, sm2) => StoredMomentum.Compare(sm1, sm2));
    //        }
    //    }
    //}

    #endregion Momentum Storage

    #region Rotation

    private void Rotation() {
        movementForward = movementDirectionTransform.forward.Horizontal().normalized;
        movementRight = Vector3.Cross(Vector3.up, movementForward).normalized;
    }

    #endregion Rotation

    #region Jump

    private void Jump() {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * Config.JumpForce, ForceMode.Impulse);
    }

    private void CoyoteUpdate() {
        if (coyoteTimer < Config.CoyoteTime) {
            coyoteTimer += Time.deltaTime;
        }
    }

    #endregion Jump

    #region Ground & Slopes

    private bool DefaultGroundCheck(out RaycastHit hitInfo) {
        return Physics.Raycast(groundCheckPosition, Vector3.down, out hitInfo, groundCheckRange, groundMask, QueryTriggerInteraction.Collide);
    }

    private bool DefaultGroundCheck() {
        return DefaultGroundCheck(out _);
    }

    private void GroundedStateCheck() {
        if (!enableGroundedStateCheck)
            return;

        if (DefaultGroundCheck(out RaycastHit hitInfo)) {
            float angle = Vector3.Angle(Vector3.up, hitInfo.normal);
            inverseSlopeNormal = -hitInfo.normal;
            groundedState = (angle < Config.MaxSlopeAngle && angle > Config.MinSlopeAngle) ? PlayerGroundedState.OnSlope : PlayerGroundedState.OnGround;
        }
        else {
            groundedState = PlayerGroundedState.InAir;
        }
    }

    private void StickToSlope() {
        rb.AddForce(inverseSlopeNormal * 50f, ForceMode.Force);

        //CapsuleCollider collider = model.GetComponent<CapsuleCollider>();
        //if (Physics.SphereCast(collider.transform.position, collider.radius, Vector3.down, out RaycastHit hitInfo, 100f, Globals.GROUND_MASK)) {
        //    rb.position = new Vector3(rb.position.x, hitInfo.point.y + collider.bounds.extents.y, rb.position.z);
        //}
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
                break;

            case PlayerGroundedState.OnSlope:
                rb.useGravity = false;
                break;

            case PlayerGroundedState.InAir:
                coyoteTimer = 0f;
                timeSpentGrounded = 0f;

                enableGroundedStateCheck = false;
                this.InvokeCancel("End Slide");
                slideEnded = true;
                break;

            default:
                break;
        }

        if (groundedState != PlayerGroundedState.OnSlope) {
            rb.useGravity = true;
        }

        if (!inAir) {
            if (waitingToDecelerate && slideEnded) {
                this.InvokeCancel("decelerate");
                Debug.Log("cancelling deceleration");
                waitingToDecelerate = false;
            }

            slideEnded = false;
            this.InvokeExclusive("End Slide", () => slideEnded = true, Config.SlideTime);
            hasJumped = false;
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
                StickToSlope();
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
            PlayerGroundedState.OnSlope => Config.SlopeSpeedMultiplier,
            _ => 1f
        };
    }

    #endregion Ground & Slopes

    #region Wall Kick

    //private void 

    #endregion Wall Kick

    #region Input

    // in case direct input is required
    public void MoveInput(InputAction.CallbackContext context) {
        MoveInput(context.ReadValue<Vector2>());
    }

    // same as above
    public void JumpInput(InputAction.CallbackContext context) {
        JumpInput(context.ReadValueAsButton());
    }

    public void MoveInput(Vector2 input) {
        if (input.x == 0) {
            movementInput.x = 0;
        }
        else {
            movementInput.x += input.x;
        }

        if (input.y == 0) {
            movementInput.y = 0;
        }
        else {
            movementInput.y += input.y;
        }
    }

    public void JumpInput(bool activated) {
        jumpActivated = activated;

        if (jumpOnlyJustActivated) {

        }
    }

    #endregion Input

    #region IMomentumModifiable

    public Vector3 GetPosition() {
        return momentumPosition.position;
    }

    public Vector3 GetMomentum() {
        return rb.linearVelocity;
    }

    public void SetMomentum(Vector3 newMomentum) {
        rb.linearVelocity = newMomentum;
    }

    #endregion IMomentumModifiable
}
