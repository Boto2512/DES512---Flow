using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonTestController : MonoBehaviour {

    private Vector2 movementInput = Vector2.zero;

    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        
    }

    #region Input

    private void GetInputs() {
        
    }

    private void MovementInput() {
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
    }

    #endregion Input
}
