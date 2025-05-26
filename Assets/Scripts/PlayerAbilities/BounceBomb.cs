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

    private bool isDetonable = false;

    public UnityEvent EventThrow = new UnityEvent();
    public UnityEvent EventActivate = new UnityEvent();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventThrow.AddListener(Throw);
        EventActivate.AddListener(Activate);
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
            var momentum = entity.GetMomentum();
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
        RaycastHit outHit = new();

        // gets all colliders that implement IMomentumModifiable that are within the largest allowed radius (weak) and are in the line of sight from the entity to the bomb
        // returns the IMomentumModifiable object and couples it with a boolean dictating whether it's in the strong blast range
        var allColliders = Physics.OverlapSphere(blastOrigin, weakBlastRadius)
            .Where(collider => {
                if (collider is IMomentumModifiable 
                        && Physics.Raycast(blastOrigin, Vector3.Normalize(collider.ClosestPoint(blastOrigin) - blastOrigin), out RaycastHit tempOutHit, weakBlastRadius)) {
                    outHit = tempOutHit;
                    return true;
                }
                return false;
            }).Select(collider => (collider as IMomentumModifiable, outHit.distance <= strongBlastRadius));

        return allColliders;
    }
}
