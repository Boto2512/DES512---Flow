using UnityEngine;

public class BBThrowController : MonoBehaviour {

    [SerializeField] private BBThrowConfig Config;

    [SerializeField] private IMomentumModifiable momentousEntity;
    [SerializeField] private Transform throwTransform;
    [SerializeField] private BounceBomb bounceBomb;
    [SerializeField, Min(0f)] private float throwPower;
    [SerializeField, Min(0f)] private float fuseTime;

    private GameObject bombInstance;

    public void ThrowBomb() {
        if (bombInstance != null)
            return;

        bombInstance = Instantiate(bounceBomb, throwTransform.position, throwTransform.rotation).gameObject;
        bombInstance.GetComponent<Rigidbody>().AddForce(momentousEntity.GetMomentum() + throwTransform.forward * throwPower, ForceMode.VelocityChange);
    }

    public void TriggerDetonation() {
        if (bombInstance == null)
            return;

        // animation stuff here

        this.InvokeExclusive("detonate", DetonateBomb, fuseTime);
    }

    private void DetonateBomb() {
        bombInstance.GetComponent<BounceBomb>().Activate();
        Destroy(bombInstance);
    }
}
