using UnityEngine;

public class ProjectileHomingOnPlayer : MonoBehaviour
{
    [SerializeField] float upwardSpeed = 5;
    [SerializeField] float homingSpeed = 8;
    [SerializeField] float homingDelay = 0.5f;
    [SerializeField] float totalDistance = 20;

    [SerializeField] Vector3 startingPos;
    [SerializeField] Vector3 endingPos;
    private bool isMoving = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        startingPos = transform.position;
        endingPos = startingPos + Vector3.up * totalDistance;

        //this.Invoke("literally anithing", startHoming, homingDelay);
        //Invoke(nameof(startHoming), homingDelay);
        
    }

    void startHoming()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, endingPos, upwardSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, endingPos) < 0.01f)
            {
                isMoving = false;
            }
        }


        
    }
}
