using UnityEngine;

[CreateAssetMenu(fileName = "PlayerVariablesConfig", menuName = "Scriptable Objects/PlayerVariablesConfig")]
public class PlayerVariablesConfig : ScriptableObject
{
    [Header("========== Health ==========")]
    public float defaultHealth;

    [Header("========== Movement ==========")]

    [Tooltip("Starting value for the acceleration")]
    public float defaultAccelertionSpeed;
    //[Tooltip("Time takes for acceleration to reach maxMovementSpeed")]
    //public float accelerationTime;
    //[Tooltip("Currently not in use")]
    public float deceleration;

    //public float startSpeed;
    [Tooltip("The max movementspeed for running")]
    public float defaultMaxMovementSpeed;
    [Tooltip("the amount the player's max speed increases when in air")]
    public float airSpeedIncrease;
    [Tooltip("the amount the player's max speed increases by when on a slope")]
    public float slopeSpeedImpact;
    [Tooltip("the max movementspeed for anything that boosts the player's movement")]
    public float boostedMaxMovementSpeed;
    [Space(10)]
    [Tooltip("Applies a force when there is no player input to slow the player")]
    public float counterForce;
    //public float groundDrag;
    //public float airDrag;
    [Tooltip("Controls how long the player stays at max velocity before decay starts")]
    public float maxVeloctiyDuration;
    [Tooltip("Controls the rate max speed & acceleration is reduced after being increased")]
    public float defaultVelocityDecayRate;
    [Tooltip("How often velocity is stored in seconds, used for the wall kick")]
    public float veloctiyStorageFrequency;

    [Space(10)]
    public float maxFallSpeed;
    [Range(0, 1), Tooltip("Controls how much control the player has when in the air (0 is none, 1 is full)")]
    public float airControlMultiplier;

    [Space(10)]
    [Header("Slope Movement")]
    [Tooltip("Maxium slope angle the player can go up")]
    public float maxSlopeAngle;
    [Tooltip("Minimum slope angle the player gets a speed boost from")]
    public float minSlopeAngle;
    
    [SerializeField,Tooltip("Force applied to keep player on slopes when going down them")] 
    public float downwardsForce;

    [Space(10)]
    [Header("Jump")]
    public float jumpForce;
    [Tooltip("applies a force once player releases jump so it reaches thye apex faster  ")]
    public float maxJumpMultiplier;
    [Tooltip("applies a a force when player falls so they fall quicker  ")]
    public float fallMultiplier;
    public float bombFallMultiplier;
    public float jumpCooldown;
    [Space(5)]
    [Tooltip("duration of coyote time")]
    public float coyoteTime = 0.2f;

    [Space(10)]
    [Header("Ground Check")]
    public float groundCheckRange;
    public LayerMask groundMask;

    [Space(10)]
    [Header("Wall Check & Kick")]
    public float wallKickRange;
    public float wallKickCooldown;


    [Space(10)]
    [Header("Attack")]
    public Vector3 attackScale;
    public LayerMask attackMask;
    public float attackRange;
    public float attackDamage;
    public float attackCooldown;

    [Space(10)]
    [Header("Speed Stages")]
    [Tooltip("the value that controls how fast the player has to be reach the 2nd speed stage")]
    public float firstBreakpoint;
    [Tooltip("the value that controls how fast the player has to be reach the 3rd speed stage")]
    public float secondBreakpoint;

    [Space(10)]
    [Header("Bounce Bomb")]
    public GameObject bounceBomb;
    [Min(0f)] public float bombThrowPower = 1f;
    [Min(0f)] public float fuseTime = 0.15f;

}
