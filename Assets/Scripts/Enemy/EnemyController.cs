using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour, IDamageable, IMomentumModifiable {
    #region Damage Variables
    [Header("Health")]
    [SerializeField, Min(0f)] float health = 100;
    [SerializeField] Slider healthBar;

    [Header("Debug Events")]
    [SerializeField] private UnityEvent takeDamage = new();
    #endregion Damage Variables

    #region AI Variables

    private NavMeshAgent agent;
    private EnemyAIStateMachine stateMachine;

    private ITargetable target;
    private Vector3 targetPosition => target.Target.position;
    private void SetTarget() => target = Globals.PLAYER;
    private Vector3 desiredDestination = Vector3.zero;                  // only use when isTargetReachable is true

    // flags for AI
    private bool isTargetInAttackRange = false;
    private bool isTargetInView = false;
    private bool isInComfortableRange = false;
    private bool isTargetTooClose = false;
    private bool isTargetReachable = false;
    private bool isTargetTooFar => !(isInComfortableRange || isTargetTooClose);

    private bool inPursuit = false;

    private List<StateMachine.Transition<EnemyAIState>> transitions;

    #endregion AI Variables

    [Header("Ranges")]
    [Tooltip("The minimum range from the target this agent will attempt to get into")]
    [SerializeField, Min(0f)] private float minComfortableRange = 10f;
    [Tooltip("The minimum range from the target this agent will attempt to get into")]
    [SerializeField, Min(0f)] private float maxComfortableRange = 20f;
    [SerializeField, Min(0f)] private float attackRange = 40f;
    private float attackRangeSquared = 1600f;                           // for optimised attack range checks

    private Rigidbody rb;

    [Header("Momentum")]
    [SerializeField] private Transform momentumPosition;

    private void Awake() {
        transitions = new() {
            new(EnemyAIState.Idle, EnemyAIState.Attack, IdleToAttackCheck, IdleToAttackCallback),
            new(EnemyAIState.Idle, EnemyAIState.Pursue, IdleToPursueCheck, IdleToPursueCallback),
            new(EnemyAIState.Pursue, EnemyAIState.Attack, PursueToAttackCheck, PursueToAttackCallback),
            new(EnemyAIState.Attack, EnemyAIState.Pursue, AttackToPursueCheck, AttackToPursueCallback)/*,
            new(EnemyAIState.Attack, EnemyAIState.Reposition, AttackToRepositionCheck, AttackToRepositionCallback),
            new(EnemyAIState.Reposition, EnemyAIState.Attack, RepositionToAttackCheck, RepositionToAttackCallback),
            new(EnemyAIState.Reposition, EnemyAIState.Pursue, RepositionToPursueCheck, RepositionToPursueCallback)*/
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

    #region Damage Interface

    public float GetHealth() {
        return health;
    }

    public void TakeDamage(float value) {
        Debug.Log($"Taken {value} damage");
        health -= value;
        healthBar.value = health;

        if (health <= 0) {
            Kill();
        }
    }

    public void Kill() {
        Debug.Log("Enemy Oneshotted - due to speed");
        healthBar.value = 0;
        Destroy(this.gameObject);
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
    }

    private void DecideAction() {
        switch (stateMachine.CurrentState) {
            case EnemyAIState.Idle:
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

            default:
                break;
        }
    }

    private void Attack() {
        Debug.Log("Attack");
    }
    
    private void Pursue() {
        agent.SetDestination(GetPursueDestination());
    }

    private void Reposition() {
        Vector3 toComfortableRange = (rb.position - targetPosition).normalized * (minComfortableRange + maxComfortableRange) / 2f;
        if (NavMesh.SamplePosition(targetPosition + toComfortableRange, out NavMeshHit hit, maxComfortableRange - minComfortableRange, NavMesh.AllAreas)) {
            agent.SetDestination(hit.position);
        }
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
        //return Vector3.Distance(targetPosition, rb.position) <= attackRange;
    }

    private bool TargetInView() {
        Vector3 enemyToTarget = targetPosition - rb.position;

        // 0.5f (half player width) for example
        return !Physics.SphereCast(rb.position, 0.5f, enemyToTarget.normalized, out RaycastHit hitInfo, enemyToTarget.magnitude, Globals.OBSTACLE_MASK);
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
        return isTargetInAttackRange && isTargetInView;
    }

    private bool IdleToPursueCheck() {
        return isTargetReachable;
    }

    private bool PursueToAttackCheck() {
        return isTargetInView && (isInComfortableRange || (!isTargetReachable && isTargetInAttackRange));
    }

    private bool AttackToPursueCheck() {
        return !isTargetInAttackRange || !isTargetInView;
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
        inPursuit = true;
        Debug.Log("Idle -> Pursue");
    }

    private void IdleToAttackCallback() {
        Debug.Log("Idle -> Attack");
    }

    private void PursueToAttackCallback() {
        inPursuit = false;
        agent.ResetPath();
        Debug.Log("Pursue -> Attack");
    }

    private void AttackToPursueCallback() {
        inPursuit = true;
        Debug.Log("Attack -> Pursue");
    }

    private void AttackToRepositionCallback() {
        Debug.Log("Attack -> Reposition");
    }

    private void RepositionToAttackCallback() {
        Debug.Log("Reposition -> Attack");
    }

    private void RepositionToPursueCallback() {
        inPursuit = true;
        Debug.Log("Reposition -> Pursue");
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
        rb.AddForce(value, ForceMode.Impulse);
    }

    #endregion Momentum Modifiable Interface
}
