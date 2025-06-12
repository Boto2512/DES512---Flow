using UnityEngine;

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
    }
}