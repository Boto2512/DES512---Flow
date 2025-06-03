using UnityEngine;
using StateMachine;
using System.Collections.Generic;

public enum EnemyAIState {
    Idle,
    Pursue,
    Attack,
    Reposition
}

public class EnemyAIStateMachine : StateMachine<EnemyAIState> {

    public EnemyAIStateMachine(List<Transition<EnemyAIState>> transitions) : base(EnemyAIState.Idle, transitions) { }

    public EnemyAIStateMachine(EnemyAIState initialState, List<Transition<EnemyAIState>> transitions) : base(initialState, transitions) {

    }

    public override void Update() {
        base.Update();
    }
}
