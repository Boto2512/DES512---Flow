using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float jumpForce;
    [SerializeField] private string triggeredTag;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(triggeredTag))
        {
            Jump(collision.transform.GetComponent<PlayerController>());
        }
    }

    private void Jump(IMomentumModifiable entity)
    {
        entity.SetMomentum(entity.GetMomentum()+transform.up * jumpForce);
    }
}
