using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BounceBomb : MonoBehaviour
{
    [SerializeField] private Transform blastCentre;
    private Vector3 blastOrigin => blastCentre.position;

    /// CONSIDERATION: blast power could be what multiplier is used if under the speed minimum
    
    [Header("Weak Blast")]
    [SerializeField, Min(0f)] private float weakBlastRadius;
    [SerializeField, Min(0f)] private float weakBlastPower;
    [SerializeField, Min(0f)] private float weakSpeedMinimum;

    [Header("Strong Blast")]
    [SerializeField, Min(0f)] private float strongBlastRadius;
    [SerializeField, Min(0f)] private float strongBlastPower;
    [SerializeField, Min(0f)] private float strongSpeedMinimum;

    [Header("Other")]
    [SerializeField] private bool isDetonable = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate() {
        if (strongBlastRadius > weakBlastRadius) {
            strongBlastRadius = weakBlastRadius;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody) ;
    }

    public void Throw() {

    }

    /// <summary>
    /// Has all the blasting logic (redirecting momentum and possibly speeding up)
    /// </summary>
    public void Activate() {
        if (!isDetonable)
            return;

        var affectableEntities = FindBlastAffectableEntities();

        // sets the momentum of each blast affectable entity to at least the speed minimum of the blast radius it's in, in the direction from itself to the blast origin
        foreach ((var entity, bool inStrongBlast) in affectableEntities) {
            Vector3 momentum = entity.GetMomentum();
            Vector3 direction = Vector3.Normalize(entity.GetPosition() - blastOrigin);

            float speedMinimumToUse = inStrongBlast ? strongSpeedMinimum : weakSpeedMinimum;
            Vector3 newMomentum = direction * MathF.Max(momentum.magnitude, speedMinimumToUse);

            entity.SetMomentum(newMomentum);
        }
    }

    /// <summary>
    /// Finds all the entities that implement IMomentumModifiable within the weakBlastRadius range, and within line of sight from the blast origin.
    /// </summary>
    /// <returns>A collection of each IMomentumModifiable entity and whether it is in the strong blast range</returns>
    private IEnumerable<(IMomentumModifiable Entity, bool InStrongBlast)> FindBlastAffectableEntities() {
        var allColliders = new List<(IMomentumModifiable, bool)>();

        foreach (var collider in Physics.OverlapSphere(blastOrigin, weakBlastRadius)) {
            var rb = collider.attachedRigidbody;
            if (rb == null) {
                continue;
            }

            var imm = rb.gameObject.GetComponent<IMomentumModifiable>();
            if (imm != null && Physics.Raycast(blastOrigin, collider.ClosestPoint(blastOrigin) - blastOrigin, out RaycastHit outHit, weakBlastRadius)) {
                allColliders.Add((imm, outHit.distance <= strongBlastRadius));
            }
        }

        return allColliders;
    }
}
