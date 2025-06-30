using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControllerConfig", menuName = "Scriptable Objects/PlayerControllerConfig")]
public class PlayerControllerConfig : ScriptableObject {

    [Header("Momentum Storage")]
    public float SlideTime = 0.1f;

    [Header("Movement")]
    [Min(0f)] public float Acceleration = 16f;
    [Min(0f)] public float AirAcceleration = 16f;
    [Tooltip("Only applies to horizontal acceleration")]
    [Min(0f)] public float MaxAcceleration = 32f;
    [Min(0f)] public float AirDrag = 1f;
    [Min(0f)] public float GroundDrag = 2f;
    [Min(0f)] public float StoppingDrag = 6f;

    [Header("Jump")]
    [Min(0f)] public float JumpForce = 7.5f;
    [Min(0f)] public float CoyoteTime = 0.2f;
    [Min(0f)] public float MinFallAcceleration = 10f;
    [Min(0f)] public float MaxFallAcceleration = 100f;
    [Tooltip("How long spent falling in seconds it takes to reach MaxFallAcceleration from MinFallAcceleration")]
    [Min(0f)] public float MaxFallAccelerationTime = 5f;

    [Header("Slope")]
    [Min(0f)] public float MinSlopeAngle = 5f;
    [Min(0f)] public float MaxSlopeAngle = 45f;
    [Min(0f)] public float SlopeSpeedMultiplier = 3f;

    [Header("Camera")]
    [Range(1, 120)] public int Sensitivity = 60;
}
