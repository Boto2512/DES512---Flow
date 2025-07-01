using UnityEngine;

/// <summary>
/// This script is not an interactive object, it is just for testing.
/// </summary>
public class Deadzone : MonoBehaviour
{
    [SerializeField] private Transform destination;

    private const string playerTag="Player";
    private const string enemyTag="Enemy";

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(playerTag))
        {
           
            //SetToZero(collision.transform.GetComponent<PlayerController>());
            collision.transform.position = destination.position;
        }else if (collision.transform.CompareTag(enemyTag))
        {
            Destroy(collision.gameObject);
        }
    }

    private void SetToZero(IMomentumModifiable entity)
    {
        entity.SetMomentum(Vector3.zero);

    }
}
