using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using UnityEngine.UI;
using UnityEngine.VFX
    ;
public class PlayerController : MonoBehaviour, IMomentumModifiable, IDamageable, ITargetable {

    private bool hasReportedMovement = false;

    #region     ========================= Variables =========================

    [SerializeField] private PlayerVariablesConfig config;

    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI groundedText;
    [Header("Health")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject gameOver;
    private float health;

    [Header("Movement")]
    [SerializeField] private float currentMaxMovementSpeed;
    [SerializeField] private float movementSpeed;

    [SerializeField] private float timeAtMaxVelocity;
    private float velocityDecayRate;
    private float veloctiyStorageTime;
    private bool doesVeloctiyTweenExist = false;
    private Tween velocityTween;



    [Space(10)]
    [SerializeField] private Transform orientation;
    private float accelerationSpeed;
    private float accelerationProgress = 0;
    private float minAccelerationProgress;
    private float reduceMaxSpeedProgress = 0;

    private Rigidbody playerRigidBody;

    private float horizontalInput, verticalInput;
    private float speedLerpProgress;

    private Vector3 moveDirection;
    private Vector3 lastAirVelocity;

    private Vector3 veloctiyStorage;
    private bool storeVelocity = true;

    private float maxSpeedStorage;
    private float accelerationStorage;

    private float dragTimer;

    private RaycastHit slopeHit;
    private bool exitSlope;
    private Vector3 inverseSlope;


    private float coyoteTimeCounter;
    private bool canJump = true;
    private bool jumpReleased;

    private bool hasJumped;
    private bool hasBombBounced;


    [Space(10)]
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPosition;
    private bool isGrounded;
    private bool groundCheckEnabled;

    [Space(10)]
    [Header("Wall Check & Kick")]
    private float timeOfLastKick;
    private bool kickOnce;

    [Space(10)]
    [Header("Attack")]
    private float attackTime;

    [Space(10)]
    [Header("Speed Stages")]
    private int currentStage = 1; //tracks which stage the player's speed is at 

    [Space(10)]
    [Header("Bounce Bomb")]
    [SerializeField] private Transform bounceBombSpawnTransform;
    private Vector3 bounceBombSpawnPosition => bounceBombSpawnTransform.position;
    private GameObject bounceBombInstance;
    private bool isDetonating = false;

    [Space(10)]
    [Header("Events")]
    [SerializeField] private UnityEvent EventPrimaryClick = new();
    [SerializeField] private UnityEvent EventSecondaryClick = new();

    [Space(10)]
    [Header("Animation Controller")]
    [SerializeField] private Animator animator;
    [SerializeField] private Animator animatorCam;

    [Space(10)]
    [Header("Visual Effects")]
    [SerializeField] private VisualEffect runningLines;
    [SerializeField] private Vector2 minSpeedOfLines, maxSpeedOfLines;
    [SerializeField] private float minSpawnRate, maxSpawnRate;

    [Space(10)]
    [Header("Target")]
    [SerializeField] private Transform target;

    [Space(10)]
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    #endregion  ========================= Variables =========================

    void Awake() {
        Globals.PLAYER = this;
    }

    void Start() {
        playerRigidBody = GetComponent<Rigidbody>();
        playerCamera = Camera.main;

        currentMaxMovementSpeed = config.defaultMaxMovementSpeed;
        maxSpeedStorage = currentMaxMovementSpeed;

        config.maxFallSpeed = -(Mathf.Abs(config.maxFallSpeed));

        currentStage = 1;
        health = config.defaultHealth;
        healthBar.maxValue = config.defaultHealth;
        healthBar.value = config.defaultHealth;

        velocityDecayRate = config.defaultVelocityDecayRate;
        accelerationSpeed = config.defaultAccelertionSpeed;
        MinAccelerationProgress();

        ValidateBounceBomb();
        EventSecondaryClick.AddListener(SpawnBounceBomb);
        EventPrimaryClick.AddListener(Attack);

        DOTween.Init();
        if (!doesVeloctiyTweenExist) {
            velocityTween = DOTween.To(() => speedLerpProgress, x => speedLerpProgress = x, 1, config.defaultVelocityDecayRate);
            velocityTween.Pause();
        }
        groundCheckEnabled = true;

        gameOver.SetActive(false);
    }

    void Update() {
        DeathCheck();
        GroundCheck();

        MovementInput();
        ActionInputs();

        SpeedControl();

        if (storeVelocity) { StartCoroutine(GetVelocity()); }
        GetSpeedStage();

        if (isGrounded) {
            coyoteTimeCounter = config.coyoteTime;
        }
        else {
            coyoteTimeCounter -= Time.deltaTime;
            GetLastAirVelocity();
        }

        speedText.text = $"Horizontal: {Utility.Horizontal(playerRigidBody.linearVelocity).magnitude}\nVertical: {Utility.Vertical(playerRigidBody.linearVelocity).magnitude}\nTotal: {playerRigidBody.linearVelocity.magnitude} ";
        //speedText.text = $"Left: {LeftSpeed()}\nRight: {RightSpeed()}\nUp: {UpSpeed()}\nDown: {DownSpeed()}\nOverall: {playerRigidBody.linearVelocity.magnitude}";

        //if (Input.GetKey(KeyCode.LeftShift)) { playerRigidBody.AddForce(orientation.forward * 3, ForceMode.Impulse); }
    }

    void FixedUpdate() {
        MovePlayer();
        VariableJump();
        PerserveMomentumOnLand();
        Acceleration();
    }

    #region ========================= Inputs =========================
    private void MovementInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetButton("Jump")) {
            if (canJump && coyoteTimeCounter > 0) {
                Jump();
                canJump = false;
                jumpReleased = false;
                exitSlope = true;
                hasJumped = true;
                timeAtMaxVelocity = 0;

                this.InvokeExclusive("Reset Jump", ResetJump, config.jumpCooldown);

                groundCheckEnabled = false;
                this.InvokeExclusive("EnableGroundCheck", EnableGroundCheck, .25f);
            }

        }
        if (Input.GetButtonUp("Jump")) {
            jumpReleased = true;
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
        if (Input.GetMouseButtonDown(0) && attackTime >= config.attackCooldown) {
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
        if (!hasReportedMovement && (horizontalInput != 0 || verticalInput != 0)) {
            hasReportedMovement = true;
            TutorialEvents.OnPlayerMoved?.Invoke();
        }

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        CounterForce();

        if (SlopeCheck() && !exitSlope) {
            playerRigidBody.AddForce(GetSlopeMovementDiretion() * movementSpeed * 10f, ForceMode.Force);
            playerRigidBody.AddForce(inverseSlope * config.downwardsForce, ForceMode.Force);
            
        }
        else if (isGrounded) {
            playerRigidBody.AddForce(moveDirection.normalized * movementSpeed * 10, ForceMode.Force);
        }
        else {
            playerRigidBody.AddForce(moveDirection.normalized * movementSpeed * 10 * config.airControlMultiplier, ForceMode.Force);
            playerRigidBody.linearDamping = 0;
        }

        playerRigidBody.useGravity = !SlopeCheck();
    }

    /// <summary>
    /// Lerps the current movement speed from 0 to the maximum movement speed when the player is using player input
    /// Does the reverse when the player is not inputing movement controls
    /// 
    /// Controls the amount of force applied to the player
    /// </summary>
    private void Acceleration() {
        if (horizontalInput != 0 || verticalInput != 0) {

            //movementSpeed = Mathf.SmoothDamp(movementSpeed, currentMaxMovementSpeed, ref accelerationSpeed, config.accelerationTime);
            movementSpeed = Mathf.Lerp(0, config.defaultMaxMovementSpeed, accelerationProgress);
            accelerationProgress += Time.deltaTime * (config.defaultAccelertionSpeed * 0.1f);
            accelerationProgress = Mathf.Clamp(accelerationProgress, 0, 1);
        }
        else if (playerRigidBody.linearVelocity.magnitude < 0.2f){
            accelerationProgress = minAccelerationProgress;
            movementSpeed = 0;
        }
    }

    private void MinAccelerationProgress() {
        minAccelerationProgress = config.startSpeed / config.defaultMaxMovementSpeed;
        accelerationProgress = minAccelerationProgress;
    }

    private void CounterForce() {
        Vector2 magnitude = FindVelRelativeToLook();

        if (isGrounded) {
            if (Mathf.Abs(magnitude.x) > 0.01f && Mathf.Abs(horizontalInput) < 0.05f || (magnitude.x < -0.01f && horizontalInput > 0) || (magnitude.x > 0.01f && horizontalInput < 0)) {
                playerRigidBody.AddForce(currentMaxMovementSpeed * orientation.right * Time.deltaTime * -magnitude.x * config.counterForce);
            }
            if (Mathf.Abs(magnitude.y) > 0.01f && Mathf.Abs(verticalInput) < 0.05f || (magnitude.y < -0.01f && verticalInput > 0) || (magnitude.y > 0.01f && verticalInput < 0)) {
                playerRigidBody.AddForce(currentMaxMovementSpeed * orientation.forward * Time.deltaTime * -magnitude.y * config.counterForce);
            }
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// </summary>
    /// <returns> Returns a vector 2 of the velocity relative to where player is looking</returns>
    public Vector2 FindVelRelativeToLook() {
        float lookAngle = orientation.eulerAngles.y;
        float moveAngle = Mathf.Atan2(playerRigidBody.linearVelocity.x, playerRigidBody.linearVelocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitude = playerRigidBody.linearVelocity.magnitude;
        float yMaagnitude = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMagnitude = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMagnitude, yMaagnitude);
    }

    private void MaxFallSpeed() {
        float yVelocity = playerRigidBody.linearVelocity.y;
        if (yVelocity <= config.maxFallSpeed) {
            playerRigidBody.linearVelocity = new Vector3(playerRigidBody.linearVelocity.x, config.maxFallSpeed, playerRigidBody.linearVelocity.z);
        }
    }

    private void SpeedControl() {
        Vector3 velocity = new Vector3(playerRigidBody.linearVelocity.x, 0, playerRigidBody.linearVelocity.z);

        if (!isGrounded) { MaxFallSpeed(); }


        if (SlopeCheck() && !exitSlope) {
            if (playerRigidBody.linearVelocity.magnitude > currentMaxMovementSpeed) {
                playerRigidBody.linearVelocity = playerRigidBody.linearVelocity.normalized * currentMaxMovementSpeed;
            };
        }

        if (velocity.magnitude > currentMaxMovementSpeed) {
            Vector3 velocityNormalized = velocity.normalized * currentMaxMovementSpeed;
            playerRigidBody.linearVelocity = new Vector3(velocityNormalized.x, playerRigidBody.linearVelocity.y, velocityNormalized.z);
        }

        if (hasBombBounced) { return; }
        else if (!isGrounded && hasJumped && CheckIfSpeedIncreases(config.airSpeedIncrease)) {
            if (playerRigidBody.linearVelocity.y != 0) {
                currentMaxMovementSpeed = config.defaultMaxMovementSpeed + config.airSpeedIncrease;
                timeAtMaxVelocity = 0;
            }
        }
        else if (SlopeCheck() && CheckIfSpeedIncreases(config.slopeSpeedImpact)) {
            if (playerRigidBody.linearVelocity.y < 0) {
                Debug.Log("INCREASING SPEED DUE TO SLOPE");
                currentMaxMovementSpeed = config.defaultMaxMovementSpeed + config.slopeSpeedImpact;
                timeAtMaxVelocity = 0;
            }
        }
        ////Debug.Log(currentMaxMovementSpeed +" " + config.defaultMaxMovementSpeed + config.airSpeedIncrease +" "+ config.defaultMaxMovementSpeed + config.boostedMaxMovementSpeed);
        //float airSpeed = config.defaultMaxMovementSpeed + config.boostedMaxMovementSpeed;
        //float boostedSpeed = config.defaultMaxMovementSpeed + config.airSpeedIncrease;
        //if (currentMaxMovementSpeed >= airSpeed && currentMaxMovementSpeed <= boostedSpeed)
        //{
        //    //Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\nAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        //    if (playerRigidBody.linearVelocity.magnitude < currentMaxMovementSpeed) { currentMaxMovementSpeed = playerRigidBody.linearVelocity.magnitude; }

        //}
        //else if 
        if (horizontalInput != 0 || verticalInput != 0 && currentMaxMovementSpeed != config.defaultMaxMovementSpeed) {
                ReduceMaxSpeed();
        }
        else {
            currentMaxMovementSpeed = config.defaultMaxMovementSpeed;
            timeAtMaxVelocity = 0;
        }

        if (Mathf.Approximately(currentMaxMovementSpeed, config.defaultMaxMovementSpeed)) { timeAtMaxVelocity = 0; }
    }

    private void ReduceMaxSpeed() {
        timeAtMaxVelocity += Time.deltaTime;

        if (timeAtMaxVelocity > config.maxVeloctiyDuration) {

            currentMaxMovementSpeed = Mathf.SmoothDamp(currentMaxMovementSpeed, config.defaultMaxMovementSpeed, ref velocityDecayRate, config.maxVeloctiyDuration);
        }
    }

    private bool CheckIfSpeedIncreases(float increaseValue) {
        if (config.defaultMaxMovementSpeed + increaseValue > currentMaxMovementSpeed) {
            return true;
        }
        else { return false; }
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
        playerRigidBody.linearVelocity = new Vector3(playerRigidBody.linearVelocity.x, 0, playerRigidBody.linearVelocity.z);
        playerRigidBody.AddForce(transform.up * config.jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Applies a force to the player after the player has released the jump button to make it reach the apex of the jump earlier
    /// Applies a force when the player is falling to make them fall faster
    /// </summary>
    private void VariableJump() {
        /*//Debug.Log($"Variable Jump: jumpReleased: ${jumpReleased},Y Velocity: ${playerRigidBody.linearVelocity.y} ");
        if (jumpReleased && !isGrounded && playerRigidBody.linearVelocity.y > 0) {
            playerRigidBody.AddForce(Vector3.down * config.maxJumpMultiplier, ForceMode.Force);
            //Debug.Log("Variable Jump - rise");
        }
        else */
        if (!isGrounded && playerRigidBody.linearVelocity.y < 0 && !hasBombBounced) {
            playerRigidBody.AddForce(Vector3.down * config.fallMultiplier, ForceMode.Force);
            //Debug.Log("Variable Jump - fall");
        }
        else if (!isGrounded && playerRigidBody.linearVelocity.y < 0 && hasBombBounced) {

            playerRigidBody.AddForce(Vector3.down * config.bombFallMultiplier, ForceMode.Force);
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
        if (Time.time >= timeOfLastKick + config.wallKickCooldown && !isGrounded) {
            timeOfLastKick = Time.time;
            RaycastHit wallHit;
            bool isWall = Physics.Raycast(groundCheckPosition.position, orientation.forward, out wallHit, config.wallKickRange, config.groundMask);
            Debug.DrawRay(groundCheckPosition.position, orientation.forward * config.wallKickRange, Color.blueViolet, 10);

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
        if (groundCheckEnabled) {
            isGrounded = Physics.Raycast(groundCheckPosition.position, Vector3.down, config.groundCheckRange, config.groundMask);
            if (isGrounded) {
                hasJumped = false;
                hasBombBounced = false;
            }
        }
        //groundedText.text = $"Is Grounded: {isGrounded}";
    }

    private void EnableGroundCheck() {
        groundCheckEnabled = true;
    }


    /// <summary>
    /// Casts a ray to find the angle the ground is at to detect if its a slope
    /// </summary>
    /// <returns> Returns a boolean value based on the angle the ground is at </returns>
    private bool SlopeCheck() {
        if (Physics.Raycast(groundCheckPosition.position, Vector3.down, out slopeHit, config.groundCheckRange)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            inverseSlope = -slopeHit.normal;
            return angle < config.maxSlopeAngle && angle > config.minSlopeAngle;
        }
        return false;
    }

    private Vector3 GetSlopeMovementDiretion() {
        Vector3 temp = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
        Debug.DrawRay(transform.position, temp, Color.green, 5f);
        return temp;
    }
    #endregion  ========================= Ground Check =========================

    #region ========================= Throw Bounce Bomb =========================
    private void ValidateBounceBomb() {
        if (config.bounceBomb == null)
            throw new System.Exception("Cannot reference null Bounce Bomb");
        if (config.bounceBomb.GetComponent<BounceBomb>() == null)
            throw new System.Exception("Bounce Bomb reference doesn't have the required BounceBomb script");
    }

    private void SpawnBounceBomb() {
        // spawns BounceBomb and 'throws' it via AddForce()

        animator.SetTrigger("hasBombed");
        animatorCam.SetTrigger("hasBombed");

        bounceBombInstance = Instantiate(config.bounceBomb, bounceBombSpawnPosition, playerRigidBody.rotation);
        bounceBombInstance.GetComponent<Rigidbody>().AddForce(GetMomentum() + Camera.main.transform.forward * config.bombThrowPower, ForceMode.VelocityChange);

        EventSecondaryClick.RemoveListener(SpawnBounceBomb);
        EventSecondaryClick.AddListener(DetonateBounceBomb);
    }

    private void DetonateBounceBomb() {
        if (isDetonating)
            return;

        animator.SetTrigger("hasDetonate");
        animatorCam.SetTrigger("hasDetonate");
        TutorialEvents.enemyKilledVeryFast?.Invoke();

        isDetonating = true;
        this.InvokeExclusive("detonate", () => {
            bounceBombInstance.GetComponent<BounceBomb>().Activate();
            Destroy(bounceBombInstance);

            EventSecondaryClick.RemoveListener(DetonateBounceBomb);
            EventSecondaryClick.AddListener(SpawnBounceBomb);
            isDetonating = false;
        }, config.fuseTime);       // default is 0.15f
    }
    #endregion ========================= Throw Bounce Bomb =========================

    #region ========================= Speed Stages =========================
    private void GetSpeedStage() {
        Vector2 horizontalVelocity = new Vector2(playerRigidBody.linearVelocity.x, playerRigidBody.linearVelocity.z);
        float horizontalSpeed = horizontalVelocity.magnitude;
        if (horizontalSpeed > config.secondBreakpoint) {
            // Checks if player is in Stage 3
            currentStage = 3;
            RunningLinesIntensity(maxSpeedOfLines, maxSpawnRate);
            return;
        }
        else if (horizontalSpeed > config.firstBreakpoint) {
            // Checks if player is in Stage 2
            currentStage = 2;
            RunningLinesIntensity(minSpeedOfLines, minSpawnRate);
            return;
        }
        else {
            // Checks if player is in stage 1
            currentStage = 1;
            runningLines.enabled = false;
            return;
        }
    }

    private void ChangeFOV() {
        
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
        animatorCam.SetTrigger("hasAttacked");

        if (currentStage == 3) {
            // larger aoe && oneshot && vfx
            //VFX GOES HERE
            Vector3 largerScale = config.attackScale * 2;
            enemies = AttackCheck(largerScale, config.attackRange + 2, Color.red);
        }
        else if (currentStage == 2) {
            //one shot & VFX
            //VFX GOES HERE
            enemies = AttackCheck(config.attackScale, config.attackRange, Color.yellow);
        }
        else {
            // default
            enemies = AttackCheck(config.attackScale, config.attackRange, Color.green);
        }

        if (enemies.Length != 0) {
            foreach (RaycastHit enemy in enemies) {
                enemy.transform.TryGetComponent<IDamageable>(out IDamageable damage);
                enemy.transform.TryGetComponent<IMomentumModifiable>(out IMomentumModifiable knockback);
                if (damage != null) {
                    if (currentStage >= 2) {
                        damage.Kill();

                        TutorialEvents.enemyKilledVeryFast?.Invoke();
                    }
                    else {
                        damage.TakeDamage(config.attackDamage);
                        TutorialEvents.OnEnemyKilled?.Invoke();
                    }
                }
                if (knockback != null)
                {
                    knockback.SetMomentum(Knockback(knockback.GetPosition()));
                }
                
            }
        }
    }
    private Vector3 Knockback(Vector3 enemy){
        Vector3 direction = (enemy - groundCheckPosition.position).normalized * config.attackKnockback;
        Debug.DrawRay(groundCheckPosition.position, direction, Color.yellow, 10f);
        return direction;
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
        enemies = Physics.BoxCastAll(direction, attackSize, Camera.main.transform.forward, Quaternion.identity, range, config.attackMask);
        Debug.DrawRay(direction, Camera.main.transform.forward * config.attackRange, colour, 3f);
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
            runningLines.SetVector2("SpeedOfLines", speedOfLines);
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
        return target.position;
    }

    public void SetMomentum(Vector3 value) {
        playerRigidBody.linearVelocity = value;

        float addedSpeed = config.defaultMaxMovementSpeed + value.magnitude;

        if (addedSpeed >= config.boostedMaxMovementSpeed) {
            addedSpeed = config.boostedMaxMovementSpeed;
        }
        if (CheckIfSpeedIncreases(addedSpeed)) {
            currentMaxMovementSpeed = config.defaultMaxMovementSpeed + addedSpeed;
        }

        hasBombBounced = true;
        groundCheckEnabled = false;
        this.InvokeExclusive("EnableGroundCheck", EnableGroundCheck, 0.1f);
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

    public void Heal(float value) {
        if (health + value <= healthBar.maxValue) {
            health += value;
        }
        else {
            health = healthBar.maxValue;
        }
    }
    #endregion  ========================= Damage Interface  =========================

    #region  ========================= Death  =========================
    private void DeathCheck() {
        if (health <= 0 && !gameOver.activeSelf) {
            gameOver.SetActive(true);
            Time.timeScale = 0;
        }
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

    #region ========================= Animation Interface =========================

    public float LeftSpeed() {
        Vector3 leftDirection = -orientation.right;
        float leftwardsSpeed = Vector3.Dot(playerRigidBody.linearVelocity, leftDirection);

        return Mathf.Max(leftwardsSpeed, 0f);
    }

    public float RightSpeed() {
        float rightwardsSpeed = Vector3.Dot(playerRigidBody.linearVelocity, orientation.right);

        return Mathf.Max(rightwardsSpeed, 0f);
    }

    public float UpSpeed() {
        return Mathf.Max(playerRigidBody.linearVelocity.y, 0f);
    }

    public float DownSpeed() {
        return -Mathf.Min(playerRigidBody.linearVelocity.y, 0f);
    }

    #endregion ========================= Animation Interface =========================


    #region old movement code
    ///this code has been banished let it be forgotten, pray we do not need it one day

    /// <summary>
    /// not used currently
    /// adjusts player drag depending on their input & state
    /// </summary>
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
        else if (horizontalInput != 0 || verticalInput != 0 && playerRigidBody.linearVelocity.magnitude < currentMaxMovementSpeed) {
            playerRigidBody.linearDamping = 0;
        }
        else {
            dragTimer = 0;
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

        if (SlopeCheck() && !exitSlope) {
            if (playerRigidBody.linearVelocity.magnitude > currentMaxMovementSpeed) {
                playerRigidBody.linearVelocity = playerRigidBody.linearVelocity.normalized * currentMaxMovementSpeed;
            }
        }
        else {
            timeAtMaxVelocity += Time.deltaTime;

            if (timeAtMaxVelocity > config.maxVeloctiyDuration) {
                ClampVelocity(velocity);
            }
            MaxFallSpeed();
        }
    }

    private void ClampVelocity(Vector3 velocity) {
        if (velocity.magnitude > currentMaxMovementSpeed) {
            Vector3 velocityNormalized = velocity.normalized * currentMaxMovementSpeed;
            playerRigidBody.linearVelocity = new Vector3(velocityNormalized.x, playerRigidBody.linearVelocity.y, velocityNormalized.z);
        }

        if (currentMaxMovementSpeed > maxSpeedStorage) {
            currentMaxMovementSpeed = Mathf.MoveTowards(currentMaxMovementSpeed, maxSpeedStorage, config.defaultVelocityDecayRate * Time.deltaTime);
            //config.acceleration = Mathf.MoveTowards(config.acceleration, accelerationStorage, config.velocityDecayRate * Time.deltaTime);
        }
        else if (horizontalInput == 0 && verticalInput == 0 && velocity == Vector3.zero) {
            currentMaxMovementSpeed = maxSpeedStorage;
            //config.acceleration = accelerationStorage;
        }
    }


    /// <summary>
    /// If the player is in air they will beable to move faster
    /// </summary>
    private void AirSpeedIncrease() {
        if (!isGrounded) {
            if (playerRigidBody.linearVelocity.y != 0) {
                currentMaxMovementSpeed = maxSpeedStorage + config.airSpeedIncrease;
                //config.acceleration = accelerationStorage + config.airSpeedIncrease;
            }
        }
    }
    /// <summary>
    /// If the player is on a slope they will be able to move faster
    /// </summary>
    private void SlopeSpeedIncrease() {
        if (SlopeCheck()) {
            if (playerRigidBody.linearVelocity.y < 0) {
                currentMaxMovementSpeed = maxSpeedStorage + config.slopeSpeedImpact;
                //config.acceleration = accelerationStorage + config.slopeSpeedImpact;
            }
        }
    }
    #endregion
}