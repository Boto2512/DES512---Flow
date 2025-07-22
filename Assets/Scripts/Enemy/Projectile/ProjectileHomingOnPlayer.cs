using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileHomingOnPlayer : MonoBehaviour
{
    private Rigidbody projectileRigidBody;

    [Header("Upwards")]
    [SerializeField] float upwardSpeed;
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

    private Vector3 lockPosition;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        projectileRigidBody = GetComponent<Rigidbody>();

        heightTarget =new Vector3(transform.position.x, transform.position.y + upwardHeight, transform.position.z);
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
            lockPosition=player.position;
        }

        Vector3 newPosition = Vector3.MoveTowards(transform.position, lockPosition, Time.deltaTime * homingSpeed);
        Vector3 dir = lockPosition - transform.position;
        transform.up = Vector3.Lerp(transform.up, dir, rotateSpeed);
        if (dir.magnitude<0.01f)
        {
            Destroy(gameObject);
        }
        else
        {
            projectileRigidBody.MovePosition(newPosition);
        }
    }

    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag("Player")) { 
            //Damage Player
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
    }
}
