using System;

[Flags]
public enum PlayerGroundedState {
    None = 0b000,
    OnGround = 0b001,
    OnSlope = 0b010,
    InAir = 0b100
}
