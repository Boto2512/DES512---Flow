using UnityEngine;

[System.Serializable]
public class SpeedStageThreshold {
    [Tooltip("If speed is greater or equal to this value, apply the rest of the valid options")]
    [SerializeField] private float speedThreshold;
    [SerializeField] private float damageMultiplier;
    [SerializeField] private float fieldOfView;

    [Header("Speedlines VFX")]
    [SerializeField] private bool hasSpeedlines;
    [SerializeField] private Vector2 speedRangeOfLines;
    [SerializeField] private float speedlineSpawnRate;
    [SerializeField] private Color speedlineColor;


    public float SpeedThreshold => speedThreshold;
    public float DamageMultiplier => damageMultiplier;
    public float FieldOfView => fieldOfView;

    // speedlines
    public bool HasSpeedLines => hasSpeedlines;
    public Vector2 SpeedRangeOfLines => speedRangeOfLines;
    public float SpeedlineSpawnRate => speedlineSpawnRate;

    public Color SpeedlineColor => speedlineColor;
}
