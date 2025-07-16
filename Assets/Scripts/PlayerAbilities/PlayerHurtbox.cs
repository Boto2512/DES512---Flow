using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerHurtbox : MonoBehaviour {

    private float swingTimer = 0f;
    private Quaternion startRotation;
    private Quaternion endRotation;

    [Header("Relations")]
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerAttackController attackController;
    [SerializeField] private Transform sweepOrigin;
    [SerializeField] private Transform cameraRotation;
    [SerializeField] private Transform HurtboxTip;
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
        capCollider.center = new(capCollider.center.x, capCollider.center.y, capCollider.height / 2f);
        HurtboxTip.position = new(0f, 0f, capCollider.height);
    }

    // Update is called once per frame
    void Update() {
        if (swingTimer > attackController.Config.SwingTime) {
            Utility.RunNextFrame(() => this.gameObject.SetActive(false)).Forget();
        }

        swingTimer += Time.deltaTime;       // unscaled by timeScale in case of HitStop
        float t = Mathf.Clamp01(swingTimer / attackController.Config.SwingTime);

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
