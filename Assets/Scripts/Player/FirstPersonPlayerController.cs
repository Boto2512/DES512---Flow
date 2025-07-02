using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FirstPersonCameraController))]
[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerAttackController))]
[RequireComponent(typeof(BBThrowController))]
public class FirstPersonPlayerController : MonoBehaviour, ITargetable {

    #region Facets of Character

    private Rigidbody rb;
    private FirstPersonCameraController cameraController;
    private PlayerMovementController movementController;
    private PlayerAttackController attackController;
    private BBThrowController throwController;

    [SerializeField] private GameObject model;

    #endregion Facets of Character

    #region Events

    [Header("Events")]
    public UnityEvent PrimaryAction = new();
    public UnityEvent SecondaryAction = new();

    #endregion Events

    [Header("Target")]
    [SerializeField] private Transform targetPosition;
    public Transform Target => targetPosition;

    private void Awake() {
        rb = this.GetComponent<Rigidbody>();
        cameraController = this.GetComponent<FirstPersonCameraController>();
        movementController = this.GetComponent<PlayerMovementController>();
        attackController = this.GetComponent<PlayerAttackController>();
        throwController = this.GetComponent<BBThrowController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        Globals.PLAYER = this;
    }

    // Update is called once per frame
    void Update() {

    }

    #region Input

    public void MoveInput(InputAction.CallbackContext context) {
        movementController.MoveInput(context.ReadValue<Vector2>());
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
        movementController.JumpInput(context.ReadValueAsButton());
    }

    #endregion Input
}
