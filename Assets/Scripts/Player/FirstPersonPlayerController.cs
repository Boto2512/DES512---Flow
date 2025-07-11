using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FirstPersonCameraController))]
[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerAttackController))]
[RequireComponent(typeof(BBThrowController))]
[RequireComponent(typeof(FirstPersonPhysicalAnimationController))]
public class FirstPersonPlayerController : MonoBehaviour, ITargetable, IHasSpeedThresholds {

    private static FirstPersonPlayerController instance;

    [SerializeField] private PlayerControllerConfig Config;
    public SpeedStageThreshold CurrentThreshold { get; private set; }

    #region Facets of Character

    private Rigidbody rb;
    private FirstPersonCameraController cameraController;
    private PlayerMovementController movementController;
    private PlayerAttackController attackController;
    private BBThrowController throwController;
    private FirstPersonPhysicalAnimationController animationController;

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
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this) {
            Debug.LogWarning("Persistent player already exists, destroying this one", this);
            Destroy(this.gameObject);
        }

        rb = this.GetComponent<Rigidbody>();
        cameraController = this.GetComponent<FirstPersonCameraController>();
        movementController = this.GetComponent<PlayerMovementController>();
        attackController = this.GetComponent<PlayerAttackController>();
        throwController = this.GetComponent<BBThrowController>();
        animationController = this.GetComponent<FirstPersonPhysicalAnimationController>();
        CurrentThreshold = Config.Thresholds.First();
        Globals.PLAYER = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    private void FixedUpdate() {
        FindCurrentThreshold();
    }

    #region Thresholds

    private void FindCurrentThreshold() {
        if (!Config.Thresholds.Any())
            return;

        for (int i = 1; i < Config.Thresholds.Count; ++i) {
            if (rb.linearVelocity.magnitude < Config.Thresholds[i].SpeedThreshold) {
                CurrentThreshold = Config.Thresholds[i - 1];
                return;
            }
        }

        CurrentThreshold = Config.Thresholds.Last();
    }

    #endregion Thresholds

    #region Input

    public void MoveInput(InputAction.CallbackContext context) {
        movementController.MoveInput(context.ReadValue<Vector2>());
    }

    public void LookInput(InputAction.CallbackContext context) {
        animationController.MouseInput(context.ReadValue<Vector2>());
    }

    public void AttackInput(InputAction.CallbackContext context) {
        if (context.performed) {
            PrimaryAction.Invoke();
        }
        else if (context.canceled) {
        }
    }

    public void ThrowInput(InputAction.CallbackContext context) {
        if (context.performed) {
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
