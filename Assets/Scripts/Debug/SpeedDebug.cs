using TMPro;
using UnityEngine;

public class SpeedDebug : MonoBehaviour
{
    [SerializeField]private PlayerController controller;
    [SerializeField] private TextMeshProUGUI txt;



    private void FixedUpdate()
    {
        //Debug.Log($"Player Current Speed:{controller.GetMomentum().magnitude}");
        txt.text = $"{controller.GetMomentum().magnitude}";
    }
}
