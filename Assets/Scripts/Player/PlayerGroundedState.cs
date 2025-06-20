using System;

[Flags]
public enum PlayerGroundedState {
    None = 0b000,
    OnGround = 0b100,
    OnSlope = 0b100,
    InAir = 0b100
}
