using UnityEngine;

public class TimeScale : MonoBehaviour
{
    [SerializeField] private float timeScale;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Time.timeScale = timeScale;
        }
    }
}
