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
    private float attackRangeSquared;       // for optimised attack range checks

    private Rigidbody rb;

    private void Awake() {
        transitions = new() {
            new(EnemyAIState.Idle, EnemyAIState.Pursue, IdleToPursueCheck, null),
            new(EnemyAIState.Idle, EnemyAIState.Attack, IdleToAttackCheck, null),
            new(EnemyAIState.Pursue, EnemyAIState.Attack, PursueToAttackCheck, null),
            new(EnemyAIState.Attack, EnemyAIState.Pursue, AttackToPursueCheck, null)
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
    }

    private bool TargetInView() {
        Vector3 enemyToTarget = targetPosition - rb.position;

        return Physics.Raycast(rb.position, enemyToTarget.normalized, out _, enemyToTarget.magnitude, Globals.OBSTACLE_MASK, QueryTriggerInteraction.Collide);
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

    private bool IdleToAttackCheck() {
        return isTargetInAttackRange && isTargetInView;
    }

    private bool IdleToPursueCheck() {
        return isTargetReachable;
    }

    private bool PursueToAttackCheck() {
        return isTargetInView && isInComfortableRange;
    }

    private bool AttackToPursueCheck() {
        return !isTargetInAttackRange || !isTargetInView;
    }

    #endregion State Machine Transitions
}
