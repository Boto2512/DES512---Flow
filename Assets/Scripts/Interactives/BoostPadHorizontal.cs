using Unity.VisualScripting;
using UnityEngine;

public class BoostPadHorizontal : MonoBehaviour
{
    [SerializeField] private float force;

    [SerializeField] private float length;
    private Transform cylinder;

    private void Start()
    {
        cylinder = transform;
    }
    private void Update() {
        cylinder.localScale = new Vector3(2, length, 2);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IMomentumModifiable momentum = other.attachedRigidbody.GetComponent<IMomentumModifiable>();
            

            Vector3 applyForce = new Vector3(0f, force, 0f);

            applyForce = transform.rotation * applyForce;
            momentum.AddMomentum(applyForce);
            Debug.Log($"force {applyForce}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
    }
}
