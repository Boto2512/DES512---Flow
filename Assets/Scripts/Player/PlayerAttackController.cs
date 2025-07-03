using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerAttackController : MonoBehaviour, IDamageable {
    [SerializeField] private PlayerDamageConfig Config;

    private float health;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Transform orientation;

    private Rigidbody rb;
    private IHasSpeedThresholds thresholdHolder;
    private System.Collections.Generic.List<SpeedStageThreshold> damageThresholds => thresholdHolder.Thresholds;

    private void Start() {
        rb = GetComponent<Rigidbody>();

        health = Config.StartingHealth;

        healthBar.maxValue = Config.MaxHealth;
        healthBar.value = health;
    }

    private void Update() {

    }

    #region Attack

    public void Attack() {
        float damage = CalculateDamage();

        // TODO: change it so attack range scales with speed too

        IDamageable[] damageables = GetEnemiesInAttackBox();
        foreach (var damageable in damageables) {
            damageable.TakeDamage(damage);
        }
    }

    private float CalculateDamage() {
        if (damageThresholds.Count < 1) {
            return Config.Attack;
        }

        for (int i = 1; i < damageThresholds.Count; i++) {
            if (rb.linearVelocity.magnitude < damageThresholds[i].SpeedThreshold) {
                return Config.Attack * damageThresholds[i - 1].DamageMultiplier;
            }
        }

        return Config.Attack * damageThresholds.Last().DamageMultiplier;
    }

    private IDamageable[] GetEnemiesInAttackBox() {
        // TODO: change this to a rotation sweep capsule cast

        Vector3 halfAttackForward = Config.AttackRange * orientation.forward / 2f;
        return Physics.OverlapBox(this.transform.position + halfAttackForward, new Vector3(Config.AttackRange, Config.AttackRange, Config.AttackRange) / 2f, orientation.transform.rotation, Config.AttackMask, QueryTriggerInteraction.Collide)
            .Where(collider => Utility.DoesMaskContainLayer(Config.AttackMask, collider.gameObject.layer)
                && collider.attachedRigidbody != null
                && collider.attachedRigidbody.GetComponent<IDamageable>() != null)
            .Select(collider => collider.attachedRigidbody.GetComponent<IDamageable>())
            .ToArray();
    }

    #endregion Attack

    #region IDamageable

    public float GetHealth() {
        return health;
    }

    public void TakeDamage(float amount) {
        health -= amount;
        healthBar.value = health;
        Debug.Log($"Damage Amount: {amount}, Current Health: {health}");
    }

    public void Kill() {
        throw new System.NotImplementedException();
    }

    public void Heal(float amount) {
        health = Mathf.Clamp(health + amount, 0f, Config.MaxHealth);
    }

    #endregion IDamageable

    #region Input

    // in case input is needed here directly
    public void AttackInput(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            Attack();
        }
    }

    #endregion Input
}
