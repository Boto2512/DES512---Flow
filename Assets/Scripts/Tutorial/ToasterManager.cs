using TMPro;
using UnityEngine;

public class ToasterManager : MonoBehaviour
{
    public static ToasterManager Instance;

    [SerializeField]
    private TextMeshProUGUI toasterMessage;



    private GameObject toasterObject;

    private const float TOAST_DURATION_SECONDS = 5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        toasterObject = toasterMessage?.gameObject;
        HideToaster();
    }

    public void ShowToaster(string message)
    {
        if (toasterMessage == null || toasterObject == null)
        {
            Debug.LogWarning("Toaster references are not properly assigned.");
            return;
        }

        toasterMessage.text = message;
        toasterObject.SetActive(true);
        

        CancelInvoke(nameof(HideToaster));
        Invoke(nameof(HideToaster), TOAST_DURATION_SECONDS);
    }

    private void HideToaster()
    {
        toasterObject?.SetActive(false);
        
    }
}
