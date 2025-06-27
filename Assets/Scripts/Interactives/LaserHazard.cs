using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserHazard : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private float laserLength = 10f;
    [SerializeField] private float laserWidth = 0.1f;
    [SerializeField] private float laserDuration = 0.2f;
    [SerializeField] private float shootInterval = 2f;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask hitLayers;

    [Header("Player Setting")]
    [SerializeField] private float damage;

    private LineRenderer lineRenderer;
    private float timer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= shootInterval)
        {
            timer = 0f;
            ShootLaser();
        }
    }

    private void ShootLaser()
    {
        Vector3 start = transform.position;
        Vector3 direction = transform.forward;
        Vector3 end = start + direction * laserLength;

        // Check for hit
        if (Physics.Raycast(start, direction, out RaycastHit hit, laserLength, hitLayers))
        {
            end = hit.point;
            Debug.Log("Damaged!");
            IDamageable damageable = hit.transform.GetComponent<IDamageable>();
            damageable.TakeDamage(damage);
        }
        Debug.DrawLine(start, end,Color.red, laserDuration);
        // Draw the laser
        StartCoroutine(FireLaserEffect(start, end));
    }

    private System.Collections.IEnumerator FireLaserEffect(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(laserDuration);

        lineRenderer.enabled = false;
    }
}
