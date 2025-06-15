using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Events;
using NUnit.Framework;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(Rigidbody))]
public class BounceBomb : MonoBehaviour
{
    [Header("Positioning")]
    [SerializeField] private Transform blastCentre;
    private Vector3 blastOrigin => blastCentre.position;

    [Header("Weak Blast")]
    [SerializeField, Min(0f)] private float weakBlastRadius;
    [SerializeField, Min(0f)] private float weakBlastPower = 1f;
    [SerializeField, Min(0f)] private float weakSpeedMinimum;

    [Header("Strong Blast")]
    [SerializeField, Min(0f)] private float strongBlastRadius;
    [SerializeField, Min(0f)] private float strongBlastPower = 1f;
    [SerializeField, Min(0f)] private float strongSpeedMinimum;

    [Header("Other")]
    [SerializeField, Min(0f), Tooltip("How far ahead the momentum projects the entity position")] private float entityProjectionScale = 0f;
    [SerializeField] private bool isDetonable = true;

    private Rigidbody rb;

    [Header("VFX")]
    [SerializeField] private GameObject vfxObject;
    [SerializeField] private VisualEffect vfx;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.includeLayers = Globals.STICKY_MASK;
        rb.excludeLayers = ~Globals.STICKY_MASK;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnValidate()
    {
        if (strongBlastRadius > weakBlastRadius)
        {
            weakBlastRadius = strongBlastRadius;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        rb.isKinematic = true;
        rb.detectCollisions = false;

        blastCentre.position = collision.contacts[0].point;
         CheckIfInsideBounceBombTriggerZone();
    }

    public void Throw()
    {

    }

    /// <summary>
    /// Has all the blasting logic (redirecting momentum and possibly speeding up)
    /// </summary>
    public void Activate()
    {
        if (!isDetonable)
            return;

        ExplosionVFX();

        DestroyExplosiveSteam();

        TutorialEvents.OnUsedBomb?.Invoke();

        FindBlastAffectableEntities().ForEach((x) => ModifyEnitityMomentum(x.Entity, x.InStrongBlast));
    }

    private Vector3 CalculateProjectedEntityPosition(IMomentumModifiable imm)
    {
        Vector3 projectedPosition = imm.GetPosition() + imm.GetMomentum() * entityProjectionScale;

        return projectedPosition;
    }

    /// <summary>
    /// Finds all the entities that implement IMomentumModifiable within the weakBlastRadius range, and within line of sight from the blast origin.
    /// </summary>
    /// <returns>A collection of each IMomentumModifiable entity and whether it is in the strong blast range</returns>
    private List<(IMomentumModifiable Entity, bool InStrongBlast)> FindBlastAffectableEntities()
    {
        var allColliders = new List<(IMomentumModifiable, bool)>();

        foreach (var collider in Physics.OverlapSphere(blastOrigin, weakBlastRadius))
        {
            Rigidbody rb = collider.attachedRigidbody;
            if (rb == null)
            {
                continue;
            }

            if (!rb.gameObject.TryGetComponent<IMomentumModifiable>(out IMomentumModifiable imm))
            {
                continue;
            }

            Vector3 momentumPosition = collider.ClosestPoint(blastOrigin);
            if (momentumPosition == blastOrigin)
            {
                allColliders.Add((imm, Vector3.Distance(momentumPosition, imm.GetPosition()) <= strongBlastRadius));
                continue;
            }

            Vector3 blastDirection = (momentumPosition - blastOrigin).normalized;
            if (Physics.Raycast(blastOrigin, blastDirection, out RaycastHit outHit, weakBlastRadius))
            {
                allColliders.Add((imm, outHit.distance <= strongBlastRadius));
                continue;
            }
        }

        return allColliders.ToList();
    }

    private void ModifyEnitityMomentum(IMomentumModifiable entity, bool inStrongBlast)
    {
        // sets the momentum of each blast affectable entity to at least the speed minimum of the blast radius it's in, in the direction from itself to the blast origin
        Vector3 projectedPosition = CalculateProjectedEntityPosition(entity);
        Vector3 blastToPosition = projectedPosition - blastOrigin;

        float tempSpeed = entity.GetMomentum().magnitude * (inStrongBlast ? strongBlastPower : weakBlastPower);
        float speedMinimumToUse = inStrongBlast ? strongSpeedMinimum : weakSpeedMinimum;
        float newSpeed = Mathf.Max(tempSpeed, speedMinimumToUse);

        entity.SetMomentum(blastToPosition.normalized * newSpeed);
    }

    private void ExplosionVFX()
    {
        vfxObject.GetComponent<VFXCleanUp>().StartTimer();

        vfx.transform.SetParent(null);
        vfx.SendEvent("explosionTrigger");

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, weakBlastRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, strongBlastRadius);
    }

    private void DestroyExplosiveSteam()
    {
        Collider[] colliders = Physics.OverlapSphere(blastOrigin, weakBlastRadius);

        foreach (Collider col in colliders)
        {
            ExplosiveSteam steam = col.GetComponent<ExplosiveSteam>();
            if (steam != null)
            {
                TutorialEvents.OnBarrelExploded?.Invoke();

                Destroy(steam.gameObject);
            }
        }
    }

private void CheckIfInsideBounceBombTriggerZone()
{
    float checkRadius = 0.5f; 
    Collider[] nearbyColliders = Physics.OverlapSphere(blastCentre.position, checkRadius);

    foreach (var col in nearbyColliders)
    {
        if (col.CompareTag("BombBounceZone")) 
        {
            Debug.Log("Bounce Bomb landed inside bounce zone");
             TutorialEvents.OnReachedBounceBomb?.Invoke();
            break;
        }
    }
}

}
