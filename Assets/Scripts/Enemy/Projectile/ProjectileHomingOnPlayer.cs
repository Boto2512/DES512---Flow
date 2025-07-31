using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileHomingOnPlayer : MonoBehaviour
{
    private Rigidbody projectileRigidBody;


    [Header("Upwards")]
    [SerializeField] float upwardSpeed;
    [Tooltip("Projectile would go upward to a random destination within the range.")]
    [SerializeField] float upwardHorizontalRange;
    [Tooltip("Projectile would go upward to a random destination within the range.")]
    [SerializeField] float upwardVerticalRange;
    [SerializeField] float upwardHeight;
    [SerializeField] float rotateSpeed;

    private Vector3 heightTarget;
    private bool hasReachedHeight = false;

    [Header("Homing")]
    [SerializeField] float homingSpeed;
    [SerializeField] float homingDelay;
    [SerializeField] float homingDuration;
    private float homingTime;
    private Transform player;


    [Header("Damage")]
    [SerializeField] private float damage;


    [Header("Model")]
    [SerializeField] private Transform projectileModel;

    private Vector3 lockDir;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        projectileRigidBody = GetComponent<Rigidbody>();

        heightTarget =new Vector3(transform.position.x, transform.position.y + (upwardHeight+ upwardVerticalRange*Random.Range(-1f,1f)), transform.position.z)+new Vector3(1,0,1)* upwardHorizontalRange * Random.Range(-1f, 1f);
        //this.Invoke("literally anithing", startHoming, homingDelay);
        //Invoke(nameof(startHoming), homingDelay);

        player = Globals.PLAYER.transform;


    }
    // Update is called once per frame
    void Update() {
        if (!hasReachedHeight) {
            MoveUpwards();
        }
        else {
            //this.Invoke("Homing", Homing, homingDelay);
            Homing();
            if (homingTime >= homingDuration) { 
                Destroy(gameObject);
            }
        }
    }

    private void MoveUpwards() {
        Vector3 newPosition = Vector3.MoveTowards(transform.position, heightTarget, Time.deltaTime * upwardSpeed);
        projectileRigidBody.MovePosition(newPosition);

        if (Vector3.Distance(newPosition, heightTarget) <= 0.5) { 
            hasReachedHeight = true;
        }
    }
    private void Homing() {
        //homingDelay = 0;
        homingTime += Time.deltaTime;
        if (homingTime<homingDelay)
        {
            lockDir= player.position - transform.position;
        }

        Vector3 newPosition = transform.position + lockDir.normalized* Time.deltaTime * homingSpeed; //Vector3.MoveTowards(, lockPosition, Time.deltaTime * homingSpeed);
        projectileModel.up = Vector3.Lerp(projectileModel.up, lockDir, rotateSpeed);
        projectileRigidBody.MovePosition(newPosition);
        AudioManager.instance?.Play("Launch", transform.position);
    }

    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag("Player")) {
            //Damage Player
            AudioManager.instance?.Play("Hurt");
            IDamageable damageable = other.attachedRigidbody.gameObject.GetComponent<IDamageable>();
            damageable.TakeDamage(damage);
            Destroy(gameObject);
            //Debug.Log("Hit");
        }else if (other.CompareTag("Enemy"))
        {
            return;
        }
        else { 
            //Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        if (homingDelay> homingDuration)
        {
            homingDelay = homingDuration;
        }

        if (upwardVerticalRange>upwardHeight)
        {
            upwardVerticalRange=upwardHeight;
        }
    }
}
