using Unity.Hierarchy;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region     ========================= Variables =========================
    [Header("Movement")]
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float maxMovementSpeed;
    [SerializeField] private float groundDrag, airDrag;
    [SerializeField] private Transform orientation;

    private float movementSpeed;
    private float accelerationProgress = 0;

    private Rigidbody playerRigidBody;

    private float horizontalInput, verticalInput;
    private Vector3 moveDirection;
    
    [Space(10)]
    [Header("Slope Movement")]
    [SerializeField] private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitSlope;

    [Space(10)]
    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxJumpMultiplier;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airControlMultiplier;
    [Space(5)]
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private bool canJump = true;
    private bool jumpReleased;

    [Space(10)]
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPosition;
    [SerializeField] private float groundCheckRange;
    [SerializeField] private LayerMask groundCheck;
    private bool isGrounded;
    

    #endregion  ========================= Variables =========================

    void Start() {
        playerRigidBody = GetComponent<Rigidbody>();
    }


    void Update()
    {
        //Debug.Log(movementSpeed);
        PlayerMovementInput();
        MovementSpeed();
        GroundCheck();
        MaxSpeed();

        if (isGrounded) {
            coyoteTimeCounter = coyoteTime;
        }
        else {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    void FixedUpdate() {
        MovePlayer();
        VariableJump();
    }

    #region     ========================= Movement =========================

    private void PlayerMovementInput(){
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetButton("Jump") && canJump && coyoteTimeCounter > 0) {
            Jump();
            canJump = false;
            jumpReleased = false;
            exitSlope = true;

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        else if (Input.GetButtonUp("Jump") && !isGrounded) {
            jumpReleased = true;
        }
    }
    /// <summary>
    /// Gets the movement direction from orintation and applies the vertical and horizontal inputs values 
    /// Adds force to this direction
    /// Checks if grounded and adds drag if so
    /// </summary>
    private void MovePlayer() {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (SlopeCheck() && !exitSlope){
            playerRigidBody.AddForce(GetSlopeMovementDiretion() * movementSpeed * 20f, ForceMode.Force);
            if (playerRigidBody.linearVelocity.y > 0) { 
                playerRigidBody.AddForce(Vector3.down * 80f, ForceMode.Force); 
            }
        }
        if (isGrounded) {
            playerRigidBody.AddForce(moveDirection.normalized * movementSpeed * 10, ForceMode.Force);
            playerRigidBody.linearDamping = groundDrag; 
        }
        else {
            playerRigidBody.AddForce(moveDirection.normalized * movementSpeed * 10 * airControlMultiplier, ForceMode.Force);
            playerRigidBody.linearDamping = airDrag;
        }

        playerRigidBody.useGravity = !SlopeCheck();
    }

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
    /// If the player is moving faster than the max speed on any axis the velocity is set to the max speed
    /// Ensures the player does not move faster on slopes --- temporary will be replaced to increase speed on decline and decrease on incline
    /// </summary>
    private void MaxSpeed(){
        if(SlopeCheck() && !exitSlope) {

            // check if going up or down
            // increase max speed & acceleration if down
            // decrease max speed & acceleration if up

            playerRigidBody.linearVelocity = playerRigidBody.linearVelocity.normalized * movementSpeed; 
        }
        else {
            Vector3 velocity = new Vector3(playerRigidBody.linearVelocity.x, 0, playerRigidBody.linearVelocity.z);

            if(velocity.magnitude > maxMovementSpeed) { 
                Vector3 maxVelocity = velocity.normalized * maxMovementSpeed;
                playerRigidBody.linearVelocity = new Vector3(maxVelocity.x, playerRigidBody.linearVelocity.y, maxVelocity.z);
            }

            if (playerRigidBody.linearVelocity.y >= maxFallSpeed) {
                playerRigidBody.linearVelocity = new Vector3(playerRigidBody.linearVelocity.x, maxFallSpeed, playerRigidBody.linearVelocity.z);
            } 
        }
    }
    #endregion  ========================= Movement =========================

    #region     ========================= Jump =========================
    private void Jump() {
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

    #region     ========================= Ground Check =========================
    private void GroundCheck() {
        RaycastHit hit;
        isGrounded = Physics.Raycast(groundCheckPosition.position, Vector3.down, out hit, groundCheckRange, groundCheck);
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
}