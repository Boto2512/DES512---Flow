using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class TurretEnemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float health;
    [SerializeField] private Slider healthBar;

    [Header("Attack")]    
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackRadius;
    [SerializeField] private float attackRange;

    private float chargeTimer;
    [SerializeField] private float attackChargeTime;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackDelay;
    [SerializeField] private LayerMask canHitMask;

    [SerializeField] private GameObject healthDrop;

    [SerializeField] private VisualEffect attackVFX;
    [SerializeField] private Transform orb;
    private Transform player;
    private void Start() {
        player = Globals.PLAYER.transform;
        healthBar.maxValue = health;
        healthBar.value = health;   

        if (attackVFX.HasFloat("Duration"))
        {
            attackVFX.SetFloat("Duration", attackDelay);
        }
        if (attackVFX.HasFloat("LaserWidth"))
        {
            attackVFX.SetFloat("LaserWidth", attackRadius);
        }
    }

    private void Update() {
        if (IsInRange()) { ChargingAttack(); }
        else { return; }
    }

    private bool IsInRange() {
        Vector3 start = orb.position;
        Vector3 direction = (player.position - orb.position).normalized;

        if (Vector3.Distance(transform.position, player.position) >= attackRange) {
            return false;
        }
        else  {
            Physics.Raycast(start, direction, out RaycastHit hit, attackRange, canHitMask);
            
            if(hit.transform != null) {
                
                if (hit.transform.CompareTag("Player")) { 
                    orb.LookAt(player);
                    return true;
                }
                else { return false; }
            }
            else { return false; }
        }
    }

    private void ChargingAttack() {
        chargeTimer += Time.deltaTime;
        if (chargeTimer > attackChargeTime) { StartCoroutine(Attack()); }
    }

    private IEnumerator Attack() {
        chargeTimer = 0;
        Vector3 start = orb.position;
        Vector3 direction = (player.position - orb.position).normalized;

        yield return new WaitForSeconds(attackDelay);

        Physics.Raycast(start, direction, out RaycastHit hit, attackRange, canHitMask);
        if (hit.collider != null) {
            Debug.Log($"{hit.transform.name}");
        } 
        else { Debug.Log("Attack Missed"); }
        Debug.DrawRay(start, direction * attackRange, Color.darkRed, 3f);

        if (attackVFX.HasFloat("LaserLength")) { attackVFX.SetFloat("LaserLength", hit.distance); }
        attackVFX.Play();

        if (hit.transform != null) {
            if (hit.transform.CompareTag("Player")) {
                hit.transform.GetComponent<IDamageable>().TakeDamage(attackRange);
            }
            yield return null;
        } 
        else { 
            yield return null; 
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #region ========================= IDamageable Interface =========================
    public float GetHealth()
    {
        return health;
    }

    public void Heal(float value)
    {
        throw new System.NotImplementedException();
    }
    public void TakeDamage(float value)
    {
        health -= value;
        healthBar.value = health;
        if (health < 0) { 
            Kill();
        }
    }

    public void Kill()
    {
        Instantiate(healthDrop, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    #endregion ========================= IDamageable Interface =========================
}
