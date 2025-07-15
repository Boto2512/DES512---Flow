using Unity.VisualScripting;
using UnityEngine;

public class BoostPadVertical : MonoBehaviour
{
    [SerializeField] private float force;

    [SerializeField] private float height;

    private Transform cylinder;

    private void Update() {
        cylinder = transform;
        cylinder.localScale = new Vector3(2, height, 2);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IMomentumModifiable momentum = other.attachedRigidbody.GetComponent<IMomentumModifiable>();
            Vector3 playerMomentum = momentum.GetMomentum();
            Vector3 boostForce = transform.up * force;
            momentum.SetMomentum(new Vector3 (playerMomentum.x + boostForce.x, boostForce.y, playerMomentum.z + boostForce.z));
            
        }
    }
}
