using UnityEngine;

public class NewViewModelSway : MonoBehaviour {
    /// use "pc.LeftSpeed()" for the speed of the player going leftwards
    /// it's the same for Right, Up, and Down, just replace Left in pc.LeftSpeed()

    [SerializeField] private FirstPersonPhysicalAnimationController controller;

    [Header("Objects")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform tiltTransform;
    [SerializeField] private Transform swayTransform;

    [Header("Sway Settings")]
    [SerializeField] private float smooth;
    [SerializeField] private float multiplier;

    [Header("Strafe Tilt Settings")]
    [SerializeField] private float tiltStrafeSmooth;
    [SerializeField] private float tiltStrafeMultiplier;

    [Header("Vertical Tilt Settings")]
    [SerializeField] private float tiltVerticalSmooth;
    [SerializeField] private float tiltVerticalMultiplier;

    private void Start() {
        this.transform.SetParent(cameraTransform);
        this.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void Update() {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * multiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * multiplier;

        // calculate target rotation
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        // rotate 
        swayTransform.localRotation = Quaternion.Slerp(swayTransform.localRotation, targetRotation, smooth * Time.deltaTime);

        // tilt
        // get movement values
        float movementLeft = Mathf.Clamp(controller.LeftSpeed(), 0, 1) * -tiltStrafeMultiplier;
        float movementRight = Mathf.Clamp(controller.RightSpeed(), 0, 1) * tiltStrafeMultiplier;
        float movementUp = Mathf.Clamp(controller.UpSpeed(), 0, 1) * -tiltVerticalMultiplier;
        float movementDown = Mathf.Clamp(controller.DownSpeed(), 0, 1) * tiltVerticalMultiplier;

        // calculate target rotation
        Quaternion movementRotationLeft = Quaternion.AngleAxis(movementLeft, Vector3.back);
        Quaternion movementRotationRight = Quaternion.AngleAxis(movementRight, Vector3.back);
        Quaternion movementRotationUp = Quaternion.AngleAxis(movementUp, Vector3.left);
        Quaternion movementRotationDown = Quaternion.AngleAxis(movementDown, Vector3.left);

        Quaternion targetStrafeRotation = movementRotationLeft * movementRotationRight;
        Quaternion targetVerticalRotation = movementRotationUp * movementRotationDown;

        // rotate
        tiltTransform.localRotation = Quaternion.Slerp(tiltTransform.localRotation, targetStrafeRotation, tiltStrafeSmooth * Time.deltaTime);
        tiltTransform.localRotation = Quaternion.Slerp(tiltTransform.localRotation, targetVerticalRotation, tiltVerticalSmooth * Time.deltaTime);
    }
}
