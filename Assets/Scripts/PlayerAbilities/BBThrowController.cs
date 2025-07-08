using AYellowpaper;
using UnityEngine;

public class BBThrowController : MonoBehaviour {

    [SerializeField] private BBThrowConfig Config;

    [Header("Throw Position & Momentum")]
    [SerializeField] private InterfaceReference<IMomentumModifiable> momentousEntity;
    [SerializeField] private Transform throwPosition;
    [SerializeField] private Transform throwOrientation;

    [Header("Animation")]
    [SerializeField] private Animator handAnimator;
    [SerializeField] private Animator cameraShakeAnimator;

    private GameObject bombInstance;
    private bool toggle = false;            // false = can throw/cannot blow, true = cannot throw/can blow

    public void ThrowBomb() {
        if (toggle)
            return;

        handAnimator.SetTrigger("hasBombed");
        cameraShakeAnimator.SetTrigger("hasBombed");

        bombInstance = Instantiate(Config.BounceBomb, throwPosition.position, throwOrientation.rotation);
        bombInstance.GetComponent<Rigidbody>().AddForce(momentousEntity.Value.GetMomentum() + throwOrientation.forward * Config.ThrowPower, ForceMode.VelocityChange);

        this.InvokeExclusive("toggle blow status", () => toggle = true, Time.fixedDeltaTime);       // one physics tick later
    }

    public void TriggerDetonation() {
        if (!toggle)
            return;

        handAnimator.SetTrigger("hasDetonate");
        cameraShakeAnimator.SetTrigger("hasDetonate");

        this.InvokeExclusive("detonate", DetonateBomb, Config.FuseTime);
    }

    private void DetonateBomb() {
        bombInstance.GetComponent<BounceBomb>().Activate();
        Destroy(bombInstance);

        toggle = false;

    }
}
