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
    public bool CapChargeIncreaseByLevel = false;
}
