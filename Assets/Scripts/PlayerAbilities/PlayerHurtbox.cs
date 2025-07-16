using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerHurtbox : MonoBehaviour {

    [Header("Swing")]
    [SerializeField] private float startingAngle;
    [SerializeField] private float endingAngle;
    [SerializeField] private float swingTime = 0.03f;
    private float swingTimer = 0f;
    private Quaternion startRotation;
    private Quaternion endRotation;

    [SerializeField] private GameObject player;
    [SerializeField] private PlayerAttackController attackController;
    [SerializeField] private Transform sweepOrigin;
    [SerializeField] private Transform cameraRotation;
    private Rigidbody rb;
    private CapsuleCollider capCollider;

    private void Awake() {
        rb = this.GetComponent<Rigidbody>();
        capCollider = this.GetComponent<CapsuleCollider>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    private void OnEnable() {
        startRotation = Quaternion.Euler(0f, attackController.Config.StartAngle, 0f);
        endRotation = Quaternion.Euler(0f, attackController.Config.EndAngle, 0f);

        rb.rotation = startRotation;
        rb.position = sweepOrigin.position;
        swingTimer = 0f;

        capCollider.height = attackController.Config.HurtboxLengthConstant + attackController.Config.HurtboxLengthMomentumMultiplier * player.GetComponent<Rigidbody>().linearVelocity.magnitude;
    }

    // Update is called once per frame
    void Update() {
        if (swingTimer > swingTime) {
            Utility.RunNextFrame(() => this.gameObject.SetActive(false)).Forget();
        }

        swingTimer += Time.deltaTime;
        float t = Mathf.Clamp01(swingTimer / swingTime);

        rb.MoveRotation(cameraRotation.rotation * Quaternion.Slerp(startRotation, endRotation, t));
        rb.MovePosition(sweepOrigin.position);
    }

    private void OnTriggerEnter(Collider other) {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
            return;

        if (!rb.gameObject.TryGetComponent<IDamageable>(out var damageable))
            return;

        attackController.DamageableHit(damageable, other);
    }

    //private void FixedUpdate() {
    //    if (swingTimer > swingTime)
    //        return;

    //    swingTimer += Time.fixedUnscaledDeltaTime;
    //    float t = Mathf.Clamp01(swingTimer / swingTime);

    //    rb.MoveRotation(Quaternion.Slerp(startingRotation, endingRotation, t));
    //    rb.MovePosition(sweepOrigin.position);
    //}
}
