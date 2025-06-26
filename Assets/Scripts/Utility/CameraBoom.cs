using UnityEngine;

public class CameraBoom : MonoBehaviour
{
    [SerializeField, Min(0f)] private float boomLength = 10f;
    [SerializeField, Min(0f)] private float bufferLength = 0.2f;
    [Tooltip("Leaving this blank ('Nothing') means Globals.OBSTACLE_MASK is used as default.")]
    [SerializeField] private LayerMask obstacles;

    private bool lengthChanged = false;
    private float actualLength;

    private Transform parent => this.transform.parent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (obstacles == 0)
        {
            obstacles = Globals.OBSTACLE_MASK;
        }

        actualLength = boomLength;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateActualLength();
        UpdateTransform();
    }

    private void OnValidate()
    {
        this.transform.LookAt(parent);
        UpdateActualLength();
        UpdateTransform();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(parent.position, this.transform.position);
    }

    private void UpdateActualLength()
    {
        if (parent == null)
            return;

        Vector3 directionFromParent = (this.transform.position - parent.position).normalized;

        float tempActualLength;
        if (Physics.Raycast(parent.position, directionFromParent, out var hitInfo, boomLength, obstacles, QueryTriggerInteraction.Collide))
        {
            tempActualLength = hitInfo.distance;
        }
        else
        {
            tempActualLength = boomLength;
        }

        lengthChanged = tempActualLength != actualLength;
        actualLength = tempActualLength;
    }

    private void UpdateTransform()
    {
        if (parent == null)
            return;

        transform.LookAt(parent);

        if (!lengthChanged)
            return;

        Vector3 directionFromParent = (this.transform.position - parent.position).normalized;

        this.transform.position = parent.position + directionFromParent * (actualLength - bufferLength);
    }
}
