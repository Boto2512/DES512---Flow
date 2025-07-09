using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Scriptable Objects/Player Config/PlayerMovementConfig")]
public class PlayerMovementConfig : ScriptableObject {

    [Header("Momentum Storage")]
    [Min(0f)] public float SlideTime = 0.1f;

    [Header("Acceleration")]
    [Min(0f)] public float Acceleration = 16f;
    [Min(0f)] public float AirAcceleration = 16f;
    [Tooltip("Only applies to horizontal acceleration")]
    [Min(0f)] public float MaxAcceleration = 32f;
    [Tooltip("Time spent over MaxAcceleration before beginning decelerating back to MaxAcceleration")]
    [Min(0f)] public float TimeToDecelerate = 2f;

    [Space(10)]
    [Min(0f)] public float CounterStrafeMultiplier = 2f;
    [Tooltip("The angle allowed either side of -180deg from the current velocity for the CounterStrafeMultiplier to be applied")]
    [Min(0f)] public float CounterStrafeAngleError = 15f;

    [Header("Drag")]
    [Min(0f)] public float AirDrag = 1f;
    [Min(0f)] public float GroundDrag = 2f;
    [Tooltip("The drag applied when no inputs are being pressed")]
    [Min(0f)] public float StoppingDrag = 6f;
    [Tooltip("The angle between the input and current velocity at which air drag won't be applied whilst bunny hopping")]
    [Min(0f)] public float KeepMomentumDespiteInputAngle = 25f;

    [Header("Jump")]
    [Min(0f)] public float JumpForce = 7.5f;
    [Min(0f)] public float CoyoteTime = 0.2f;

    [Space(10)]
    [Min(0f)] public float MinFallAcceleration = 10f;
    [Min(0f)] public float MaxFallAcceleration = 100f;
    [Tooltip("How long spent falling in seconds it takes to reach MaxFallAcceleration from MinFallAcceleration")]
    [Min(0f)] public float MaxFallAccelerationTime = 5f;

    [Header("Slope")]
    [Min(0f)] public float MinSlopeAngle = 5f;
    [Min(0f)] public float MaxSlopeAngle = 45f;
    [Min(0f)] public float SlopeSpeedMultiplier = 3f;

    [Header("Wall Kick")]
    [Range(0f, 20f)] public float WallKickDistance = 2f;
    [Tooltip("Determines whether falling wall kicks continue downwards or force the player to change vertical direction and go upwards")]
    public bool AlwaysWallKickUpwards = true;
    public bool UseWallKickFixedVerticalSpeed = true;
    [Min(0f)] public float WallKickFixedVerticalSpeed = 8f;
    public float MinWallKickAngle = 0f;
    public float MaxWallKickAngle = 30f;

}
