using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonPhysicalAnimationController : MonoBehaviour {

    [SerializeField] private Transform orientation;

    private Vector2 mouseInput = Vector2.zero;

    private Rigidbody rb;

    private void Awake() {
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        //mouseInput = Vector2.zero;
    }

    public Vector2 MouseMovement() => mouseInput;

    public float LeftSpeed() => Mathf.Max(Vector3.Dot(rb.linearVelocity, -orientation.right), 0f);
    public float RightSpeed() => Mathf.Max(Vector3.Dot(rb.linearVelocity, orientation.right), 0f);
    public float UpSpeed() => Mathf.Max(rb.linearVelocity.y, 0f);
    public float DownSpeed() => -Mathf.Min(rb.linearVelocity.y, 0f);

    #region Input

    // in case input needs to be tied specifically to this script
    public void MouseInput(InputAction.CallbackContext context) {
        MouseInput(context.ReadValue<Vector2>());
    }

    public void MouseInput(Vector2 input) {
        mouseInput += input;
    }

    #endregion Input
}
