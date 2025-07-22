using UnityEngine;

/// <summary>
/// This script is not an interactive object, it is just for testing.
/// </summary>
public class Deadzone : MonoBehaviour
{
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private float damage;
    private const string playerTag = "Player";
    private const string enemyTag = "Enemy";

    private void Start()
    {
        checkpointManager = FindFirstObjectByType<CheckpointManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(playerTag))
        {
            collision.rigidbody.GetComponent<IDamageable>().TakeDamage(damage);
            checkpointManager.MovePlayer();
        }
        else if (collision.transform.CompareTag(enemyTag))
        {
            Destroy(collision.gameObject);
        }
    }

    private void SetToZero(IMomentumModifiable entity)
    {
        entity.SetMomentum(Vector3.zero);

    }
}
