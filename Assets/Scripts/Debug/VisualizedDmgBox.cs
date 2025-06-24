using UnityEngine;

public class VisualizedDmgBox : MonoBehaviour
{
    [SerializeField] private PlayerVariablesConfig PlayerVariablesConfig;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (Globals.PLAYER.Target)
        {
            Vector3 center = Globals.PLAYER.Target.position + Globals.PLAYER.Target.forward * PlayerVariablesConfig.attackRange * 0.5f;
            //Vector3 center = Camera.main.transform.position + Camera.main.transform.forward * PlayerVariablesConfig.attackRange * 0.5f;
            float z_len = 2 * PlayerVariablesConfig.attackScale.z * 2 + PlayerVariablesConfig.attackRange;
            Vector3 scale = new Vector3(PlayerVariablesConfig.attackScale.x, PlayerVariablesConfig.attackScale.y, z_len);
            Gizmos.DrawWireCube(center, scale);
        }
    }

}
