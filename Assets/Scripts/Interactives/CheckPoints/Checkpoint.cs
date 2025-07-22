using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    int childIndex;
    private void OnTriggerEnter(Collider other)
    {
       transform.parent.GetComponent<CheckpointManager>().UpdateCheckpoint(childIndex);
    }

    public void GetChildIndex(int index)
    {
        childIndex = index;
    }
}
