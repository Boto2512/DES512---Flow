using UnityEngine;

public class ParryProjectile : MonoBehaviour {
    public float Damage;
    public Vector3 Momentum;

    [SerializeField] private float lifespan = 20f;
    private float lifetimer = 0f;

    private Rigidbody rb;

    private void Awake() {
        rb = this.GetComponent<Rigidbody>();
    }

    private void Start() {
        this.transform.Rotate(90f, 0f, 0f);
        rb.linearVelocity = Momentum;
    }

    private void Update() {
        lifetimer += Time.deltaTime;
        if (lifetimer >= lifespan) {
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        Debug.Log("PP hit " + collision.gameObject.name);

        Rigidbody otherRB = collision.rigidbody;
        if (otherRB == null) {
            if (Utility.IsObstacle(collision.gameObject.layer)) {
                Destroy(this.gameObject);
            }
            return;
        }

        if (!otherRB.gameObject.TryGetComponent<IDamageable>(out var damageable))
            return;

        damageable.TakeDamage(Damage);
        Destroy(this.gameObject);
    }
}
