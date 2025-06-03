using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor.PackageManager;

public class DamageDebug : MonoBehaviour, IDamageable
{
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

        if (health <=0 ) {
            healthBar.value = healthBar.maxValue;
            health = healthBar.maxValue;
            Debug.Log("Player Killed Debug Dummy");
        }
    }


    #region  ========================= Damage Interface =========================
    public float GetHealth() {
        return health;
    }

    public void TakeDamage(float value) {
        Debug.Log($"Taken {value} damage");
        health -= value;
        healthBar.value = health;
    }

    public void Kill()
    {
        Debug.Log("Enemy Oneshotted - due to speed");
    }
    #endregion  ========================= Damage Interface =========================
}
