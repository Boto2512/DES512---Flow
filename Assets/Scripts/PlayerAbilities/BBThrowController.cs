using System;
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
    private bool thrownThisInput = false;

    [Header("UI")]
    [SerializeField] private Material[] chargeMaterials;


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

        // 1f is the hardcoded time until the hand goes down
        if (chargeCounter < 1) {
            this.InvokeExclusive("not holding bomb", () => handAnimator.SetBool("isHoldingBomb", false), 0.8f);
        }
        else {
            this.InvokeCancel("not holding bomb");
        }
    }

    public void ThrowBomb() {
        if (thrownThisInput) {
            thrownThisInput = false;
            return;
        }

        if (toggle || throwing)
            return;

        if (chargeCounter < 1)
            return;

        --chargeCounter;

        this.InvokeCancel("not holding bomb");
        handAnimator.SetBool("isHoldingBomb", false);

        throwing = true;
        thrownThisInput = true;
        this.InvokeOverwrite("throwing bomb", () => throwing = false, Config.ThrowCooldown);

        //handAnimator.SetTrigger("hasBombed");
        cameraShakeAnimator.SetTrigger("hasBombed");
        AudioManager.instance?.Play("BombThrow");
        bombInstance = Instantiate(Config.BounceBomb, throwPosition.position, Quaternion.identity);
        bombInstance.GetComponent<Rigidbody>().AddForce(momentousEntity.Value.GetMomentum() + throwOrientation.forward * Config.ThrowPower, ForceMode.VelocityChange);


        Utility.RunNextFrame(() => toggle = true).Forget();     // one physics tick later
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

        ChargeUI();
    }

    public void AddChargeRegenAmount(float amount) {
        float currentChargeCap = Mathf.Floor(Config.CapChargeIncreaseByLevel ? chargeCounter + 1 : (float)Config.MaxCharges);

        chargeCounter = Mathf.Min(chargeCounter + amount, currentChargeCap);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ChargeRegenProgress() => chargeCounter - (float)ChargeCount();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ChargeCount() => (int)chargeCounter;

    public void BombBounceEnded() {
        AddChargeRegenAmount(1f);
    }


    private void ChargeUI() {

        if(chargeCounter == Config.MaxCharges) { return; }
        float currentCharge = (chargeCounter % 1);
        int currentAmount = Mathf.FloorToInt(chargeCounter);
        int index = 0;

        foreach (Material mat in chargeMaterials) {
            if (index == currentAmount) { chargeMaterials[index].SetFloat("_LiquidAmount", currentCharge); }
            else if (index < currentAmount) { chargeMaterials[index].SetFloat("_LiquidAmount", 1); }
            else if (index > currentAmount) { chargeMaterials[index].SetFloat("_LiquidAmount", 0); }

            index++;
        }
    }
    #endregion Charges
}
