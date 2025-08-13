using System;
using System.Collections.Generic;
using StateMachine;

[Flags]
public enum EnemyAIState {
    Idle = 0b0,
    Pursue = 0b1,
    Attack = 0b10,
    Reposition = 0b100,
    FanOut = 0b1000
}

public class EnemyAIStateMachine : StateMachine<EnemyAIState> {

    public bool MovementState => (CurrentState & (EnemyAIState.Pursue | EnemyAIState.Reposition | EnemyAIState.FanOut)) != 0;

    public EnemyAIStateMachine(List<Transition<EnemyAIState>> transitions) : base(EnemyAIState.Idle, transitions) { }

    public EnemyAIStateMachine(EnemyAIState initialState, List<Transition<EnemyAIState>> transitions) : base(initialState, transitions) {

    }

    public override void Update() {
        base.Update();
    }
}
