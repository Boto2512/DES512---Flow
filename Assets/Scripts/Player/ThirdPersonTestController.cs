using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonTestController : MonoBehaviour {

    [SerializeField, Range(1, 120)] private int sensivity = 60;

    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool attackPressed;
    private bool throwPressed;
    private bool jumpInput;

    private Rigidbody rb;

    private Transform cameraStand;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;

        rb = this.GetComponent<Rigidbody>();
        cameraStand = this.transform.Find("CameraStand");
        if (cameraStand == null) {
            throw new System.Exception("ThirdPersonTestController requires a CameraStand child GameObject");
        }
    }

    // Update is called once per frame
    void Update() {
        
    }

    #region Input

    public void Move(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context) {
        lookInput = context.ReadValue<Vector2>();

        cameraStand.Rotate(Vector3.up, lookInput.y * 0.02f * sensivity);
        cameraStand.Rotate(Vector3.right, lookInput.x * 0.02f * sensivity);

        //cameraStand.Rotate(lookInput.y * 0.02f * sensivity, lookInput.x * 0.02f * sensivity, 0f);
    }

    public void Attack(InputAction.CallbackContext context) {
        if (context.started) {
            attackPressed = true;
        }
        else if (context.canceled) {
            attackPressed = false;
        }
    }

    public void Throw(InputAction.CallbackContext context) {
        if (context.started) {
            throwPressed = true;
        }
        else if (context.canceled) {
            throwPressed = false;
        }
    }

    public void Jump(InputAction.CallbackContext context) {
        if (context.started) {
            jumpInput = true;
        }
        else if (context.canceled) {
            jumpInput = false;
        }
    }

    #endregion Input
}
