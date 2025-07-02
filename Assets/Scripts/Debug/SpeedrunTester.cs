using UnityEngine;
using System;

public class SpeedrunTester : MonoBehaviour
{

    private DateTime startTime;

    private const string tag_player = "Player";



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime=DateTime.Now;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(tag_player))
        {
            Debug.LogError($"Arrived at the destination.\nTime:{DateTime.Now.Subtract(startTime).TotalSeconds}");
        }
    }

}
