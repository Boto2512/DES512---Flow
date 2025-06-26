using UnityEngine;

public class TutorialTriggerZone : MonoBehaviour
{
    [SerializeField]
    private int stepIndex = 0; 

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider collision)
{
    if (hasTriggered) return;
    if (!TutorialManager.Instance.IsCurrentStep(stepIndex)) return;

    if (collision.transform.parent.CompareTag("Player"))
    {
        hasTriggered = true;
        TutorialManager.Instance.NotifyStepConfirmed();
        Debug.Log($"Tutorial Step {stepIndex} confirmed.");
    }
}

}