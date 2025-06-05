using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Unity.Cinemachine;
using Unity.Android.Types;
using UnityEditorInternal;
public class PlayerController : MonoBehaviour, IMomentumModifiable, ITargetable
{
    #region     ========================= Variables =========================
    [Header("Movement")]
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float maxMovementSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField, Tooltip("Controls how long it takes for velocity takes to return to max speed when the player goes over it")] 
    private float velocityDecayTime;

    [Space(10)]
    [SerializeField, Tooltip("the amount the player's max speed and acceleration increases by when in air")] 
    private float airSpeedIncrease;
    [SerializeField] private float maxFallSpeed;
    [SerializeField, Range(0,1), Tooltip("Controls how much control the player has when in the air (0 is none, 1 is full)")] 
    private float airControlMultiplier;

    [Space(10)]
    [SerializeField] private Transform orientation;

    private float accelerationProgress = 0;

    private Rigidbody playerRigidBody;

    private float movementSpeed;
    private float horizontalInput, verticalInput;
    private float speedLerpProgress;
    private Vector3 moveDirection;
    private Vector3 velocity;
    private Vector3 lastAirVelocity;

    private float maxSpeedStorage;
    private float accelerationStorage;


    [Space(10)]
    [Header("Slope Movement")]
    [SerializeField, Tooltip("Maxium slope angle the player can go up")] 
    private float maxSlopeAngle;
    [SerializeField, Tooltip("the amount the player's max speed and acceleration increases by when on a slope")] 
    private float slopeSpeedImpact;
    private RaycastHit slopeHit;
    private bool exitSlope;

    [Space(10)]
    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField, Tooltip("applies a force once player releases jump so it reaches thye apex faster  ")] 
    private float maxJumpMultiplier;
    [SerializeField, Tooltip("applies a a force when player falls so they fall quicker  ")] 
    private float fallMultiplier;
    [SerializeField] private float jumpCooldown;

    [Space(5)]
    [SerializeField, Tooltip("duration of coyote time")] 
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private bool canJump = true;
    private bool jumpReleased;

    [Space(10)]
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPosition;
    [SerializeField] private float groundCheckRange;
    [SerializeField] private LayerMask groundMask;
    private bool isGrounded;

    [Space(10)]
    [Header("Wall Check & Kick")]
    [SerializeField] private float wallKickRange;
    [SerializeField] private float wallKickCooldown;
    private float timeOfLastKick;

    [Space(10)]
    [Header("Attack")]
    [SerializeField] private Vector3 attackScale;
    [SerializeField] private LayerMask attackMask;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackCooldown;

    private float attackTime;

    [Space(10)]
    [Header("Speed Stages")]
    [SerializeField, Tooltip("the value that controls how fast the player has to be reach the 2nd speed stage")] 
    private float firstBreakpoint;
    [SerializeField, Tooltip("the value that controls how fast the player has to be reach the 3rd speed stage")] 
    private float secondBreakpoint;
    private int currentStage = 1; //tracks which stage the player's speed is at 

    [Space(10)]
    [Header("Bounce Bomb")]
    [SerializeField] private GameObject bounceBomb;
    [SerializeField, Min(0f)] private float bombThrowPower = 1f;
    [SerializeField] private Transform bounceBombSpawnTransform;
    [SerializeField, Min(0f)] private float fuseTime = 0.2f;
    private Vector3 bounceBombSpawnPosition => bounceBombSpawnTransform.position;
    private GameObject bounceBombInstance;
    private bool isDetonating = false;

#if DEBUG
    [Space(10)]
    [Header("Events")]
    [SerializeField] private UnityEvent EventPrimaryClick = new();
    [SerializeField] private UnityEvent EventSecondaryClick = new();
#endif 

    [Space(10)]
    [Header("Animation Controller")]
    [SerializeField] Animator animator;

    [Space(10)]
    [Header("Target")]
    [SerializeField] private Transform target;

    #endregion  ========================= Variables =========================

    void Awake() {
        Globals.PLAYER = this;
    }

    void Start() {
        playerRigidBody = GetComponent<Rigidbody>();

        accelerationStorage = acceleration;
        maxSpeedStorage = maxMovementSpeed;

        maxFallSpeed = -maxFallSpeed;

        currentStage = 1;

        ValidateBounceBomb();
        EventSecondaryClick.AddListener(SpawnBounceBomb);

        EventPrimaryClick.AddListener(Attack);
    }

    void Update() {
        GroundCheck();

        MovementInput();
        ActionInputs();

        MovementSpeed();
        GetSpeedStage();

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            GetLastAirVelocity();
        }

        PerserveMomentumOnLand();
    }
    void FixedUpdate() {
        MovePlayer();
        VariableJump();
    }
    void LateUpdate() {
        MaxSpeed();
    }
    #region ========================= Inputs =========================
    private void MovementInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            Vector3 wallHit;
            if (canJump && coyoteTimeCounter > 0) {
                Jump();
                canJump = false;
                jumpReleased = false;
                exitSlope = true;

                this.Invoke(ResetJump, jumpCooldown);
            }
            else if (WallCheck(out wallHit)) {;
                WallKick(wallHit); 
            }
            else if (Input.GetButtonUp("Jump") && !isGrounded) {
                jumpReleased = true;
            }
        }
    }
    private void ActionInputs() {
        attackTime += Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && attackTime >= attackCooldown) { 
            EventPrimaryClick.Invoke(); 
            attackTime = 0;
        }
        if (Input.GetMouseButtonDown(1)) { EventSecondaryClick.Invoke(); }
    }

    #endregion  ========================= Inputs =========================

    #region ========================= Movement =========================
   
    /// <summary>
    /// Gets the movement direction from orintation and applies the vertical and horizontal inputs values 
    /// Adds force to this direction
    /// Checks if grounded and adds drag if so
    /// </summary>
    private void MovePlayer() {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (SlopeCheck() && !exitSlope){
            playerRigidBody.AddForce(GetSlopeMovementDiretion() * movementSpeed * 20f, ForceMode.Force);


            if (Input.GetButton("Horizontal") || Input.GetButton("Vertical")) { 
                playerRigidBody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        if (isGrounded) {
            playerRigidBody.AddForce(moveDirection.normalized * movementSpeed * 10, ForceMode.Force);
            playerRigidBody.linearDamping = groundDrag; 
        }
        else {
            playerRigidBody.AddForce(moveDirection.normalized * movementSpeed * 10 * airControlMultiplier, ForceMode.Force);
            playerRigidBody.linearDamping = 0;
        }

        playerRigidBody.useGravity = !SlopeCheck();
    }

    /// <summary>
    /// Lerps the current movement speed from 0 to the maximum movement speed when the player is using player input
    /// Does the reverse when the player is not inputing movement controls
    /// </summary>
    private void MovementSpeed() {
        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical")) {
            movementSpeed = Mathf.Lerp(0, maxMovementSpeed, accelerationProgress);
            accelerationProgress += Time.deltaTime * (acceleration * 0.1f);
            accelerationProgress = Mathf.Clamp(accelerationProgress, 0, 1);
        }
        else {
            float inverseProgress = 1 - accelerationProgress;
            movementSpeed = Mathf.Lerp(maxMovementSpeed, 0, inverseProgress);

            accelerationProgress = Time.deltaTime * deceleration * 0.1f;
            accelerationProgress = Mathf.Clamp(accelerationProgress, 0, 1);
        }
    }

    /// <summary>
    /// Ensures the player does not move faster than the max speed 
    /// Calls functions to increase max speed when on slopes or in air
    /// </summary>
    private void MaxSpeed() {               
        AirSpeedIncrease();
        SlopeSpeedIncrease();

        if (SlopeCheck() && !exitSlope && playerRigidBody.linearVelocity.magnitude > maxMovementSpeed) {
            playerRigidBody.linearVelocity = playerRigidBody.linearVelocity.normalized * movementSpeed;
        }
        else {
                
            Vector3 velocity = new Vector3(playerRigidBody.linearVelocity.x, 0, playerRigidBody.linearVelocity.z);
            Vector3 maxVelocity = velocity.normalized * maxMovementSpeed;

            if(velocity.magnitude > maxMovementSpeed * 3) {
                playerRigidBody.linearVelocity = new Vector3(maxVelocity.x * 3, playerRigidBody.linearVelocity.y, maxVelocity.z * 3);
            }
            else if (velocity.magnitude > maxMovementSpeed) {
                float distance = Vector3.Distance(maxVelocity, playerRigidBody.linearVelocity);

                DOTween.Init();
                DOTween.To(() => speedLerpProgress, x => speedLerpProgress = x, 1, velocityDecayTime);

                Vector3 lerpedVelocity = Vector3.Lerp(playerRigidBody.linearVelocity, maxVelocity, speedLerpProgress);
                playerRigidBody.linearVelocity = new Vector3(lerpedVelocity.x, playerRigidBody.linearVelocity.y, lerpedVelocity.z);
                
                if (speedLerpProgress >= 1) {
                    speedLerpProgress = 0;
                }
            }

            float yVelocity = Mathf.Abs(playerRigidBody.linearVelocity.y);
            if (yVelocity <= maxFallSpeed) {
                playerRigidBody.linearVelocity = new Vector3(playerRigidBody.linearVelocity.x, maxFallSpeed, playerRigidBody.linearVelocity.z);
            } 
        }
    }

    /// <summary>
    /// If the player is in air they will beable to move faster
    /// </summary>
    private void AirSpeedIncrease() {
        if (!isGrounded) {
            if (playerRigidBody.linearVelocity.y < 0) {
                maxMovementSpeed = maxSpeedStorage + airSpeedIncrease;
                acceleration = accelerationStorage + airSpeedIncrease;
            }
            else {
                maxMovementSpeed = maxSpeedStorage;
                acceleration = accelerationStorage;
            }
        }
    }
    /// <summary>
    /// If the player is on a slope they will be able to move faster
    /// </summary>
    private void SlopeSpeedIncrease()
    {
        if(SlopeCheck()) {
            if (playerRigidBody.linearVelocity.y < 0) {
                maxMovementSpeed = maxSpeedStorage + slopeSpeedImpact;
                acceleration = accelerationStorage + slopeSpeedImpact;
            }
            else {  
                maxMovementSpeed = maxSpeedStorage;
                acceleration = accelerationStorage;
            }
        } 
        else if (isGrounded) {
            maxMovementSpeed = maxSpeedStorage;
            acceleration = accelerationStorage;
        }
    }

    /// <summary>
    /// Uses the last stored air velocity and replaces the player's velocity with it
    /// </summary>
    private void PerserveMomentumOnLand() {
        bool previousGround = isGrounded;
        GroundCheck();

        if (previousGround == false && isGrounded == true) {
            playerRigidBody.linearVelocity = new Vector3(lastAirVelocity.x, playerRigidBody.linearVelocity.y, lastAirVelocity.z);
        }
    }
    private void GetLastAirVelocity() {
            lastAirVelocity = playerRigidBody.linearVelocity;
        
    }
    #endregion ========================= Movement =========================

    #region     ========================= Jump =========================
    private void Jump() {
        exitSlope = true;
        playerRigidBody.linearVelocity = new Vector3(playerRigidBody.linearVelocity.x, 0, playerRigidBody.linearVelocity.z) ;
        playerRigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Applies a force to the player after the player has released the jump button to make it reach the apex of the jump earlier
    /// Applies a force when the player is falling to make them fall faster
    /// </summary>
    private void VariableJump()
    {
        if (jumpReleased && !isGrounded && playerRigidBody.linearVelocity.y > 0) {
            playerRigidBody.AddForce(Vector3.down * maxJumpMultiplier, ForceMode.Force);
        }
        else if (jumpReleased && !isGrounded && playerRigidBody.linearVelocity.y < 0) {
            playerRigidBody.AddForce(Vector3.down * fallMultiplier, ForceMode.Force);
        }
    }

    private void ResetJump() {
        canJump = true;
        exitSlope = false;
    }
    #endregion  ========================= Jump =========================

    #region  ========================= Wall Kick =========================
    /// <summary>
    /// Performs a check to see if a wall is closeenough for the wall kick
    /// </summary>
    private bool WallCheck(out Vector3 wallNormal) {
        if(Time.time >= timeOfLastKick + wallKickCooldown && !isGrounded) {
            RaycastHit wallHit;
            bool isWall = Physics.Raycast(groundCheckPosition.position, orientation.forward, out wallHit, wallKickRange, groundMask);
            Debug.DrawRay(groundCheckPosition.position, orientation.forward * wallKickRange, Color.blueViolet, 10);

            wallNormal = wallHit.normal;
            return isWall;
        }
        else { 
            wallNormal = Vector3.zero;
            return false; 
        }
        
    }
    /// <summary>
    /// Inverses the x & z velocity of the player
    /// </summary>
    private void WallKick(Vector3 wallNormal) {
        timeOfLastKick = Time.time;
        
        Vector3 reflectedDirection = Vector3.Reflect(playerRigidBody.linearVelocity, wallNormal);
        //reflectedDirection = reflectedDirection * 10;
        Vector3 newVelocity = new Vector3(reflectedDirection.x, playerRigidBody.linearVelocity.y, reflectedDirection.z);
        playerRigidBody.linearVelocity = newVelocity;
    }
    #endregion  ========================= Wall Kick =========================

    #region     ========================= Ground Check =========================
    private void GroundCheck() {
        isGrounded = Physics.Raycast(groundCheckPosition.position, Vector3.down, groundCheckRange, groundMask);
    }
    /// <summary>
    /// Casts a ray to find the angle the ground is at to detect if its a slope
    /// </summary>
    /// <returns> Returns a boolean value based on the angle the ground is at </returns>
    private bool SlopeCheck(){
        if (Physics.Raycast(groundCheckPosition.position, Vector3.down, out slopeHit, groundCheckRange)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
    private Vector3 GetSlopeMovementDiretion() {
        return Vector3.ProjectOnPlane(moveDirection,slopeHit.normal).normalized;
    }
    #endregion  ========================= Ground Check =========================

    #region  ========================= Momentum Interface =========================
    public Vector3 GetMomentum() {
        return playerRigidBody.linearVelocity;
    }
    public Vector3 GetPosition() {
        return playerRigidBody.position;
    }
    public void SetMomentum(Vector3 value) {
        playerRigidBody.AddForce(value, ForceMode.VelocityChange);
    }
    #endregion  ========================= Momentum Interface  =========================

    #region ========================= Throw Bounce Bomb =========================
    private void ValidateBounceBomb() {
        if (bounceBomb == null)
            throw new System.Exception("Cannot reference null Bounce Bomb");
        if (bounceBomb.GetComponent<BounceBomb>() == null)
            throw new System.Exception("Bounce Bomb reference doesn't have the required BounceBomb script");
    }

    private void SpawnBounceBomb() {
        // spawns BounceBomb and 'throws' it via AddForce()
        bounceBombInstance = Instantiate(bounceBomb, bounceBombSpawnPosition, playerRigidBody.rotation);
        bounceBombInstance.GetComponent<Rigidbody>().AddForce(GetMomentum() + Camera.main.transform.forward * bombThrowPower, ForceMode.VelocityChange);

        EventSecondaryClick.RemoveListener(SpawnBounceBomb);
        EventSecondaryClick.AddListener(DetonateBounceBomb);
    }
    private void DetonateBounceBomb() {
        if (isDetonating)
            return;

        isDetonating = true;
        this.Invoke(() => {
            bounceBombInstance.GetComponent<BounceBomb>().Activate();
            Destroy(bounceBombInstance);

            EventSecondaryClick.RemoveListener(DetonateBounceBomb);
            EventSecondaryClick.AddListener(SpawnBounceBomb);
            isDetonating = false;
        }, fuseTime);
    }
    #endregion ========================= Throw Bounce Bomb =========================

    #region ========================= Speed Stages =========================
    private void GetSpeedStage() {
        float speed = playerRigidBody.linearVelocity.magnitude;
        if (speed > secondBreakpoint) {
            // Checks if player is in Stage 3
            currentStage = 3; 
            return;
        }
        else if (speed > firstBreakpoint) {
            // Checks if player is in Stage 2
            currentStage = 2;
            return;
        }
        else { 
            // Checks if player is in stage 1
            currentStage = 1;
            return;
        }
    }
    #endregion ========================= Speed Stages =========================

    #region ========================= Attack =========================
    /// <summary>
    /// Checks current speedstage then calls attack check to see if it hit anything
    /// then goes through the array and checks it hit object can take damage
    /// </summary>
    private void Attack() {
        RaycastHit[] enemies;

        animator.SetTrigger("hasAttacked");

        if (currentStage == 3) {
            // larger aoe && oneshot && vfx
            //VFX GOES HERE
            Vector3 largerScale = attackScale * 2;
            enemies = AttackCheck(largerScale, attackRange +2, Color.red);
        }
        else if (currentStage == 2)  {
            //one shot & VFX
            //VFX GOES HERE
            enemies = AttackCheck(attackScale, attackRange, Color.yellow);
        }
        else {
            // default
            enemies = AttackCheck(attackScale, attackRange, Color.green);
        }

        if (enemies.Length != 0) {
            foreach (RaycastHit enemy in enemies) {
                IDamageable damage = enemy.transform.GetComponent<IDamageable>();
                if (damage != null) {
                    if (currentStage >= 2) { damage.Kill(); 
                    } 
                    else {
                        damage.TakeDamage(attackDamage); 
                    }
                }
            }
        }
    }
    /// <summary>
    /// Performs a box cast all with the specific conditions 
    /// </summary>
    /// <param name="attackSize"> the size of the box </param>
    /// <param name="range"> the range of the attack </param>
    /// <param name="colour"> controls the colour of the debug </param>
    /// <returns> returns an array containing all enemies it by the box cast </returns>
    private RaycastHit[] AttackCheck(Vector3 attackSize, float range, Color colour) {
        RaycastHit[] enemies = null;
        Vector3 direction = Camera.main.transform.position;
        enemies = Physics.BoxCastAll(direction, attackSize, Camera.main.transform.forward, Quaternion.identity, range, attackMask);
        Debug.DrawRay(direction, Camera.main.transform.forward * attackRange, colour, 3f);
        return enemies;
    }
    #endregion ========================= Attack =========================

    #region ========================= Gizmos =========================
    private void OnDrawGizmos() {
    }
    #endregion ========================= Gizmos =========================

    #region ========================= Targetable =========================
    public Transform Target {
        get => target;
    }
    #endregion ========================= Targetable =========================
}