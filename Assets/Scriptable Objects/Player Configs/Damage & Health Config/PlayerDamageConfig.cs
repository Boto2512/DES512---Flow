using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDamageConfig", menuName = "Scriptable Objects/Player Config/PlayerDamageConfig")]
public class PlayerDamageConfig : ScriptableObject {
    [Header("Attack")]
    [Min(0f)] public float Attack = 10f;
    [Min(0f)] public float AttackRange = 5f;
    [Min(0f)] public float AttackCooldown = 0.5f;
    public LayerMask AttackMask;
    public float KnockbackDealt = 8f;
    public float KnockbackHeight = 2f;

    [Header("Hurtbox Swing")]
    public float StartAngle = 90f;
    public float EndAngle = -90f;
    [Min(0f)] public float SwingTime = 0.1f;
    [Tooltip("Hurtbox.Height = HurtboxLengthConstant + IMomentumModifiable.GetMomentum().magnitude * HurtboxLengthMomentumMultiplier (average speed is ~14 as of writing)")]
    [Min(0f)] public float HurtboxLengthConstant = 3f;
    [Tooltip("Hurtbox.Height = HurtboxLengthConstant + IMomentumModifiable.GetMomentum().magnitude * HurtboxLengthMomentumMultiplier (average speed is ~14 as of writing)")]
    [Min(0f)] public float HurtboxLengthMomentumMultiplier = 0.2f;

    [Header("Parry")]
    [Min(0f)] public float ParryCheckDistance = 1f;
    [Min(0f)] public float ParryMaxAngle = 30f;
    [Min(0f)] public float ParriedProjectileSpeedMultiplier = 2f;
    [Min(0f)] public float ParriedProjectileMinimumSpeed = 10f;
    [Min(0f)] public float ParriedProjectileDefaultDamage = 10f;

    [Header("Hitstop")]
    public bool UseHitStop = true;
    [Min(0f)] public float HitStopTimeScale = 0.02f;
    [Min(0f)] public float HitStopDuration = 0.1f;

    [Header("Health")]
    [Min(0f)] public float MaxHealth = 100f;
    [Min(0f)] public float StartingHealth = 100f;
}
