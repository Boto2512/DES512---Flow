using UnityEngine;
using UnityEngine.VFX;

public class movingObjectScript : MonoBehaviour
{
    //private Vector3 playerTargetLocation => Globals.PLAYER.Target.position;
    [SerializeField] private Transform hitTransform;
    [SerializeField] private float projectileSpeed = 4;
    [SerializeField] private float projectileCooldown;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float damageToPlayer = 10;
    [SerializeField] VisualEffect VFXProjectile;

    [ExecuteInEditMode]
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
       // VFXProjectile = GetComponent<VisualEffect>();


    }

    private void Awake()
    {
        VFXProjectile = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        //VFXProjectile.SetVector3("playerLocation", playerTargetLocation);
        updateProjectileVector();
    }

    [ContextMenu("Update Projectile Vector")]
    public void updateProjectileVector()
    {
        VFXProjectile.SetVector3("playerLocation", hitTransform.localPosition);
    }
}
