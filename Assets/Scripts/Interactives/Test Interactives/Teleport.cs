using UnityEngine;

/// <summary>
/// This script is not an interactive object, it is just for testing.
/// </summary>
public class Teleport : MonoBehaviour
{
    [SerializeField] private string triggeredTag;
    [SerializeField] private Transform destination;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(triggeredTag))
        {
           
            //SetToZero(collision.transform.GetComponent<PlayerController>());
            collision.transform.position = destination.position;
        }
    }

    private void SetToZero(IMomentumModifiable entity)
    {
        entity.SetMomentum(Vector3.zero);

    }
}
