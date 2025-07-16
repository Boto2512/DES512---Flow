using UnityEngine;

public class ParryProjectile : MonoBehaviour {
    public float Damage;
    public Vector3 Momentum;

    private Rigidbody rb;

    private void Awake() {
        rb = this.GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        rb.MovePosition(rb.position + Momentum);
    }

    private void OnCollisionEnter(Collision collision) {
        Destroy(this.gameObject, 0f);

        Rigidbody otherRB = collision.rigidbody;
        if (otherRB == null)
            return;

        if (!otherRB.gameObject.TryGetComponent<IDamageable>(out var damageable))
            return;

        damageable.TakeDamage(Damage);
        //Destroy(this.gameObject);
    }
}
