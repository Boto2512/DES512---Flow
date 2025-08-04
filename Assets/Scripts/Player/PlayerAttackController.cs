using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.VFX;
using static UnityEngine.Rendering.STP;

public class PlayerAttackController : MonoBehaviour, IDamageable
{
    [SerializeField] public PlayerDamageConfig Config;

    [Header("GameOver")]
    [SerializeField] private GameObject firstSelectedObject;
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private bool dead;

    [Header("Health")]
    [SerializeField] private Slider healthBar;
    private float health;
    [SerializeField] private Material vignetteMAT;
    [SerializeField] private float vignettePower = 10;
    [SerializeField] private float vignetteDelay;
    float vignetteTimer;

    [Header("Orientation")]
    [SerializeField] private Transform orientation;

    [Header("Animations")]
    [SerializeField] private Animator attackAnimator;
    [SerializeField] private Animator cameraAnimator;
    private bool attacking = false;
    private bool attackReadied = false;

    [Header("Hurtbox")]
    [SerializeField] private PlayerHurtbox hurtbox;

    [Header("Crosshair")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color defaultCrosshairColor = Color.white;
    [SerializeField] private Color enemyCrosshairColor = Color.red;
    [SerializeField] private float aimCheckDistance = 100f;
    [SerializeField] private LayerMask enemyLayerMask;


    [Header("Prefabs")]
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject parryProjectile;
    [SerializeField] private Transform parryProjectilePosition;
    private bool parried = false;
    [SerializeField] private VisualEffect slashVFX;



    private Rigidbody rb;
    private IHasSpeedThresholds thresholdHolder;

    // input
    private bool attackPressed = false;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        thresholdHolder = this.GetComponent<IHasSpeedThresholds>();

        health = Config.StartingHealth;

        healthBar.maxValue = Config.MaxHealth;
        healthBar.value = health;

        hurtbox.gameObject.SetActive(false);
        gameOverCanvas.SetActive(false);

        vignetteMAT = GetComponent<FullScreenPassRendererFeature>().passMaterial;

        if (!parryProjectile.TryGetComponent<ParryProjectile>(out _))
            Debug.LogError("Parry Projectile prefab in PlayerAttackController doesn't have the ParryProjectile script component");
    }

    private void Update()
    {
        if (attacking && !parried)
            Parry();

        VignettePower();

        CheckIfAimingAtEnemy();

    }

    #region Attack

    public void ReadyAttack()
    {
        attackPressed = true;
        if (attacking)
            return;

        attackReadied = true;
        attackAnimator.SetBool("isHoldingHammer", true);
    }

    public void Attack()
    {
        attackPressed = false;
        if (attacking || !attackReadied)
            return;

        attackAnimator.SetBool("isHoldingHammer", false);
        //attackAnimator.SetTrigger("hasAttacked");
        cameraAnimator.SetTrigger("hasAttacked");
        AudioManager.instance?.Play("PlayerAttackInTheAir");

        slashVFX.transform.position = hurtbox.transform.position;
        slashVFX.transform.rotation = hurtbox.transform.rotation;
        slashVFX.Play();

        attacking = true;
        attackReadied = false;
        hurtbox.gameObject.SetActive(true);

        this.InvokeOverwrite("attack animation playing", () => { attacking = false; parried = false; CheckHoldingAttack(); }, Config.AttackCooldown);
    }

    public void DamageableHit(IDamageable damageable, Collider collider)
    {
        float damage = CalculateDamage();
        damageable.TakeDamage(damage, this.gameObject);

        if (collider.attachedRigidbody != null && collider.attachedRigidbody.TryGetComponent<IMomentumModifiable>(out var imm))
        {
            Vector3 knockback = orientation.forward.normalized * Config.KnockbackDealt;
            knockback.y = Config.KnockbackHeight;
            imm.AddMomentum(knockback);
        }

        if (Config.UseHitStop)
        {
            HitStop.Slow(Config.HitStopTimeScale, Config.HitStopDuration).Forget();
        }
        //HitStop.Stop(0.33f).Forget();

        Instantiate(hitVFX, collider.ClosestPoint(hurtbox.transform.position), Quaternion.identity);
        AudioManager.instance?.Play("PlayerAttack");
    }

    private void Parry()
    {
        Collider[] projectiles = new Collider[5];
        int projectilesLength = Physics.OverlapSphereNonAlloc(orientation.position, Config.ParryCheckDistance, projectiles, Globals.PROJECTILE_MASK, QueryTriggerInteraction.Collide);
        if (projectilesLength == 0)
        {
            return;
        }

        for (int i = 0; i < projectilesLength; ++i)
        {
            float angle = Vector3.Angle(orientation.forward, projectiles[i].gameObject.transform.position - orientation.position);
            if (angle <= Config.ParryMaxAngle)
            {
                parried = true;

                float projectileSpeed = Mathf.Max(Config.ParriedProjectileMinimumSpeed, rb.linearVelocity.magnitude * Config.ParriedProjectileSpeedMultiplier);
                Vector3 momentum = orientation.forward.normalized * projectileSpeed;

                if (Config.UseParryHitStop)
                    HitStop.Slow(Config.HitStopTimeScale, Config.HitStopDuration).Forget();

                if (projectiles[i].attachedRigidbody != null && projectiles[i].attachedRigidbody.gameObject.TryGetComponent<BounceBomb>(out var bb))
                {
                    projectiles[i].attachedRigidbody.linearVelocity = momentum;
                    projectiles[i].attachedRigidbody.useGravity = false;
                    projectiles[i].attachedRigidbody.linearDamping = 0f;
                    projectiles[i].attachedRigidbody.rotation = Quaternion.Euler(90f, 0f, 0f);
                    projectiles[i].attachedRigidbody.angularVelocity = new(0f, 50f, 0f);
                    bb.SetParried();
                }
                else {
                    ParryProjectile parriedProjectileScript = Instantiate(parryProjectile, parryProjectilePosition.position, Quaternion.FromToRotation(Vector3.zero, orientation.forward)).GetComponent<ParryProjectile>();
                    parriedProjectileScript.Momentum = momentum;
                    parriedProjectileScript.Damage = Config.ParriedProjectileDefaultDamage * thresholdHolder.CurrentThreshold.DamageMultiplier;

                    Destroy(projectiles[i].attachedRigidbody.gameObject);
                }

                break;
            }
        }
    }

    private float CalculateDamage()
    {
        if (thresholdHolder.CurrentThreshold == null)
            return Config.Attack;

        return Config.Attack * thresholdHolder.CurrentThreshold.DamageMultiplier;
    }

    private void CheckHoldingAttack()
    {
        if (attackPressed)
        {
            ReadyAttack();
        }
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

    public float GetHealth()
    {
        return health;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        healthBar.value = health;
        TutorialEvents.OnEnemyKilled?.Invoke();


        vignettePower = 3f;
        vignetteMAT.SetFloat("_VignettePower", vignettePower);
        vignetteTimer = 0;

        if (health <= 0 && !gameOverCanvas.activeSelf)
        {
            Kill();
        }
    }

    private void VignettePower()
    {
        if (vignettePower != 6)
        {
            vignetteTimer += Time.deltaTime * .01f;
            vignettePower = Mathf.Lerp(vignettePower, 7, vignetteTimer);
            vignetteMAT.SetFloat("_VignettePower", vignettePower);
        }
    }
    public void Kill()
    {
        StartCoroutine(Death());

    }

    private IEnumerator Death()
    {
        yield return new WaitForSeconds(.5f);
        gameOverCanvas.SetActive(true);
        dead = true;
        EventSystem.current.SetSelectedGameObject(firstSelectedObject);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
    }

    public void Heal(float amount)
    {
        health = Mathf.Clamp(health + amount, 0f, Config.MaxHealth);
        healthBar.value = health;
    }

    #endregion IDamageable

    #region Input

    // in case input is needed here directly
    public void AttackInput(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            Attack();
        }
    }

    #endregion Input

    #region Crosshair

    private void CheckIfAimingAtEnemy()
    {
        Ray ray = new Ray(orientation.position, orientation.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, aimCheckDistance, enemyLayerMask | Globals.OBSTACLE_MASK, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                crosshairImage.color = enemyCrosshairColor;
                return;
            }
        }

        crosshairImage.color = defaultCrosshairColor;
    }

    #endregion Crosshair
}
