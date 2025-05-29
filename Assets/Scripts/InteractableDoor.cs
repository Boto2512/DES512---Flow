using System;
using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    private bool isPlayerInRage;

    private void Update()
    {
        if (isPlayerInRage && Input.GetKeyDown(KeyCode.E))
            {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            {
            isPlayerInRage = true;
        }

    }
 
}
