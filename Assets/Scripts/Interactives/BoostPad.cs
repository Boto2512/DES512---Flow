using Unity.VisualScripting;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [SerializeField] private float force;

    [SerializeField] private float height;

    [SerializeField] private Transform cylinder;

    private void Update() {
        cylinder.localScale = new Vector3(2, height, 2);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IMomentumModifiable momentum = other.attachedRigidbody.GetComponent<IMomentumModifiable>();
            momentum.SetMomentum(Vector3.up * force);
        }
    }
}
