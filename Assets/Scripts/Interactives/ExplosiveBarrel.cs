using UnityEngine;
using UnityEngine.VFX;

public class ExplosiveBarrel : MonoBehaviour, IDamageable, IMomentumModifiable {
    [SerializeField] private float damage;
    [SerializeField] private float force;
    [SerializeField] private float explosiveRange;
    [SerializeField] private LayerMask explosiveMask;
    [SerializeField] private Transform ground;

    [SerializeField] private VisualEffect explosion;
    private float health = 1;
    void Awake() {
        explosion.Stop();
    }
    private void Explode() {
        Collider[] explosionHits = Physics.OverlapSphere(transform.position, explosiveRange, explosiveMask);
        explosion.Play();

        foreach (Collider entity in explosionHits) {
            if (entity.transform == this.transform) { return; }

            Transform parent = entity.attachedRigidbody.transform;
            if (parent.transform.TryGetComponent<IDamageable>(out IDamageable iDamage)) {
                iDamage.TakeDamage(damage);
            }
            if (parent.transform.TryGetComponent<IMomentumModifiable>(out IMomentumModifiable iMomentum)) {
                Vector3 directionForce = (iMomentum.GetPosition() - ground.position).normalized * force;
                Debug.Log($"{parent.name}: {directionForce}");
                iMomentum.SetMomentum(directionForce);
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosiveRange);
    }
    #region ========================= IDamageable Interface =========================
    public float GetHealth() {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(float value) {
        health -= value;
        bool once = false;
        if (health <= 0 && !once) { Kill(); once = true; }
    }

    public void Kill() {
        Explode();
        explosion.GetComponent<VFXCleanUp>().StartTimer();
        explosion.transform.SetParent(null);
        Destroy(gameObject);
    }

    public void Heal(float value) {
        throw new System.NotImplementedException();
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    public Vector3 GetMomentum() {
        return Vector3.zero;
    }

    public void SetMomentum(Vector3 value) {
    }

    public void BeenBombBounced() {

    }

    #endregion ========================= IDamageable Interface =========================
}
