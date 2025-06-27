using UnityEngine;

[System.Serializable]
public class SpeedStageThreshold {
    [Tooltip("If speed is greater or equal to this value, apply the corresponding damage multiplier")]
    [SerializeField] private float speedThreshold;
    [SerializeField] private float damageMultiplier;

    public float SpeedThreshold { get { return speedThreshold; } }
    public float DamageMultiplier { get { return damageMultiplier; } }
}
