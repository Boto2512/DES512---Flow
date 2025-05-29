using System;
using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    private bool isPlayerInRange;

     void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
            Destroy(gameObject);
        }
    }
     void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
            {
            isPlayerInRange = true;
        }

    }
 
}
