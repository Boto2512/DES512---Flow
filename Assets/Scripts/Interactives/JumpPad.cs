using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float jumpForce;

    private void jump(Rigidbody rb)
    {
        rb.AddForce(rb.transform.up*jumpForce);
    }
}
