using TMPro;
using UnityEngine;

public class ToasterManager : MonoBehaviour
{
    public static ToasterManager Instance;

    [SerializeField] private TextMeshProUGUI toasterMessage;
    [SerializeField] private Animator toasterAnimator;

    private GameObject toasterObject;
    [SerializeField] GameObject AnimationObject;

    private const float TOAST_DURATION_SECONDS = 5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        toasterObject = toasterMessage?.gameObject;
        HideToaster();
    }

    /// <summary>
    /// Show toaster UI with text and optional animation trigger.
    /// </summary>
    public void ShowToaster(string message, string animationTrigger = null)
    {
        if (toasterMessage == null || toasterObject == null)
        {
            Debug.LogWarning("Toaster references are not properly assigned.");
            return;
        }

        toasterMessage.text = message;
        toasterObject.SetActive(true);
        AnimationObject.SetActive(true);

        // Trigger animation if set
        if (toasterAnimator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            toasterAnimator.ResetTrigger(animationTrigger); // optional: clear first
            toasterAnimator.SetTrigger(animationTrigger);
        }

        CancelInvoke(nameof(HideToaster));
        Invoke(nameof(HideToaster), TOAST_DURATION_SECONDS);
    }

    private void HideToaster()
    {
        toasterObject?.SetActive(false);
        AnimationObject?.SetActive(false);
    }
}
