using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCameraConfig", menuName = "Scriptable Objects/PlayerCameraConfig")]
public class PlayerCameraConfig : ScriptableObject {
    [Header("Sensitivity")]
    [Range(1, 120)] public int HorizontalSensitivity = 45;
    [Range(1, 120)] public int VerticalSensitivity = 45;

    [Header("Other")]
    public bool InvertHorizontalInput = false;
    public bool InvertVerticalInput = false;

}
