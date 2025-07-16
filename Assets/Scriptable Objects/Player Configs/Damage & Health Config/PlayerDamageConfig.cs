using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDamageConfig", menuName = "Scriptable Objects/Player Config/PlayerDamageConfig")]
public class PlayerDamageConfig : ScriptableObject {
    [Header("Attack")]
    [Min(0f)] public float Attack = 10f;
    [Min(0f)] public float AttackRange = 5f;
    [Min(0f)] public float AttackCooldown = 0.5f;
    public LayerMask AttackMask;

    [Header("Health")]
    [Min(0f)] public float MaxHealth = 100f;
    [Min(0f)] public float StartingHealth = 100f;
}
