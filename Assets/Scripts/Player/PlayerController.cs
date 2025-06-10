using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.VFX;
using UnityEngine.UI;
using TMPro;
public class PlayerController : MonoBehaviour, IMomentumModifiable, IDamageable, ITargetable
{
    #region     ========================= Variables =========================

    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI groundedText;
    [SerializeField] private float downwardsForce;
    [Header("Health")]
    [SerializeField] private float health;
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject gameOver;

    [Header("Movement")]
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float startSpeed;
    [SerializeField] private float maxMovementSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField, Tooltip("Controls the rate max speed & acceleration is reduced after being increased")]
    private float velocityDecayRate;
    [SerializeField, Tooltip("How often velocity is stored in seconds, used for the wall kick")]
    private float veloctiyStorageTime;
    private bool doesVeloctiyTweenExist = false;
    private Tween velocityTween;

    [Space(10)]
    [SerializeField, Tooltip("the amount the player's max speed and acceleration increases by when in air")]
    private float airSpeedIncrease;
    [SerializeField] private float maxFallSpeed;
    [SerializeField, Range(0, 1), Tooltip("Controls how much control the player has when in the air (0 is none, 1 is full)")]
    private float airControlMultiplier;

    [Space(10)]
    [SerializeField] private Transform orientation;

    private float accelerationProgress = 0;

    private Rigidbody playerRigidBody;

    private float movementSpeed;
    private float horizontalInput, verticalInput;
    private float speedLerpProgress;

    private Vector3 moveDirection;
    private Vector3 lastAirVelocity;

    private Vector3 veloctiyStorage;
    private bool storeVelocity = true;

    private float maxSpeedStorage;
    private float accelerationStorage;

    private float dragTimer;

    [Space(10)]
    [Header("Slope Movement")]
    [SerializeField, Tooltip("Maxium slope angle the player can go up")]
    private float maxSlopeAngle;
    [SerializeField, Tooltip("Minimum slope angle the player gets a speed boost from")]
    private float minSlopeAngle;
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
    private bool kickOnce;

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
    [SerializeField] private Animator animator;

    [Space(10)]
    [Header("Visual Effects")]
    [SerializeField] private VisualEffect runningLines;
    [SerializeField] private Vector2 minSpeedOfLines, maxSpeedOfLines;
    [SerializeField] private float minSpawnRate, maxSpawnRate;

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

        healthBar.maxValue = health;
        healthBar.value = health;

        ValidateBounceBomb();
        EventSecondaryClick.AddListener(SpawnBounceBomb);
        EventPrimaryClick.AddListener(Attack);

        DOTween.Init();
        if (!doesVeloctiyTweenExist) {
            velocityTween = DOTween.To(() => speedLerpProgress, x => speedLerpProgress = x, 1, velocityDecayRate);
            velocityTween.Pause();
        }
    }
    void Update() {
        if (DeathCheck()) { return; }

        GroundCheck();

        MovementInput();
        ActionInputs();

        MovementSpeed();
        if (storeVelocity) { StartCoroutine(GetVelocity()); }
        GetSpeedStage();

        if (isGrounded) {
            coyoteTimeCounter = coyoteTime;
        }
        else {
            coyoteTimeCounter -= Time.deltaTime;
            GetLastAirVelocity();
        }
        speedText.text = playerRigidBody.linearVelocity.magnitude.ToString();

        if (Input.GetKey(KeyCode.LeftShift)) { playerRigidBody.AddForce(orientation.forward * 3, ForceMode.Impulse); }
    }
    void FixedUpdate() {
        MovePlayer();
        VariableJump();
        PerserveMomentumOnLand();
        MaxSpeed();
    }

    #region ========================= Inputs =========================
    private void MovementInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetButton("Jump"))
        {
            if (canJump && coyoteTimeCounter > 0) {
                Jump();

                canJump = false;
                jumpReleased = false;
                exitSlope = true;

                this.Invoke(ResetJump, jumpCooldown);
            }
            else if (Input.GetButtonUp("Jump") && !isGrounded) {
                jumpReleased = true;
            }
        }

        if (Input.GetButtonDown("Jump") && !isGrounded) {
            Vector3 wallHit;

            if (WallCheck(out wallHit) && !kickOnce) {
                WallKick(wallHit);
            }
        }
        else if (Input.GetButtonUp("Jump")) {
            kickOnce = false;

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

        if (SlopeCheck() && !exitSlope) {
            playerRigidBody.AddForce(GetSlopeMovementDiretion() * movementSpeed * 20f, ForceMode.Force);

            if (horizontalInput != 0 || verticalInput != 0 && playerRigidBody.linearVelocity.y <0) {
                playerRigidBody.AddForce(Vector3.down * downwardsForce, ForceMode.Force);
            }
        }
        if (isGrounded) {
            playerRigidBody.AddForce(moveDirection.normalized * movementSpeed * 10, ForceMode.Force);
            playerRigidBody.linearDamping = groundDrag;
        }
        else {
            playerRigidBody.AddForce(moveDirection.normalized * movementSpeed * 10 * airControlMultiplier, ForceMode.Force);
            playerRigidBody.linearDamping = 0f;
        }

        playerRigidBody.useGravity = !SlopeCheck();
       // PlayerDrag();
    }

    private void PlayerDrag() {
        //if in air 0
        //if moving
            // if above max speed increase drag
            // if below decrease drag
        if (!isGrounded) { 
            playerRigidBody.linearDamping = 0;
            dragTimer = 0;
        }/*
        else if (horizontalInput != 0 || verticalInput != 0 && playerRigidBody.linearVelocity.magnitude > maxMovementSpeed) {
            dragTimer += Time.deltaTime;
            float dragValue = playerRigidBody.linearDamping;
            playerRigidBody.linearDamping = Mathf.MoveTowards(dragValue, groundDrag, .5f * dragTimer);
            playerRigidBody.linearDamping = dragValue;
        }*/
        else if (horizontalInput != 0 || verticalInput != 0 && playerRigidBody.linearVelocity.magnitude < maxMovementSpeed) { 
            playerRigidBody.linearDamping = 0; 
        }
        else { 
            playerRigidBody.linearDamping = groundDrag; 
            dragTimer = 0;
        }
    }

    /// <summary>
    /// Lerps the current movement speed from 0 to the maximum movement speed when the player is using player input
    /// Does the reverse when the player is not inputing movement controls
    /// 
    /// Controls the amount of force applied to the player
    /// </summary>
    private void MovementSpeed() {

        if (horizontalInput != 0 || verticalInput != 0) {
            movementSpeed = Mathf.Lerp(startSpeed, maxMovementSpeed, accelerationProgress);
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
        Vector3 velocity = new Vector3(playerRigidBody.linearVelocity.x, 0, playerRigidBody.linearVelocity.z);
        Vector3 maxVelocity = velocity.normalized * maxMovementSpeed;
        Vector3 combinedVelocity = new Vector3(maxVelocity.x, playerRigidBody.linearVelocity.y, maxVelocity.z);

        if (SlopeCheck() && !exitSlope && velocity.magnitude > maxMovementSpeed) {
            playerRigidBody.linearVelocity = playerRigidBody.linearVelocity.normalized * movementSpeed;
            Debug.Log("SlopeCheck");
        }
        else {
           if (velocity.magnitude > maxMovementSpeed * 3) {
                playerRigidBody.linearVelocity = new Vector3(maxVelocity.x * 3, playerRigidBody.linearVelocity.y, maxVelocity.z * 3);
            }
             /* if (velocity.magnitude > maxMovementSpeed && isGrounded) {

                if (!velocityTween.IsPlaying()) { velocityTween.Play(); }

                Vector3 lerpedVelocity = Vector3.Lerp(playerRigidBody.linearVelocity, maxVelocity, speedLerpProgress);
                playerRigidBody.linearVelocity = new Vector3(lerpedVelocity.x, playerRigidBody.linearVelocity.y, lerpedVelocity.z);
                
                if (playerRigidBody.linearVelocity.magnitude  <= maxMovementSpeed +.5f && velocityTween != null)
                {
                    Debug.Log("ResetTween & PRogress");
                    speedLerpProgress = 0;

                    velocityTween.Restart();
                    velocityTween.Pause();
                }
            }*/

            //Vector3.Movetowards to reduce mvoement speed;
            /*if (velocity.magnitude > maxMovementSpeed && isGrounded) {
                Debug.Log("Reducing speed to MaxSpeed");
                Vector3 horizontalVelocity = Vector3.MoveTowards(velocity, maxVelocity, velocityDecayRate * Time.deltaTime);
                playerRigidBody.linearVelocity = new Vector3(horizontalVelocity.x, playerRigidBody.linearVelocity.y, horizontalVelocity.z);
            } */

            /* Mathf.movetowards */
            if (maxMovementSpeed > maxSpeedStorage && isGrounded) {
                Debug.Log("reducing max speed ");
                maxMovementSpeed = Mathf.MoveTowards(maxMovementSpeed, maxSpeedStorage, velocityDecayRate * Time.deltaTime);
                acceleration = Mathf.MoveTowards(acceleration, accelerationStorage, velocityDecayRate * Time.deltaTime);
            } else if(horizontalInput == 0 && verticalInput == 0 && velocity == Vector3.zero) {
                maxMovementSpeed = maxSpeedStorage;
                acceleration = accelerationStorage;
            }

                float yVelocity = Mathf.Abs(playerRigidBody.linearVelocity.y);
            if (yVelocity <= maxFallSpeed) {
                playerRigidBody.linearVelocity = new Vector3(playerRigidBody.linearVelocity.x, maxFallSpeed, playerRigidBody.linearVelocity.z);
            } 
        }
    }

    private IEnumerator ReducingMovementSpeed()
    {
        yield return null;
    }

    /// <summary>
    /// If the player is in air they will beable to move faster
    /// </summary>
    private void AirSpeedIncrease() {
        if (!isGrounded) {
            if (playerRigidBody.linearVelocity.y != 0) {
                maxMovementSpeed = maxSpeedStorage + airSpeedIncrease;
                acceleration = accelerationStorage + airSpeedIncrease;
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

    private IEnumerator GetVelocity() {
        storeVelocity = false;
        yield return new WaitForSeconds(veloctiyStorageTime);
        veloctiyStorage = playerRigidBody.linearVelocity;
        storeVelocity = true;
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
  /// checks if there is a wall to kick off of
  /// </summary>
  /// <param name="wallNormal"> Returns the wall's normal </param>
  /// <returns> Returns a boolean value depending on if a wall was hit</returns>
    private bool WallCheck(out Vector3 wallNormal) {
        if(Time.time >= timeOfLastKick + wallKickCooldown && !isGrounded) {

            timeOfLastKick = Time.time; 
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
    /// Deflects the players velocity off the wall
    /// </summary>
    /// <param name="wallNormal"> the wall's normal from the wall kick </param>
    private void WallKick(Vector3 wallNormal) {
        kickOnce = true;
        Vector3 reflectedDirection = Vector3.Reflect(veloctiyStorage, wallNormal);
        //reflectedDirection = reflectedDirection * 1000;
        Vector3 newVelocity = new Vector3(reflectedDirection.x, playerRigidBody.linearVelocity.y, reflectedDirection.z);
        playerRigidBody.linearVelocity = newVelocity;
    }
    #endregion  ========================= Wall Kick =========================

    #region     ========================= Ground Check =========================
    private void GroundCheck() {
        isGrounded = Physics.Raycast(groundCheckPosition.position, Vector3.down, groundCheckRange, groundMask);
        groundedText.text = $"Is Grounded: {isGrounded}";
    }
    /// <summary>
    /// Casts a ray to find the angle the ground is at to detect if its a slope
    /// </summary>
    /// <returns> Returns a boolean value based on the angle the ground is at </returns>
    private bool SlopeCheck(){
        if (Physics.Raycast(groundCheckPosition.position, Vector3.down, out slopeHit, groundCheckRange)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle > minSlopeAngle;
        }
        return false;
    }
    private Vector3 GetSlopeMovementDiretion() {
        return Vector3.ProjectOnPlane(moveDirection,slopeHit.normal).normalized;
    }
    #endregion  ========================= Ground Check =========================

    #region ========================= Throw Bounce Bomb =========================
    private void ValidateBounceBomb() {
        if (bounceBomb == null)
            throw new System.Exception("Cannot reference null Bounce Bomb");
        if (bounceBomb.GetComponent<BounceBomb>() == null)
            throw new System.Exception("Bounce Bomb reference doesn't have the required BounceBomb script");
    }

    private void SpawnBounceBomb() {
        // spawns BounceBomb and 'throws' it via AddForce()

        animator.SetTrigger("HasBombed");

        bounceBombInstance = Instantiate(bounceBomb, bounceBombSpawnPosition, playerRigidBody.rotation);
        bounceBombInstance.GetComponent<Rigidbody>().AddForce(GetMomentum() + Camera.main.transform.forward * bombThrowPower, ForceMode.VelocityChange);

        EventSecondaryClick.RemoveListener(SpawnBounceBomb);
        EventSecondaryClick.AddListener(DetonateBounceBomb);
    }
    private void DetonateBounceBomb() {
        if (isDetonating)
            return;

        animator.SetTrigger("HasDetonate");

        isDetonating = true;
        this.Invoke(() => {
            bounceBombInstance.GetComponent<BounceBomb>().Activate();
            Destroy(bounceBombInstance);

            EventSecondaryClick.RemoveListener(DetonateBounceBomb);
            EventSecondaryClick.AddListener(SpawnBounceBomb);
            isDetonating = false;
        }, fuseTime);       // default is 0.15f
    }
    #endregion ========================= Throw Bounce Bomb =========================

    #region ========================= Speed Stages =========================
    private void GetSpeedStage() {
        float speed = playerRigidBody.linearVelocity.magnitude;
        if (speed > secondBreakpoint) {
            // Checks if player is in Stage 3
            currentStage = 3;
            RunningLinesIntensity(maxSpeedOfLines,maxSpawnRate);
            return;
        }
        else if (speed > firstBreakpoint) {
            // Checks if player is in Stage 2
            currentStage = 2;
            RunningLinesIntensity(minSpeedOfLines, maxSpawnRate);
            return;
        }
        else { 
            // Checks if player is in stage 1
            currentStage = 1;
            runningLines.enabled = false;
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

    #region ========================= Visual Effects =========================
    /// <summary>
    /// Controls the intensity of the running lines visual effect
    /// </summary>
    /// <param name="speedOfLines"> how fast the speedlines will go </param>
    /// <param name="spawnRate"> how fast speed lines will spawn </param>
    private void RunningLinesIntensity(Vector2 speedOfLines, float spawnRate) {
        runningLines.enabled = true;
        if (runningLines.HasVector2("SpeedOfLines")) { 
            runningLines.SetVector2("SpeedOfLines",speedOfLines);
        }
        if (runningLines.HasFloat("SpawnRate")) {
            runningLines.SetFloat("SpawnRate", spawnRate);
        }
    }
    #endregion ========================= Visual Effects =========================

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

    #region  ========================= Damage Interface  =========================
    public float GetHealth() {
        return health;
        throw new System.NotImplementedException();
    }
    public void TakeDamage(float value) {
        health -= value;
        healthBar.value = health;
    }
    public void Kill() {
        Debug.LogError("Player should not be oneshotted");
        throw new System.NotImplementedException();
    }
    #endregion  ========================= Damage Interface  =========================

    #region  ========================= Death  =========================
    private bool DeathCheck() {
        if (health <= 0) {
            Time.timeScale = 0;
            gameObject.SetActive(true);
            return true;
        }
        return false;
    }
    #endregion  ========================= Death  =========================

    #region ========================= Targetable =========================
    public Transform Target {
        get => target;
    }
    #endregion ========================= Targetable =========================

    #region ========================= Gizmos =========================
    private void OnDrawGizmos() {
    }

    #endregion ========================= Gizmos =========================
}