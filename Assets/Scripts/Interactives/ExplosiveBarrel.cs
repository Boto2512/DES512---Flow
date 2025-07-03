using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour, IDamageable
{
    [SerializeField] private float damage;
    [SerializeField] private float force;
    [SerializeField] private float explosiveRange;
    [SerializeField] private LayerMask explosiveMask;
    private float health = 1;

    private void Explode() {
        Debug.Log("EXPLODE¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬¬");
        Collider[] explosionHits = Physics.OverlapSphere(transform.position, explosiveRange, explosiveMask);
        Debug.Log($"in array {explosionHits.Length} raycasts");

        foreach (Collider entity in explosionHits) {
            Debug.Log(entity.transform.name);
            if (entity.transform == this.transform) { return; }

            Transform parent = entity.attachedRigidbody.transform;
            if (parent.transform.TryGetComponent<IDamageable>(out IDamageable iDamage)) {
            iDamage.TakeDamage(damage); 
            }
            if (parent.transform.TryGetComponent<IMomentumModifiable>(out IMomentumModifiable iMomentum)) {
            Vector3 directionForce = (iMomentum.GetPosition() - transform.forward).normalized * force;
            iMomentum.SetMomentum(directionForce); 
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosiveRange);
    }
    #region ========================= IDamageable Interface =========================
    public float GetHealth() {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(float value) {
        Debug.Log("Damage-----------------------------------");
        health -= value;
        bool once = false;
        if (health <=0 && !once) { Kill(); once = true; }
    }

    public void Kill() {
        Explode();
        //Destroy(gameObject);
    }

    public void Heal(float value) {
        throw new System.NotImplementedException();
    }

    #endregion ========================= IDamageable Interface =========================
}
