using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent navAgent;

    private void Start() {
        navAgent = GetComponent<NavMeshAgent>();
    }
}
