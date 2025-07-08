using UnityEngine;

[CreateAssetMenu(fileName = "BBThrowConfig", menuName = "Scriptable Objects/Player Config/BBThrowConfig")]
public class BBThrowConfig : ScriptableObject {
    [Min(0f)] public float ThrowPower = 10f;
    [Min(0f)] public float FuseTime = 0.15f;
    public GameObject BounceBomb;
}
