using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] int childIndex;
    private void OnTriggerEnter(Collider other)
    {
       transform.parent.GetComponent<CheckpointManager>().UpdateCheckpoint(childIndex);
    }
}
