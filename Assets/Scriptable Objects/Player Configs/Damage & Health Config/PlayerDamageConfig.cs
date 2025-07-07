using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDamageConfig", menuName = "Scriptable Objects/Player Config/PlayerDamageConfig")]
public class PlayerDamageConfig : ScriptableObject {
    [Header("Attack")]
    [Min(0f)] public float Attack = 10f;
    [Min(0f)] public float AttackRange = 5f;
    public LayerMask AttackMask;
    //public List<SpeedStageThreshold> DamageThresholds;

    [Header("Health")]
    [Min(0f)] public float MaxHealth = 100f;
    [Min(0f)] public float StartingHealth = 100f;
}
