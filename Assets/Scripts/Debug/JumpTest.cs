using UnityEngine;

public class JumpTest : MonoBehaviour
{
    [SerializeField] private float jumpForce;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Jump(Globals.PLAYER);
        }
    }

    private void Jump(IMomentumModifiable entity)
    {
        entity.SetMomentum(entity.GetMomentum() + transform.up * jumpForce);
    }
}
