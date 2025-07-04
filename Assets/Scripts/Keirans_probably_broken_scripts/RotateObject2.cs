using UnityEngine;

public class RotateObject2 : MonoBehaviour
{
    public Vector3 rotation;

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(rotation * 1 * Time.deltaTime); 
    }
}
