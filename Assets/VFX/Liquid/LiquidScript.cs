using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidScript : MonoBehaviour
{
    Renderer rend;
    Vector3 lastPos;
    Vector3 velocity;
    Vector3 lastRot;
    [SerializeField] Vector3 angularVelocity;
    [SerializeField] private float placeWobble = 0.05f;
    [SerializeField] private float wobbleSpeed = 1f;
    [SerializeField] private float recovery = 1f;
    [SerializeField] float wobbleAmountX;
    [SerializeField] float wobbleAmountZ;
    [SerializeField] float wobbleAmountToAddX;
    [SerializeField] float wobbleAmountToAddZ;
    [SerializeField] float pulse;
    [SerializeField] float time = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rend = GetComponent<Renderer>();
             
    }

    // Update is called once per frame
    private void Update()
    {
        time += Time.deltaTime;
        //Decreases wobble of liquid over time
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * (recovery));
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * (recovery));

        //Shader Updates
        rend.material.SetFloat("_WobbleAxisX", wobbleAmountToAddX);
        rend.material.SetFloat("_WobbleAxisZ", wobbleAmountToAddZ);

        //Velocity
        velocity = (lastPos - transform.position) / Time.deltaTime;
        angularVelocity = transform.rotation.eulerAngles - lastRot;

        //ADDS velocity to the wobble

        wobbleAmountToAddX += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * placeWobble, -placeWobble, placeWobble);
        wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * placeWobble, -placeWobble, placeWobble);

        //Last Position
        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;

    }
}
