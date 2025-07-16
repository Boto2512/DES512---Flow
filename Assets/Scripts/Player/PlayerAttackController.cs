using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerAttackController : MonoBehaviour, IDamageable {
    [SerializeField] private PlayerDamageConfig Config;

    [Header("GameOver")]
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject firstSelectedObject;
    [SerializeField] private Animator fade;

    [Header("Health")]
    [SerializeField] private Slider healthBar;
    private float health;

    [Header("Orientation")]
    [SerializeField] private Transform orientation;

    [Header("Animations")]
    [SerializeField] private Animator attackAnimator;
    [SerializeField] private Animator cameraAnimator;

    private Rigidbody rb;
    private IHasSpeedThresholds thresholdHolder;

    private void Start() {
        rb = this.GetComponent<Rigidbody>();
        thresholdHolder = this.GetComponent<IHasSpeedThresholds>();

        health = Config.StartingHealth;

        healthBar.maxValue = Config.MaxHealth;
        healthBar.value = health;
        gameOver.SetActive(false);
        //cameraImpulseSource.ImpulseDefinition.
    }

    private void Update() {

    }

    #region Attack

    public void Attack() {
        float damage = CalculateDamage();

        attackAnimator.SetTrigger("hasAttacked");
        cameraAnimator.SetTrigger("hasAttacked");

        // TODO: change it so attack range scales with speed too

        IDamageable[] damageables = GetEnemiesInAttackBox();
        foreach (var damageable in damageables) {
            damageable.TakeDamage(damage);
        }
    }

    private float CalculateDamage() {
        if (thresholdHolder.CurrentThreshold == null)
            return Config.Attack;

        return Config.Attack * thresholdHolder.CurrentThreshold.DamageMultiplier;
    }

    private IDamageable[] GetEnemiesInAttackBox() {
        // TODO: change this to a rotation sweep capsule cast

        Vector3 halfAttackForward = Config.AttackRange * orientation.forward / 2f;
        return Physics.OverlapBox(this.transform.position + halfAttackForward,
                new Vector3(Config.AttackRange, Config.AttackRange, Config.AttackRange) / 2f, orientation.transform.rotation, Config.AttackMask, QueryTriggerInteraction.Collide)
            .Where(collider => Utility.DoesMaskContainLayer(Config.AttackMask, collider.gameObject.layer)
                && collider.attachedRigidbody != null
                && collider.attachedRigidbody.GetComponent<IDamageable>() != null)
            .Select(collider => collider.attachedRigidbody.GetComponent<IDamageable>())
            .Where(damageable => damageable != (IDamageable)this)
            .ToArray();
    }

    #endregion Attack

    #region IDamageable

    public float GetHealth() {
        return health;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        healthBar.value = health;
        TutorialEvents.OnEnemyKilled?.Invoke();
        if(health <= 0 && !gameOver.activeSelf)
        {
            Kill();
        }
    }

    public void Kill()
    {


        Time.timeScale = 0;
        gameOver.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstSelectedObject);


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
