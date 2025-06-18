using UnityEngine;


public class TutorialTriggerZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TutorialManager.Instance.NotifyStepConfirmed();
            Debug.Log("dsfsdfbjbjsdkfbsdjk");
        }
    }
}
