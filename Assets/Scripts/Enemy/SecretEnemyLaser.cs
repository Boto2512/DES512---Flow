using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class SecretEnemyLaser : MonoBehaviour
{
    [Header("Laser")]
    [SerializeField] private float laserDamage;
    [SerializeField] private float laserRadius;
    private float laserLength;
    [SerializeField] private float shootInterval;
    [SerializeField] private LayerMask canHitMask;
    [SerializeField] private VisualEffect laserVFX;

    [Header("Distance Check")]
    [SerializeField] private LayerMask distanceMask;

    private bool isShooting=false;
    private float shootingTimer;


    private void Start()
    {
        laserVFX.Stop();
        //GetLength();

        
        
        if (laserVFX.HasFloat("LaserWidth"))
        {
            laserVFX.SetFloat("LaserWidth", laserRadius);
        }
    }
    private void FixedUpdate()
    {
        if (isShooting) {
            if (shootingTimer==0)
            {
                applyDamage();
            }

            shootingTimer += Time.fixedDeltaTime;
            if (shootingTimer>=shootInterval)
            {
                shootingTimer = 0;
            }            
        }
        
    }

    private void GetLength()
    {
        Vector3 start = transform.position;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(start, direction, out RaycastHit hit, Mathf.Infinity, distanceMask))
        {
            laserLength = hit.distance;
        }
        else
        {
            laserLength = Mathf.Infinity;
        }

        Debug.Log($"length={laserLength}");
    }


    public void ShootLaser(float duration)
    {
        Debug.Log("Shoot laser");
        GetLength();
        if (laserVFX.HasFloat("LaserLength"))
        {
            laserVFX.SetFloat("LaserLength", laserLength);
        }

        if (laserVFX.HasFloat("Duration"))
        {
            laserVFX.SetFloat("Duration", duration);
        }

        Vector3 start = transform.position;
        Vector3 direction = transform.forward;
        if (laserLength<100)
        {
            Debug.DrawRay(start, direction * laserLength, Color.darkRed, duration);
        }
        else
        {
            Debug.DrawRay(start, direction * 100, Color.darkRed, duration);
        }

        
        laserVFX.Play();

        shootingTimer = 0;
        isShooting = true;
        this.InvokeExclusive("TurnOff",()=> { isShooting = false; laserVFX.Stop(); },duration);
    }

    private void applyDamage()
    {
        Vector3 start = transform.position;
        Vector3 direction = transform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(start, laserRadius, direction, laserLength, canHitMask);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.GetComponent<IDamageable>() != null)
            {
                hit.transform.GetComponent<IDamageable>().TakeDamage(laserDamage);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, laserRadius);
    }
}
