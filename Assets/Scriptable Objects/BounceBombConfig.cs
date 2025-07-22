using UnityEngine;

[CreateAssetMenu(fileName = "BounceBombConfig", menuName = "Scriptable Objects/BounceBombConfig")]
public class BounceBombConfig : ScriptableObject {

    [Header("Weak Blast")]
    [Min(0f)] public float WeakBlastRadius = 7f;
    [Min(0f)] public float WeakBlastPower = 2.5f;
    [Min(0f)] public float WeakSpeedMinimum = 5f;

    [Header("Strong Blast")]
    [Min(0f)] public float StrongBlastRadius = 4f;
    [Min(0f)] public float StrongBlastPower = 1.5f;
    [Min(0f)] public float StrongSpeedMinimum = 10f;

    [Header("Vertical Gain")]
    public bool UseFixedVerticalSpeed = false;
    [Tooltip("The fixed amount of vertical speed the player inherits when using the Bounce Bomb")]
    [Min(0f)] public float FixedVerticalSpeed = 3f;
    [Tooltip("The minimum vertical speed the IMomentumModifiable can be at after FixedVerticalSpeed is applied")]
    public float VerticalGainMinimum = 3f;
    [Tooltip("The ratio between horizontal momentum gain and vertical momentum gain (1 is equal gain, 0 is no vertical gain). Ignored when UseFixedVerticalSpeed is true")]
    [Range(0f, 1f)] public float VerticalGainRatio = 0.5f;

    [Header("Other")]
    [Tooltip("How far ahead the momentum projects the entity position")]
    [Min(0f)] public float EntityProjectionScale = 0f;
    [Min(0f)] public float ParryDamage = 70f;

    private void OnValidate() {
        if (StrongBlastRadius > WeakBlastRadius) {
            WeakBlastRadius = StrongBlastRadius;
        }
    }
}
