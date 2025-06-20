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

    [Header("Other")]
    [Tooltip("The ratio between horizontal momentum gain and vertical momentum gain (1 is equal gain, 0.5 is 50% of vertical taken off and given to horizontal, 0 is no vertical gain, full horizontal gain)")]
    [Range(0f, 1f)] public float VerticalGainRatio = 0.5f;
    [Tooltip("How far ahead the momentum projects the entity position")]
    [Min(0f)] public float EntityProjectionScale = 0f;

    private void OnValidate() {
        if (StrongBlastRadius > WeakBlastRadius) {
            WeakBlastRadius = StrongBlastRadius;
        }
    }
}
