using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonTestController : MonoBehaviour, IMomentumModifiable, ITargetable, IDamageable {

    #region Variables

    [SerializeField] private PlayerControllerConfig Config;

    #region Attack & Damage

    [SerializeField, Min(0f)] private float attack;
    [SerializeField, Min(0f)] private float attackRange;
    [SerializeField] private LayerMask attackMask;
    [SerializeField] private List<SpeedStageThreshold> damageThresholds;

    [SerializeField, Min(0f)] private float maxHealth;
    [SerializeField, Min(0f)] private float health;
    [SerializeField] private Slider healthBar;

    #endregion Attack & Damage

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

    private Vector3 slopePlane = Vector3.up;


    #endregion Ground & Slope Check

    #region Camera Variables

    [Header("Camera Settings")]
    [SerializeField, Range(0f, 15f)] private float cameraDistance = 3f;
    [SerializeField] private CinemachineInputAxisController inputAxisController;
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [SerializeField] private CinemachineDeoccluder deoccluder;
    [SerializeField] private CinemachineCamera ccamera;
    [SerializeField] private GameObject cameraObject;

    #endregion Camera Variables

    #region Input Variables

    private Vector2 movementInput;
    private bool jumpActivated;

    #endregion Input Variables

    #region Debug Variables

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI debugSpeedText;

    #endregion Debug Variables

    #region Misc Child Objects

    [Header("Miscellaneous Child Objects")]
    [SerializeField] private GameObject model;
    [SerializeField] private Transform throwTransform;
    [SerializeField] private Transform momentumPosition;
    [SerializeField] private Transform targetPosition;

    #endregion Misc Child Objects

    #region Events

    public UnityEvent PrimaryAction = new();
    public UnityEvent SecondaryAction = new();

    #endregion Events

    private Rigidbody rb;
    public Transform Target => targetPosition;

    #endregion Variables

    #region MonoBehaviour Functions

    private void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Globals.PLAYER = this;

        if (groundMask == 0) {
            groundMask = Globals.GROUND_MASK;
        }

        deoccluder.CollideAgainst = Globals.OBSTACLE_MASK;
        deoccluder.TransparentLayers = ~Globals.OBSTACLE_MASK;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        rb = this.GetComponent<Rigidbody>();

        UpdateSensitivity();
        orbitalFollow.Radius = cameraDistance;
        deoccluder.AvoidObstacles.DistanceLimit = cameraDistance;

        healthBar.maxValue = maxHealth;
        healthBar.value = health;
    }

    // Update is called once per frame
    void Update() {
        GroundedStateCheck();
        GroundedStateUpdates();

        model.transform.rotation = Quaternion.Euler(0f, cameraObject.transform.rotation.eulerAngles.y, 0f);
        ccamera.Target.TrackingTarget.rotation = ccamera.transform.rotation;
        throwTransform.rotation = ccamera.transform.rotation;
        prevGroundedState = groundedState;

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

    private void OnCollisionStay(Collision collision) {
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
        tempInputAxisController.Controllers[0].Input.Gain = (1f / 75f) * Config.Sensitivity;
        tempInputAxisController.Controllers[1].Input.Gain = -(1f / 75f) * Config.Sensitivity;

        this.GetComponent<Rigidbody>().linearDamping = Config.GroundDrag;

        damageThresholds = damageThresholds.OrderBy(dt => dt.SpeedThreshold).ToList();
    }

    private void OnDrawGizmos() {
        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(model.transform.position, model.transform.position + model.transform.forward);
        //Gizmos.DrawLine(model.transform.position, model.transform.position + ccamera.transform.forward);
    }

    #endregion MonoBehaviour Functions

    #region Attack

    public void Attack() {
        float damage = CalculateDamage();

        // TODO: change it so attack range scales with speed too

        IDamageable[] damageables = GetEnemiesInAttackBox();
        foreach (var damageable in damageables) {
            damageable.TakeDamage(damage);
        }
    }

    private float CalculateDamage() {
        if (damageThresholds.Count < 1) {
            return attack;
        }

        for (int i = 1; i < damageThresholds.Count; i++) {
            if (rb.linearVelocity.magnitude < damageThresholds[i].SpeedThreshold) {
                return attack * damageThresholds[i - 1].DamageMultiplier;
            }
        }

        return attack * damageThresholds.Last().DamageMultiplier;
    }

    private IDamageable[] GetEnemiesInAttackBox() {
        // TODO: change this to a rotation sweep capsule cast

        Vector3 halfAttackForward = attackRange * ccamera.transform.forward / 2f;
        return Physics.OverlapBox(this.transform.position + halfAttackForward, new Vector3(attackRange, attackRange, attackRange) / 2f, ccamera.transform.rotation, attackMask, QueryTriggerInteraction.Collide)
            .Where(collider => Utility.DoesMaskContainLayer(attackMask, collider.gameObject.layer)
                && collider.attachedRigidbody != null
                && collider.attachedRigidbody.GetComponent<IDamageable>() != null)
            .Select(collider => collider.attachedRigidbody.GetComponent<IDamageable>())
            .ToArray();
    }

    #endregion Attack

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
                rb.AddForce(new(0f, -Mathf.Lerp(Config.MinFallAcceleration, Config.MaxFallAcceleration, fallTimer / Config.MaxFallAccelerationTime), 0f), ForceMode.Acceleration);
            }
            else {
                fallTimer = 0f;
            }
        }
    }

    private void HorizontalMovement() {
        bool hasInputX = movementInput.x != 0;
        bool hasInputY = movementInput.y != 0;

        if (!hasInputX && !hasInputY) {     // no input
            if (!inAir) {                   // on ground
                if (slideEnded) {
                    rb.linearDamping = Config.StoppingDrag;
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
            rb.linearDamping = inAir || !slideEnded ? Config.AirDrag : Config.GroundDrag;

            Vector3 worldMovement = GenerateWorldMovement();
            rb.AddForce(worldMovement, ForceMode.Acceleration);
        }
    }

    private Vector3 GenerateWorldMovement() {
        Vector3 relativeMovement = new(movementInput.y, 0f, movementInput.x);
        relativeMovement.Normalize();
        relativeMovement *= inAir ? Config.AirAcceleration : Config.Acceleration;

        relativeMovement *= GetGroundedStateMovementModifier();

        Vector3 worldMovement = new(Vector3.Dot(relativeMovement, movementForward), 0f, Vector3.Dot(relativeMovement, movementRight));
        return worldMovement;
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
        movementForward = cameraObject.transform.forward.Horizontal().normalized;
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
            slopePlane = -hitInfo.normal;
            groundedState = (angle < Config.MaxSlopeAngle && angle > Config.MinSlopeAngle) ? PlayerGroundedState.OnSlope : PlayerGroundedState.OnGround;
        }
        else {
            groundedState = PlayerGroundedState.InAir;
        }
    }

    private void StickToSlope() {
        rb.AddForce(slopePlane * 50f, ForceMode.Force);

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
                slideEnded = false;
                this.InvokeExclusive("End Slide", () => slideEnded = true, Config.SlideTime);
                hasJumped = false;
                break;

            case PlayerGroundedState.OnSlope:
                slideEnded = false;
                this.InvokeExclusive("End Slide", () => slideEnded = true, Config.SlideTime);
                hasJumped = false;

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
            PrimaryAction.Invoke();
        }
        else if (context.canceled) {
        }
    }

    public void ThrowInput(InputAction.CallbackContext context) {
        if (context.started) {
            SecondaryAction.Invoke();
        }
        else if (context.canceled) {
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

        inputAxisController.Controllers[0].Input.Gain = (1f / 75f) * Config.Sensitivity;
        inputAxisController.Controllers[1].Input.Gain = -(1f / 75f) * Config.Sensitivity;
    }

    #endregion Sensitivity

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

    #region IDamageable

    public float GetHealth() {
        return health;
    }

    public void TakeDamage(float amount) {
        health -= amount;
        healthBar.value = health;
        Debug.Log($"Damage Amount: {amount}, Current Health: {health}");
    }

    public void Kill() {
        throw new System.NotImplementedException();
    }

    public void Heal(float amount) {
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
    }

    #endregion IDamageable
}