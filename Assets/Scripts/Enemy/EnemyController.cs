using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour, IDamageable, IMomentumModifiable {
    #region Damage Variables
    [Header("Health")]
    [SerializeField, Min(0f)] float health = 100;
    [SerializeField] Slider healthBar;
    [SerializeField] GameObject healthDrop;

    [Header("Drops")]
    [Min(0f)] public float BombRegenAmount { get; private set; } = 0.5f;
    [Min(0f)] private float droppedHealth;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject oilHitVFX;
    [SerializeField] private GameObject oilDieVFX;

    [Header("Debug Events")]
    [SerializeField] private UnityEvent takeDamage = new();
    #endregion Damage Variables

    #region AI Variables

    private NavMeshAgent agent;
    private EnemyAIStateMachine stateMachine;

    private ITargetable target;
    private Vector3 targetPosition => target.Target.position;
    private void SetTarget() => target = Globals.PLAYER_TARGET;
    private Vector3 desiredDestination = Vector3.zero;                  // only use when isTargetReachable is true

    // flags for AI
    private bool isTargetInAttackRange = false;
    private bool isTargetInView = false;
    private bool isInComfortableRange = false;
    private bool isTargetTooClose = false;
    private bool isTargetReachable = false;
    private bool isTargetTooFar => !(isInComfortableRange || isTargetTooClose);
    private bool bombBounced = false;

    [Header("Attack")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform attackTransform;
    [SerializeField, Min(0f)] private float attackCooldown = 1f;
    private bool isAttacking = false;
    private Vector3 firingPosition => attackTransform.position;

    private List<StateMachine.Transition<EnemyAIState>> transitions;

    #endregion AI Variables

    [Header("Ranges")]
    [SerializeField, Min(0f)] private float attackRange = 40f;
    [Tooltip("The maximum range from the target this agent will attempt to get into")]
    [SerializeField, Min(0f)] private float maxComfortableRange = 20f;
    [Tooltip("The minimum range from the target this agent will attempt to get into")]
    [SerializeField, Min(0f)] private float minComfortableRange = 10f;
    private float attackRangeSquared = 1600f;                           // for optimised attack range checks

    private Rigidbody rb;

    [Header("Ground Checking")]
    [SerializeField] private Transform groundCheckPosition;
    [SerializeField, Min(0f)] private float groundCheckRange = 0.25f;
    private bool isGrounded = true;
    private bool groundCheckEnabled = true;

    [Header("Momentum")]
    [SerializeField] private Transform momentumPosition;

    private void Awake() {
        transitions = new() {
            new(EnemyAIState.Idle, EnemyAIState.Attack, IdleToAttackCheck, IdleToAttackCallback),
            new(EnemyAIState.Idle, EnemyAIState.Pursue, IdleToPursueCheck, IdleToPursueCallback),
            new(EnemyAIState.Pursue, EnemyAIState.Idle, PursueToIdleCheck, PursueToIdleCallback),
            new(EnemyAIState.Pursue, EnemyAIState.Attack, PursueToAttackCheck, PursueToAttackCallback),
            new(EnemyAIState.Pursue, EnemyAIState.Reposition, PursueToRepositionCheck, PursueToRepositionCallback),
            new(EnemyAIState.Attack, EnemyAIState.Idle, AttackToIdleCheck, AttackToIdleCallback),
            new(EnemyAIState.Attack, EnemyAIState.Pursue, AttackToPursueCheck, AttackToPursueCallback),
            new(EnemyAIState.Attack, EnemyAIState.Reposition, AttackToRepositionCheck, AttackToRepositionCallback),
            new(EnemyAIState.Reposition, EnemyAIState.Attack, RepositionToAttackCheck, RepositionToAttackCallback),
            new(EnemyAIState.Reposition, EnemyAIState.Pursue, RepositionToPursueCheck, RepositionToPursueCallback)
        };
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        healthBar.maxValue = health;
        healthBar.value = healthBar.maxValue;

        SetTarget();
        agent = this.GetComponent<NavMeshAgent>();
        rb = this.GetComponent<Rigidbody>();

        stateMachine = new EnemyAIStateMachine(transitions);
        Globals.EVENT_PLAYER_MODIFIED.AddListener(SetTarget);
    }

    // Update is called once per frame
    void Update() {
        UpdateAIFlags();
        stateMachine.Update();
        DecideAction();
    }

    private void OnValidate() {
        // makes sure maxComfortDistance is always larger or equal to minComfortDistance
        if (maxComfortableRange < minComfortableRange) {
            minComfortableRange = maxComfortableRange;
        }

        attackRangeSquared = attackRange * attackRange;
    }

    private void OnCollisionEnter(Collision collision) {
        GameObject go = collision.gameObject;
        if (go == null)
            return;

        if (!groundCheckEnabled && Utility.DoesMaskContainLayer(Globals.GROUND_MASK, go.layer)) {
            groundCheckEnabled = true;
        }
    }

    #region Damage Interface

    public float GetHealth() {
        return health;
    }

    public void TakeDamage(float value) {
        //Debug.Log($"Taken {value} damage");
        health -= value;
        healthBar.value = health;

        if (health <= 0) {
            Instantiate(oilDieVFX, attackTransform.position, Quaternion.Euler(Vector3.up));
            Kill();
            TutorialEvents.OnEnemyKilled?.Invoke();
            TutorialEvents.enemyKilledVeryFast?.Invoke();
        }
        else {
            Instantiate(oilHitVFX, attackTransform.position, Quaternion.Euler(Vector3.up));
        }
    }

    public void Kill() {
        //Debug.Log("Enemy Oneshotted - due to speed");
        healthBar.value = 0;
        Instantiate(healthDrop, transform.position, transform.rotation);
        Destroy(this.gameObject);
        
    }

    public void Heal(float value) {
        if (health + value <= healthBar.maxValue) {
            health += value;
        }
        else {
            health = healthBar.maxValue;
        }
    }

    #endregion Damage Interface

    #region AI Agent Methods

    private void UpdateAIFlags() {
        if (target == null) {
            isTargetInAttackRange = false;
            isTargetInView = false;
            isInComfortableRange = false;
            isTargetTooClose = false;
            isTargetReachable = false;

            return;
        }

        isTargetInAttackRange = TargetInAttackRange();
        isTargetInView = TargetInView();
        (isInComfortableRange, isTargetTooClose) = TargetRangeCheck();
        isTargetReachable = TargetReachable();
        isGrounded = GroundCheck();
    }

    private void DecideAction() {
        switch (stateMachine.CurrentState) {
            case EnemyAIState.Idle:
                Idle();
                break;

            case EnemyAIState.Pursue:
                Pursue();
                break;

            case EnemyAIState.Attack:
                Attack();
                break;

            case EnemyAIState.Reposition:
                Reposition();
                break;

            case EnemyAIState.FanOut:
                FanOut();
                break;

            default:
                break;
        }
    }

    private void Idle() {
        if (isGrounded && bombBounced) {
            bombBounced = false;
        }
    }

    private void Pursue() {
        SetAgentDestination(GetPursueDestination());
    }

    private void Attack() {
        if (isAttacking)
            return;

        isAttacking = true;
        Instantiate(projectile, attackTransform.position, attackTransform.rotation);
        this.InvokeExclusive("attackCooldown", () => isAttacking = false, attackCooldown);
    }

    private void Reposition() {
        float halfComfortableRange = (maxComfortableRange + minComfortableRange) / 2f;
        Vector3 toComfortableRange = (rb.position - targetPosition).normalized * halfComfortableRange;

        if (NavMesh.SamplePosition(targetPosition + toComfortableRange, out NavMeshHit hit, halfComfortableRange, NavMesh.AllAreas)) {
            SetAgentDestination(hit.position);
        }
    }

    private void FanOut() {

    }

    private Vector3 GetPursueDestination() {
        if (target == null)
            return rb.position;

        if (isTargetReachable)
            return desiredDestination;

        return rb.position;
    }

    private bool TargetInAttackRange() {
        return (targetPosition - rb.position).sqrMagnitude <= attackRangeSquared;
    }

    private bool TargetInView() {
        Vector3 enemyToTarget = targetPosition - firingPosition;

        // 0.5f (half player width) for example
        return !Physics.SphereCast(firingPosition, 0.1f, enemyToTarget.normalized, out _, enemyToTarget.magnitude, Globals.OBSTACLE_MASK);
    }

    private (bool inComfortableRange, bool tooClose) TargetRangeCheck() {
        float distanceToTarget = Vector3.Distance(targetPosition, rb.position);

        return (distanceToTarget <= maxComfortableRange && distanceToTarget >= minComfortableRange, distanceToTarget < minComfortableRange);
    }

    private bool TargetReachable() {
        //float desiredDistance = Mathf.Clamp(Vector3.Distance(rb.position, targetPosition), minComfortableRange, maxComfortableRange);
        //Vector3 directionToTarget = (targetPosition - rb.position).normalized;
        //Vector3 desiredPosition = targetPosition - directionToTarget * desiredDistance;

        bool result = NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, attackRange, NavMesh.AllAreas);
        desiredDestination = hit.position;

        return result;
    }

    #endregion AI Agent Methods

    #region State Machine Transitions

    #region State Machine Predicates

    private bool IdleToAttackCheck() {
        return isTargetInAttackRange && isTargetInView && isGrounded && !bombBounced;
    }

    private bool IdleToPursueCheck() {
        return isTargetReachable && isGrounded && !bombBounced;
    }

    private bool AttackToIdleCheck() {
        return !isGrounded || bombBounced;
    }

    private bool PursueToIdleCheck() {
        return !isGrounded || bombBounced;
    }

    private bool PursueToAttackCheck() {
        return isTargetInView && (isInComfortableRange || (!isTargetReachable && isTargetInAttackRange));
    }

    private bool PursueToRepositionCheck() {
        return isTargetTooClose;
    }

    private bool AttackToPursueCheck() {
        return !isTargetInAttackRange || !isTargetInView || bombBounced;
    }

    private bool AttackToRepositionCheck() {
        return isTargetInView && isTargetTooClose;
    }

    private bool RepositionToAttackCheck() {
        return isTargetInView && isInComfortableRange;
    }

    private bool RepositionToPursueCheck() {
        return !isTargetInView || isTargetTooFar;
    }

    #endregion State Machine Predicates

    #region State Machine Callbacks

    private void IdleToPursueCallback() {
        agent.enabled = true;
        rb.isKinematic = true;

        //Debug.Log("Idle -> Pursue");
    }

    private void IdleToAttackCallback() {
        agent.enabled = true;
        rb.isKinematic = true;

        //Debug.Log("Idle -> Attack");
    }

    private void AttackToIdleCallback() {
        //Debug.Log("Attack -> Idle");
    }

    private void PursueToIdleCallback() {
        ResetAgentPath();
        //Debug.Log("Pursue -> Idle");
    }

    private void PursueToAttackCallback() {
        ResetAgentPath();
        //Debug.Log("Pursue -> Attack");
    }

    private void PursueToRepositionCallback() {
        ResetAgentPath();
        //Debug.Log("Pursue -> Reposition");
    }

    private void AttackToPursueCallback() {
        //Debug.Log("Attack -> Pursue");
    }

    private void AttackToRepositionCallback() {
        //Debug.Log("Attack -> Reposition");
    }

    private void RepositionToAttackCallback() {
        ResetAgentPath();
        //Debug.Log("Reposition -> Attack");
    }

    private void RepositionToPursueCallback() {
        ResetAgentPath();
        //Debug.Log("Reposition -> Pursue");
    }

    #endregion State Machine Callbacks

    #endregion State Machine Transitions

    #region Momentum Modifiable Interface

    public Vector3 GetPosition() {
        return momentumPosition.position;
    }

    public Vector3 GetMomentum() {
        return rb.linearVelocity;
    }

    public void SetMomentum(Vector3 value) {
        ResetAgentPath();
        agent.enabled = false;
        rb.isKinematic = false;

        isGrounded = false;
        groundCheckEnabled = false;

        rb.linearVelocity = value;
        //this.InvokeOverwrite("bombBounced", () => bombBounced = true, 0.1f);
    }

    public void AddMomentum(Vector3 value, ForceMode additionType = ForceMode.Impulse) {
        SetMomentum(GetMomentum() + value);
    }

    public void BeenBombBounced() {
        bombBounced = true;
    }

    #endregion Momentum Modifiable Interface

    private bool GroundCheck() {
        if (!groundCheckEnabled)
            return isGrounded;

        return Physics.Raycast(groundCheckPosition.position, Vector3.down, groundCheckRange, Globals.OBSTACLE_MASK);
    }

    #region Safe NavMeshAgent Methods

    private bool SetAgentDestination(Vector3 destination) {
        return isGrounded && agent.enabled && agent.SetDestination(destination);
    }

    private void ResetAgentPath() {
        if (isGrounded && agent.enabled) {
            agent.ResetPath();
        }
    }

    #endregion Safe NavMeshAgent Methods

}
