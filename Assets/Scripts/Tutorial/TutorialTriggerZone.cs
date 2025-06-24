using UnityEngine;

public class TutorialTriggerZone : MonoBehaviour
{
    [SerializeField]
    private int stepIndex = 0; 

    private bool hasTriggered = false;

    private void OnCollisionEnter(Collision collision)
{
    if (hasTriggered) return;
    if (!TutorialManager.Instance.IsCurrentStep(stepIndex)) return;

    if (collision.gameObject.CompareTag("Player"))
    {
        hasTriggered = true;
        TutorialManager.Instance.NotifyStepConfirmed();
        Debug.Log($"Tutorial Step {stepIndex} confirmed.");
    }
}

}