using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCameraConfig", menuName = "Scriptable Objects/Player Config/PlayerCameraConfig")]
public class PlayerCameraConfig : ScriptableObject {
    [Header("Sensitivity")]
    [Range(1, 120)] public float HorizontalSensitivity = 45;
    [Range(1, 120)] public float VerticalSensitivity = 45;

    [Header("Other")]
    public bool InvertHorizontalInput = false;
    public bool InvertVerticalInput = false;
    public float fovChangeSpeed;

}
