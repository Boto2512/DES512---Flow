using UnityEngine;

public class ProjectileHomingOnPlayer : MonoBehaviour
{
    [SerializeField] float upwardSpeed = 5;
    [SerializeField] float homingSpeed = 8;
    [SerializeField] float homingDelay = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        

        Invoke(nameof(startHoming), homingDelay);
        
    }

    void startHoming()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
