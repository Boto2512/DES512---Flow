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

    //private static readonly List<StateMachine.Transition<EnemyAIState>> transitions = new() { 
    //    new(EnemyAIState.Idle, EnemyAIState.Pursue, )
    //};

    #endregion AI Variables

    [SerializeField, Min(0f)] private float attackRange = 20f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        agent = this.GetComponent<NavMeshAgent>();
        stateMachine = new EnemyAIStateMachine(new List<StateMachine.Transition<EnemyAIState>>());
        Globals.EVENT_PLAYER_MODIFIED.AddListener(SetTarget);
    }

    // Update is called once per frame
    void Update() {
        
    }

    private bool IsTargetInAttackRange() {
        bool result = Physics.Raycast(this.transform.position, targetPosition - this.transform.position, out RaycastHit hitInfo, attackRange);
        result = result && hitInfo.rigidbody.gameObject.CompareTag(Globals.PLAYER_TAG);

        return result;
    }

    
}
