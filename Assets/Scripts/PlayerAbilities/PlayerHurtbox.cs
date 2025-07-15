using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerHurtbox : MonoBehaviour {

    [Header("Swing")]
    [SerializeField] private float startingAngle;
    [SerializeField] private float endingAngle;
    [SerializeField] private float swingTime = 0.03f;
    private float swingTimer = 0f;
    private Quaternion startingRotation;
    private Quaternion endingRotation;


    [SerializeField] private GameObject player;
    [SerializeField] private PlayerAttackController attackController;
    [SerializeField] private Transform sweepOrigin;
    [SerializeField] private Transform cameraRotation;
    private Rigidbody rb;

    private void Awake() {
        rb = this.GetComponent<Rigidbody>();


    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    private void OnEnable() {
        startingRotation = Quaternion.Euler(0f, startingAngle, 0f);
        endingRotation = Quaternion.Euler(0f, endingAngle, 0f);

        rb.rotation = startingRotation;
        rb.position = sweepOrigin.position;
        swingTimer = 0f;
    }

    // Update is called once per frame
    void Update() {
        if (swingTimer > swingTime) {
            Utility.RunNextFrame(() => this.gameObject.SetActive(false)).Forget();
        }

        swingTimer += Time.deltaTime;
        float t = Mathf.Clamp01(swingTimer / swingTime);

        rb.MoveRotation(cameraRotation.rotation * Quaternion.Slerp(startingRotation, endingRotation, t));
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
