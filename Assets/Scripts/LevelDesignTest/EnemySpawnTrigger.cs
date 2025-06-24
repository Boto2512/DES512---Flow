using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    [SerializeField] private GameObject objectToEnable;
    [SerializeField] private bool triggerOnce = true;
    private bool hasTriggered = false;

    private void OnCollisionEnter(Collision other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.gameObject.CompareTag("Player") && objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            hasTriggered = true;

            Debug.Log($"{objectToEnable.name} has been enabled by player trigger.");
        }
    }
    


}
