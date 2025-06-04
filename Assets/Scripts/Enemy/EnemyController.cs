using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    #region AI Variables

    private NavMeshAgent agent;
    private EnemyAIStateMachine stateMachine;

    private GameObject target;
    private Vector3 targetPosition => target.transform.position;
    private void SetTarget() => target = Globals.PLAYER;
    private Vector3 desiredDestination = Vector3.zero;                  // only use when isTargetReachable is true

    // flags for AI
    private bool isTargetInAttackRange = false;
    private bool isTargetInView = false;
    private bool isInComfortableRange = false;
    private bool isTargetReachable = false;

    private List<StateMachine.Transition<EnemyAIState>> transitions;

    #endregion AI Variables

    [Header("Ranges")]
    [Tooltip("The minimum range from the target this agent will attempt to get into")]
    [SerializeField, Min(0f)] private float minComfortableRange = 10f;
    [Tooltip("The minimum range from the target this agent will attempt to get into")]
    [SerializeField, Min(0f)] private float maxComfortableRange = 20f;
    [SerializeField, Min(0f)] private float attackRange = 40f;
    private float attackRangeSquared = 1600f;               // for optimised attack range checks

    private Rigidbody rb;

    private void Awake() {
        transitions = new() {
            new(EnemyAIState.Idle, EnemyAIState.Attack, IdleToAttackCheck, IdleToAttackCallback),
            new(EnemyAIState.Idle, EnemyAIState.Pursue, IdleToPursueCheck, IdleToPursueCallback),
            new(EnemyAIState.Pursue, EnemyAIState.Attack, PursueToAttackCheck, PursueToAttackCallback),
            new(EnemyAIState.Attack, EnemyAIState.Pursue, AttackToPursueCheck, AttackToPursueCallback)
        };
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
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

    #region AI Agent Methods

    private void UpdateAIFlags() {
        if (target == null) {
            isTargetInAttackRange = false;
            isTargetInView = false;
            isInComfortableRange = false;
            isTargetReachable = false;

            return;
        }

        isTargetInAttackRange = TargetInAttackRange();
        isTargetInView = TargetInView();
        isInComfortableRange = TargetInComfortableRange();
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

            default:
                break;
        }
    }
    private void Pursue() {
        if (agent.isStopped) {
            return;
        }

        agent.SetDestination(GetPursueDestination());
    }

    private Vector3 GetPursueDestination() {
        if (target == null)
            return rb.position;

        if (isTargetReachable)
            return desiredDestination;

        return rb.position;
    }

    private void Attack() {
        Debug.Log("Attack");
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

    private bool TargetInComfortableRange() {
        float distanceToTarget = Vector3.Distance(targetPosition, rb.position);

        return distanceToTarget <= maxComfortableRange && distanceToTarget >= minComfortableRange;
    }

    private bool TargetReachable() {
        float desiredDistance = Mathf.Clamp(Vector3.Distance(rb.position, targetPosition), minComfortableRange, maxComfortableRange);
        Vector3 directionToTarget = (targetPosition - rb.position).normalized;
        Vector3 desiredPosition = targetPosition - directionToTarget * desiredDistance;

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

    #endregion State Machine Predicates

    #region State Machine Callbacks

    private void IdleToPursueCallback() {
        agent.isStopped = false;
        Debug.Log("Idle -> Pursue");
    }

    private void IdleToAttackCallback() {
        Debug.Log("Idle -> Attack");
    }

    private void PursueToAttackCallback() {
        agent.isStopped = true;
        agent.ResetPath();
        Debug.Log("Pursue -> Attack");
    }

    private void AttackToPursueCallback() {
        agent.isStopped = false;
        Debug.Log("Attack -> Pursue");
    }

    #endregion State Machine Callbacks

    #endregion State Machine Transitions
}
