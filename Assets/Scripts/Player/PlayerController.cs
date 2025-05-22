using Unity.Hierarchy;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region     ========================= Variables =========================
    [Header("Movement")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float groundDrag, airDrag;
    [SerializeField] private Transform orientation;

    private Vector3 velocity;

    private Rigidbody playerRigidBody;

    private float horizontalInput, verticalInput;
    private Vector3 moveDirection;

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
        velocity = playerRigidBody.linearVelocity;
        PlayerMovementInput();
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
    private void MovePlayer(){
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        if (isGrounded) { 
            playerRigidBody.AddForce(moveDirection.normalized * movementSpeed * 10, ForceMode.Force);
            playerRigidBody.linearDamping = groundDrag; 
        }
        else {
            playerRigidBody.AddForce(moveDirection.normalized * movementSpeed * 10 * airControlMultiplier, ForceMode.Force);
            playerRigidBody.linearDamping = airDrag;
        }
    }

    /// <summary>
    /// If the player is moving faster than the max speed on any axis the velocity is set to the max speed
    /// </summary>
    private void MaxSpeed(){
        Vector3 velocity = new Vector3(playerRigidBody.linearVelocity.x, 0, playerRigidBody.linearVelocity.z);
        if(velocity.magnitude > movementSpeed) { 
            Vector3 maxVelocity = velocity.normalized * movementSpeed;
            playerRigidBody.linearVelocity = new Vector3(maxVelocity.x, playerRigidBody.linearVelocity.y, maxVelocity.z);
        }
        if (playerRigidBody.linearVelocity.y >= maxFallSpeed) {
            playerRigidBody.linearVelocity = new Vector3(playerRigidBody.linearVelocity.x, maxFallSpeed, playerRigidBody.linearVelocity.z);
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
            playerRigidBody.AddForce(Vector3.down * maxJumpMultiplier);
        }
        else if (jumpReleased && !isGrounded && playerRigidBody.linearVelocity.y < 0) {
            playerRigidBody.AddForce(Vector3.down * fallMultiplier);
        }
    }

    private void ResetJump() {
        canJump = true;
    }

    #endregion  ========================= Jump =========================

    #region     ========================= GroundCheck =========================

    private void GroundCheck() {
        RaycastHit hit;
        isGrounded = Physics.Raycast(groundCheckPosition.position, Vector3.down, out hit, groundCheckRange, groundCheck);
        Debug.Log(hit.collider);

    }

    #endregion  ========================= GroundCheck =========================
}