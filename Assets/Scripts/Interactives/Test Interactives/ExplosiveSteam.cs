using UnityEngine;

public class ExplosiveSteam : MonoBehaviour
{
//    [SerializeField] private float jumpForce;
//    private bool isInteracting = false;
//    private IMomentumModifiable interactingEntity;

//    private void Update()
//    {
//        if (isInteracting)
//        {
//            Jump(interactingEntity);
//            Destroy(gameObject);
//        }
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        if (collision.transform.CompareTag("Player"))
//        {
//            interactingEntity = collision.transform.GetComponent<IMomentumModifiable>();
//            isInteracting = true;
//        }
//    }

//    private void OnCollisionExit(Collision collision)
//    {
//        if (collision.transform.CompareTag("Player"))
//        {
//            isInteracting = false;
//            interactingEntity = null;
//        }
//    }

//    private void Jump(IMomentumModifiable entity)
//    {
//        if (entity != null)
//        {
//            entity.SetMomentum(entity.GetMomentum() + Vector3.up * jumpForce);
//        }
//    }
}