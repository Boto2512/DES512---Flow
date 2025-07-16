using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerAttackController : MonoBehaviour, IDamageable {
    [SerializeField] public PlayerDamageConfig Config;

    [Header("Health")]
    [SerializeField] private Slider healthBar;
    private float health;

    [Header("Orientation")]
    [SerializeField] private Transform orientation;

    [Header("Animations")]
    [SerializeField] private Animator attackAnimator;
    [SerializeField] private Animator cameraAnimator;
    private bool attacking = false;

    [Header("Hurtbox")]
    [SerializeField] private PlayerHurtbox hurtbox;

    [Header("Hit VFX Prefab")]
    [SerializeField] private GameObject hitVFX;

    private Rigidbody rb;
    private IHasSpeedThresholds thresholdHolder;

    private void Start() {
        rb = this.GetComponent<Rigidbody>();
        thresholdHolder = this.GetComponent<IHasSpeedThresholds>();

        health = Config.StartingHealth;

        healthBar.maxValue = Config.MaxHealth;
        healthBar.value = health;

        hurtbox.gameObject.SetActive(false);

        //cameraImpulseSource.ImpulseDefinition.
    }

    private void Update() {

    }

    #region Attack

    public void Attack() {
        if (attacking)
            return;

        attackAnimator.SetTrigger("hasAttacked");
        cameraAnimator.SetTrigger("hasAttacked");

        // TODO: change it so attack range scales with speed too

        //float damage = CalculateDamage();
        //IDamageable[] damageables = GetEnemiesInAttackBox();
        //foreach (var damageable in damageables) {
        //    damageable.TakeDamage(damage);
        //    HitStop.Slow(0.1f, 0.1f).Forget();
        //}

        attacking = true;
        hurtbox.gameObject.SetActive(true);
        this.InvokeOverwrite("attack animation playing", () => attacking = false, Config.AttackCooldown);
    }

    public void DamageableHit(IDamageable damageable, Collider collider) {
        float damage = CalculateDamage();
        damageable.TakeDamage(damage);

        if (collider.attachedRigidbody != null && collider.attachedRigidbody.TryGetComponent<IMomentumModifiable>(out var imm)) {
            Vector3 knockback = orientation.forward.normalized * Config.KnockbackDealt;
            knockback.y = Config.KnockbackHeight;
            imm.AddMomentum(knockback);
        }

        if (Config.UseHitStop) {
            HitStop.Slow(Config.HitStopTimeScale, Config.HitStopDuration).Forget();
        }
        //HitStop.Stop(0.33f).Forget();

        Instantiate(hitVFX, collider.ClosestPoint(hurtbox.transform.position), Quaternion.identity);
    }

    private float CalculateDamage() {
        if (thresholdHolder.CurrentThreshold == null)
            return Config.Attack;

        return Config.Attack * thresholdHolder.CurrentThreshold.DamageMultiplier;
    }

    //private IDamageable[] GetEnemiesInAttackBox() {
    //    // TODO: change this to a rotation sweep capsule cast

    //    Vector3 halfAttackForward = Config.AttackRange * orientation.forward / 2f;
    //    return Physics.OverlapBox(this.transform.position + halfAttackForward,
    //            new Vector3(Config.AttackRange, Config.AttackRange, Config.AttackRange) / 2f, orientation.transform.rotation, Config.AttackMask, QueryTriggerInteraction.Collide)
    //        .Where(collider => Utility.DoesMaskContainLayer(Config.AttackMask, collider.gameObject.layer)
    //            && collider.attachedRigidbody != null
    //            && collider.attachedRigidbody.GetComponent<IDamageable>() != null)
    //        .Select(collider => collider.attachedRigidbody.GetComponent<IDamageable>())
    //        .Where(damageable => damageable != (IDamageable)this)
    //        .ToArray();
    //}

    #endregion Attack

    #region IDamageable

    public float BombRegenAmount { get; } = 0f;

    public float GetHealth() {
        return health;
    }

    public void TakeDamage(float amount) {
        health -= amount;
        healthBar.value = health;
        TutorialEvents.OnEnemyKilled?.Invoke();
        //Debug.Log($"Damage Amount: {amount}, Current Health: {health}");
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
