using UnityEngine;

[CreateAssetMenu(fileName = "BBThrowConfig", menuName = "Scriptable Objects/Player Config/BBThrowConfig")]
public class BBThrowConfig : ScriptableObject {

    [Header("Bounce Bomb Prefab")]
    public GameObject BounceBomb;

    [Header("Throw Logic")]
    [Min(0f)] public float ThrowPower = 10f;
    [Min(0f)] public float FuseTime = 0.15f;

    [Header("Charges")]
    [Min(0)] public int MaxCharges = 3;
    [Min(0f)] public float ChargeRegenTime = 2f;
    [Tooltip("Caps the charge to the current level during addition. E.g., when true: 1.8 charges + 0.5 = 2 charges, when false: 1.8 charges + 0.5 = 2.3 charges")]
    public bool CapChargeIncreaseByLevel = false;
}
