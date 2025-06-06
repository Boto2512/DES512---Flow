using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 4;
    [SerializeField] private Vector3 playerLocation;
    [SerializeField] private float timeToFire;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float damageAmount = 10;
    private Vector3 direction;
    private Rigidbody rb;
    //[SerializeField] private VisualEffect visualEffect;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerLocation = Globals.PLAYER.transform.position;

        rb = GetComponent<Rigidbody>();

        direction = (playerLocation - rb.position).normalized;

        // Optional: face the projectile toward the player
        rb.rotation = Quaternion.LookRotation(direction);

        //VFX positoion set.
        //VFXEventAttribute eventAttribute = visualEffect.CreateVFXEventAttribute();


    }



    void Update()
    {

        rb.position += direction * projectileSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        //
        if (other.attachedRigidbody.CompareTag(Globals.PLAYER_TAG))
        {
            Debug.Log("Steve");
            //Damage player
            //VFXEventAttribute eventAttribute = visualEffect.CreateVFXEventAttribute();
            //visualEffect.SendEvent("OnPlay", eventAttribute);
            //Play VFX explosion
            //delay the destroying the game object

            other.GetComponent<IDamageable>().TakeDamage(damageAmount);

            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
