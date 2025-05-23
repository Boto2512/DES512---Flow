using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform orientationTransform;
    [SerializeField] private float xAxisSensitivity, yAxisSensitivity;
    private float xAxisRotation, yAxisRotation;

    #region ========================= Camera =========================

    private void Start(){
        /*
         Cursor.lockState = CursorLockMode.Locked;
         Cursor.visible = false;
         */
    }
    /// <summary>
    /// Gets mouse input and uses it to rotate the player camera
    /// </summary>
    private void Update() { 
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * xAxisSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * yAxisSensitivity;

        xAxisRotation -= mouseY;
        xAxisRotation = Mathf.Clamp(xAxisRotation, -90f, 90f);

        yAxisRotation += mouseX;
        transform.rotation = Quaternion.Euler(xAxisRotation, yAxisRotation, 0);
        orientationTransform.rotation = Quaternion.Euler(0, yAxisRotation, 0);
    }

    #endregion ========================= Camera =========================
}
