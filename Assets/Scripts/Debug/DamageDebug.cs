using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DamageDebug : MonoBehaviour, IDamageable {
    [Header("Health")]
    [SerializeField] float health;
    [SerializeField] Slider healthBar;

    [Header("Debug Events")]
    [SerializeField] private UnityEvent takeDamage = new();


    void Start() {
        healthBar.maxValue = health;
        healthBar.value = healthBar.maxValue;
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.T)) {
            takeDamage.Invoke();

        }

        if (health <= 0) {
            StartCoroutine(HealDummy());
        }
    }

    private IEnumerator HealDummy() {
        yield return new WaitForSeconds(1);

        healthBar.value = healthBar.maxValue;
        health = healthBar.maxValue;
        Debug.Log("Player Killed Debug Dummy");
    }

    #region  ========================= Damage Interface =========================
    public float BombRegenAmount { get; } = 0f;

    public float GetHealth() {
        return health;
    }

    public void TakeDamage(float value) {
        Debug.Log($"Taken {value} damage");
        health -= value;
        healthBar.value = health;
    }

    public void Kill() {
        Debug.Log("Enemy Oneshotted - due to speed");
        healthBar.value = 0;
    }

    public void Heal(float value) { }
    #endregion  ========================= Damage Interface =========================
}
