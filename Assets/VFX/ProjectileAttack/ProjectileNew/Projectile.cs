using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float fireRate = 4;
    [SerializeField] private Vector3 playerLocation;
    [SerializeField] private float timeToFire;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //playerLocation = Globals.PLAYER; 
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
