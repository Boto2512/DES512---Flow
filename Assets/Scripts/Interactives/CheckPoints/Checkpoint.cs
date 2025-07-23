using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    int childIndex;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) {
       transform.parent.GetComponent<CheckpointManager>().UpdateCheckpoint(childIndex); 
        }
    }

    public void GetChildIndex(int index)
    {
        childIndex = index;
    }
}
