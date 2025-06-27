using UnityEngine;
using DG.Tweening;

public class HealthDrop : MonoBehaviour
{
    [Tooltip("The amount that the player heals when collected")]
    [SerializeField] private float healAmount;

    [Header("Floating")]
    [SerializeField] private float floatDuration;
    [SerializeField] private float floatHeight;
    private Tween floatTween;
    [Space(10)]
    [Header("Rotation")]
    [SerializeField] private float rotationDuration;
    private Tween rotateTween;

    private Transform parent;
    private void Start()
    {
        DOTween.Init();
        parent = gameObject.transform.parent;

        Vector3 floatVector = new Vector3(0,floatHeight,0);

        floatTween = transform.DOLocalMove(floatVector, floatDuration);
        floatTween.SetLoops(-1, LoopType.Yoyo);
        rotateTween = transform.DOLocalRotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.FastBeyond360);
        rotateTween.SetLoops(-1, LoopType.Yoyo);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.CompareTag("Player"))
        { 

            other.GetComponentInParent<IDamageable>().Heal(healAmount);
            Destroy(parent.gameObject);
        }
    }
}