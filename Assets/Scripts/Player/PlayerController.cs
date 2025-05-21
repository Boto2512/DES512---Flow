using Unity.Hierarchy;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region ========================= Variables =========================
    [Header("Movement")]
    [SerializeField] float movementSpeed;
    [SerializeField] float groundDrag;
    [SerializeField] private Transform orientation;

    private Rigidbody playerRigidBody;

    private float horizontalInput, verticalInput;
    private Vector3 moveDirection;

    [Header("Ground Check")]
    private bool isGrounded;
    [SerializeField] Transform groundCheckPosition;
    [SerializeField] float groundCheckRange;
    [SerializeField] LayerMask groundCheck;

    #endregion ========================= Variables =========================

    void Start(){
        
        playerRigidBody = GetComponent<Rigidbody>();
    }


    void Update(){

        PlayerMovementInput();
        GroundCheck();

    }

    void FixedUpdate() {
        MovePlayer();
    }


    #region ========================= Movement =========================

    private void PlayerMovementInput(){
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
    private void MovePlayer(){
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        playerRigidBody.AddForce(moveDirection.normalized * movementSpeed * 10, ForceMode.Force);
    }
    #endregion ========================= Movement =========================

    #region ========================= GroundCheck =========================

    private bool GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheckPosition.position, groundCheckRange, groundCheck);
        return isGrounded;
    }

    #endregion ========================= GroundCheck =========================
}