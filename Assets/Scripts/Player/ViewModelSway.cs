using UnityEngine;
using UnityEngine.Rendering;

public class WeaponSway : MonoBehaviour
{
    /// use "pc.LeftSpeed()" for the speed of the player going leftwards
    /// it's the same for Right, Up, and Down, just replace Left in pc.LeftSpeed()
    /// 
    [Header("Tilt")]
    [SerializeField] private PlayerController playerController;

    [Header("Sway Settings")]
    [SerializeField] private float smooth;
    [SerializeField] private float multiplier;

    [Header("Strafe Tilt Settings")]
    [SerializeField] private float tiltStrafeSmooth;
    [SerializeField] private float tiltStrafeMultiplier;

    [Header("Vertical Tilt Settings")]
    [SerializeField] private float tiltVerticalSmooth;
    [SerializeField] private float tiltVerticalMultiplier;

    private void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * multiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * multiplier;

        // calculate target rotation
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        // rotate 
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);

        // tilt
        // get movement values
        float movementLeft = Mathf.Clamp(playerController.LeftSpeed(), 0, 1) * -tiltStrafeMultiplier;
        float movementRight = Mathf.Clamp(playerController.RightSpeed(), 0, 1) * tiltStrafeMultiplier;
        float movementUp = Mathf.Clamp(playerController.UpSpeed(), 0, 1) * -tiltVerticalMultiplier;
        float movementDown = Mathf.Clamp(playerController.DownSpeed(), 0 ,1) * tiltVerticalMultiplier;

        // calculate target rotation
        Quaternion movementRotationLeft = Quaternion.AngleAxis(movementLeft, Vector3.back);
        Quaternion movementRotationRight = Quaternion.AngleAxis(movementRight, Vector3.back);
        Quaternion movementRotationUp = Quaternion.AngleAxis(movementUp, Vector3.left);
        Quaternion movementRotationDown = Quaternion.AngleAxis(movementDown, Vector3.left);

        Quaternion targetStrafeRotation = movementRotationLeft * movementRotationRight;
        Quaternion targetVerticalRotation = movementRotationUp * movementRotationDown;

        // rotate
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetStrafeRotation, tiltStrafeSmooth * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetVerticalRotation, tiltVerticalSmooth * Time.deltaTime);
    }
}