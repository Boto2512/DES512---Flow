using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class BounceBomb : MonoBehaviour {
    [SerializeField] private BounceBombConfig Config;

    [Header("Positioning")]
    [SerializeField] private Transform blastCentre;
    private Vector3 blastOrigin => blastCentre.position;

    [Header("Other")]
    [SerializeField] private bool isDetonable = true;

    private Rigidbody rb;

    [Header("VFX")]
    [SerializeField] private GameObject vfxObject;
    [SerializeField] private VisualEffect vfx;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.includeLayers = Globals.STICKY_MASK;
        rb.excludeLayers = ~Globals.STICKY_MASK;

        rb.angularVelocity = new(0f, 50f, 0f);
    }

    // Update is called once per frame
    void Update() {

    }

    private void OnCollisionEnter(Collision collision) {
        rb.isKinematic = true;
        rb.detectCollisions = false;
        CheckIfInsideBounceBombTriggerZone();

        this.transform.SetPositionAndRotation(collision.collider.ClosestPoint(this.transform.position), Quaternion.FromToRotation(Vector3.forward, collision.contacts[0].normal));
        blastCentre.position = collision.GetContact(0).point;
    }

    public void Throw() {

    }

    /// <summary>
    /// Has all the blasting logic (redirecting momentum and possibly speeding up)
    /// </summary>
    public void Activate() {
        if (!isDetonable)
            return;

        ExplosionVFX();
        DestroyExplosiveSteam();
        TutorialEvents.OnUsedBomb?.Invoke();

        FindBlastAffectableEntities().ForEach((x) => ModifyEnitityMomentum(x.Entity, x.InStrongBlast));
    }

    private Vector3 CalculateProjectedEntityPosition(IMomentumModifiable imm) {
        Vector3 projectedPosition = imm.GetPosition() + imm.GetMomentum() * Config.EntityProjectionScale;

        return projectedPosition;
    }

    /// <summary>
    /// Finds all the entities that implement IMomentumModifiable within the Config.WeakBlastRadius range, and within line of sight from the blast origin.
    /// </summary>
    /// <returns>A collection of each IMomentumModifiable entity and whether it is in the strong blast range</returns>
    private List<(IMomentumModifiable Entity, bool InStrongBlast)> FindBlastAffectableEntities() {
        var allColliders = new List<(IMomentumModifiable, bool)>();

        foreach (var collider in Physics.OverlapSphere(blastOrigin, Config.WeakBlastRadius)) {
            if (collider.gameObject.CompareTag("ExplosiveBarrel")) {
                collider.transform.GetComponent<IDamageable>().TakeDamage(1);
            }
            else { }

            Rigidbody rb = collider.attachedRigidbody;
            if (rb == null) {
                continue;
            }

            if (!rb.gameObject.TryGetComponent<IMomentumModifiable>(out IMomentumModifiable imm)) {
                continue;
            }

            Vector3 momentumPosition = collider.ClosestPoint(blastOrigin);
            if (momentumPosition == blastOrigin) {
                allColliders.Add((imm, Vector3.Distance(momentumPosition, imm.GetPosition()) <= Config.StrongBlastRadius));
                continue;
            }

            Vector3 blastDirection = (momentumPosition - blastOrigin).normalized;
            if (Physics.Raycast(blastOrigin, blastDirection, out RaycastHit outHit, Config.WeakBlastRadius)) {
                allColliders.Add((imm, outHit.distance <= Config.StrongBlastRadius));
                continue;
            }


        }

        return allColliders.ToList();
    }

    private void ModifyEnitityMomentum(IMomentumModifiable entity, bool inStrongBlast) {
        // sets the momentum of each blast affectable entity to at least the speed minimum of the blast radius it's in, in the direction from itself to the blast origin
        Vector3 projectedPosition = CalculateProjectedEntityPosition(entity);
        Vector3 directionFromBlast = (projectedPosition - blastOrigin).normalized;

        Vector3 momentum = entity.GetMomentum();

        float speedMinimumToUse = inStrongBlast ? Config.StrongSpeedMinimum : Config.WeakSpeedMinimum;
        float speedMultiplierToUse = inStrongBlast ? Config.StrongBlastPower : Config.WeakBlastPower;

        float newVerticalSpeed, newHorizontalSpeed;
        Vector3 newMomentum;
        if (Config.UseFixedVerticalSpeed) {
            newVerticalSpeed = Mathf.Max(momentum.y + Config.FixedVerticalSpeed, Config.VerticalGainMinimum);
            newHorizontalSpeed = Mathf.Max(momentum.Horizontal().magnitude * speedMultiplierToUse, speedMinimumToUse);
            newMomentum = new(directionFromBlast.x * newHorizontalSpeed, newVerticalSpeed, directionFromBlast.z * newHorizontalSpeed);
        }
        else {
            float newSpeed = Mathf.Max(momentum.magnitude * speedMultiplierToUse, speedMinimumToUse);
            Vector3 proposedMomentum = directionFromBlast * newSpeed;

            newVerticalSpeed = proposedMomentum.y * Config.VerticalGainRatio;
            newHorizontalSpeed = Mathf.Sqrt(newSpeed * newSpeed - newVerticalSpeed * newVerticalSpeed);
            newMomentum = new(directionFromBlast.x * newHorizontalSpeed, directionFromBlast.y * newVerticalSpeed, directionFromBlast.z * newHorizontalSpeed);
        }

        //Vector3 newMomentum = new(directionFromBlast.x * newHorizontalSpeed, newVerticalSpeed, directionFromBlast.z * newHorizontalSpeed);
        //Debug.Log($"Old: {entity.GetMomentum()}; New: {newMomentum}; InStrongBlast: {inStrongBlast}; Old Horizontal Momentum: {momentum.Horizontal().magnitude}; New Horizontal Momentum: {newHorizontalSpeed}");
        entity.SetMomentum(newMomentum);
        entity.BeenBombBounced();
    }

    private void ExplosionVFX() {
        AudioManager.instance?.Play("BombBounce");
        vfxObject.GetComponent<VFXCleanUp>().StartTimer();

        vfx.transform.SetParent(null);
        vfx.SendEvent("explosionTrigger");

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Config.WeakBlastRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Config.StrongBlastRadius);
    }

    private void DestroyExplosiveSteam() {
        Collider[] colliders = Physics.OverlapSphere(blastOrigin, Config.WeakBlastRadius);

        foreach (Collider col in colliders) {
            ExplosiveSteam steam = col.GetComponent<ExplosiveSteam>();
            if (steam != null) {
                Destroy(steam.gameObject);
                TutorialEvents.OnBarrelExploded?.Invoke();
            }
        }
    }

    private void CheckIfInsideBounceBombTriggerZone() {
        float checkRadius = 0.5f;
        Collider[] nearbyColliders = Physics.OverlapSphere(blastCentre.position, checkRadius);

        foreach (var col in nearbyColliders) {
            if (col.CompareTag("BombBounceZone")) {
                Debug.Log("Bounce Bomb landed inside bounce zone");
                TutorialEvents.OnReachedBounceBomb?.Invoke();
                break;
            }
        }
    }
}
