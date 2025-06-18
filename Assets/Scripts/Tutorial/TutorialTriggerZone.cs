using UnityEngine;


public class TutorialTriggerZone : MonoBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TutorialManager.Instance.NotifyStepConfirmed();
            
            Debug.Log("dsfsdfbjbjsdkfbsdjk");
        }
    }
}
