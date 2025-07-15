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
        startingRotation = cameraRotation.rotation * Quaternion.Euler(0f, startingAngle, 0f);
        endingRotation = cameraRotation.rotation * Quaternion.Euler(0f, endingAngle, 0f);

        rb.rotation = startingRotation;
        rb.position = sweepOrigin.position;
        swingTimer = 0f;
    }

    // Update is called once per frame
    void Update() {
        if (swingTimer > swingTime) {
            this.gameObject.SetActive(false);
        }

        swingTimer += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(swingTimer / swingTime);

        rb.MoveRotation(Quaternion.Slerp(startingRotation, endingRotation, t));
        rb.MovePosition(sweepOrigin.position);
        Debug.Log($"move position {sweepOrigin.position}");
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.TryGetComponent<Rigidbody>(out var rb))
            return;

        Debug.Log(rb.gameObject.name);

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
