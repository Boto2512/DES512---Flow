using UnityEngine;

public class LaserTrap : MonoBehaviour {

    [Header("Laser")]
    [SerializeField] private float laserDamage;
    [SerializeField] private float laserRadius;
                     private float laserLength;
    [SerializeField] private float laserDuration;
    [SerializeField] private float shootInterval;
    [SerializeField] private LayerMask canHitMask;

    [Header("Distance Check")]
    [SerializeField] private LayerMask distanceMask;

    private void Start()    {
        GetLength();
    }

    private void Update()
    {
        this.InvokeExclusive("ShootLaser", ShootLaser, shootInterval);
    }
    private void GetLength() {
        Vector3 start = transform.position;
        Vector3 direction = transform.forward;
        
        if( Physics.Raycast(start, direction, out RaycastHit hit, Mathf.Infinity, distanceMask)) { 
        laserLength = hit.distance;
        } 
        else { 
        laserLength = Mathf.Infinity;        
        }
    }

    private void ShootLaser() {
        Vector3 start = transform.position;
        Vector3 direction = transform.forward;
        RaycastHit[] hits = Physics.SphereCastAll(start, laserRadius, direction, laserLength, canHitMask);

        Debug.DrawRay(start, direction * laserLength, Color.darkRed, laserDuration);

        foreach (RaycastHit hit in hits) {
            if (hit.transform.GetComponent<IDamageable>() != null) {
            hit.transform.GetComponent<IDamageable>().TakeDamage(laserDamage);
            }
        }
    }


}
