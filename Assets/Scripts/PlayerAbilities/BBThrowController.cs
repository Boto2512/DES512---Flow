using System.Runtime.CompilerServices;
using AYellowpaper;
using TMPro;
using UnityEngine;


public class BBThrowController : MonoBehaviour {

    [SerializeField] private BBThrowConfig Config;

    [Header("Position & Momentum")]
    [SerializeField] private InterfaceReference<IMomentumModifiable> momentousEntity;
    [SerializeField] private Transform throwPosition;
    [SerializeField] private Transform throwOrientation;

    [Header("Animation")]
    [SerializeField] private Animator handAnimator;
    [SerializeField] private Animator cameraShakeAnimator;
    private bool throwing = false;
    private bool detonating = false;

#if DEBUG
    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI debugChargeCountText;
#endif

    private GameObject bombInstance;
    private bool toggle = false;            // false = can throw/cannot blow, true = cannot throw/can blow

    // charges
    private float chargeCounter;

    private void Start() {
        chargeCounter = (float)Config.MaxCharges;
    }

    private void Update() {
        UpdateChargeCounter();

#if DEBUG
        debugChargeCountText.text = $"Charges: {ChargeCount()}, Regen {chargeCounter - ChargeCount():P1}";
#endif
    }

    #region Throwing & Detonating

    public void HoldThrow() {
        if (bombInstance != null)
            return;

        handAnimator.SetBool("isHoldingBomb", true);
    }

    public void ThrowBomb() {
        if (toggle || throwing)
            return;

        if (chargeCounter < 1)
            return;

        --chargeCounter;

        throwing = true;
        this.InvokeOverwrite("throwing bomb", () => throwing = false, Config.ThrowCooldown);

        handAnimator.SetBool("isHoldingBomb", false);
        //handAnimator.SetTrigger("hasBombed");
        cameraShakeAnimator.SetTrigger("hasBombed");

        bombInstance = Instantiate(Config.BounceBomb, throwPosition.position, throwOrientation.rotation);
        bombInstance.GetComponent<Rigidbody>().AddForce(momentousEntity.Value.GetMomentum() + throwOrientation.forward * Config.ThrowPower, ForceMode.VelocityChange);

        this.InvokeExclusive("toggle blow status", () => toggle = true, Time.fixedDeltaTime);       // one physics tick later
    }

    public void TriggerDetonation() {
        if (!toggle || detonating)
            return;

        detonating = true;
        this.InvokeOverwrite("detonating bomb", () => detonating = false, Config.DetonationCooldown);

        handAnimator.SetTrigger("hasDetonate");
        cameraShakeAnimator.SetTrigger("hasDetonate");

        this.InvokeExclusive("detonate", DetonateBomb, Config.FuseTime);
    }

    private void DetonateBomb() {
        if (bombInstance != null && bombInstance.TryGetComponent<BounceBomb>(out var bb)) {
            bb.Activate();
            Destroy(bombInstance);
        }

        toggle = false;
    }

    #endregion Throwing & Detonation

    #region Charges

    private void UpdateChargeCounter() {
        if (chargeCounter == Config.MaxCharges)
            return;

        if (chargeCounter > Config.MaxCharges) {
            chargeCounter = Config.MaxCharges;
            return;
        }

        chargeCounter += Time.deltaTime / Config.ChargeRegenTime;
    }

    public void AddChargeRegenAmount(float amount) {
        float currentChargeCap = Mathf.Floor(Config.CapChargeIncreaseByLevel ? chargeCounter + 1 : (float)Config.MaxCharges);

        chargeCounter = Mathf.Min(chargeCounter + amount, currentChargeCap);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ChargeCount() => (int)chargeCounter;

    public void BombBounceEnded() {
        AddChargeRegenAmount(1f);
    }

    #endregion Charges
}
