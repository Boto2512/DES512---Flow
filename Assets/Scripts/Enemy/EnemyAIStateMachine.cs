using UnityEngine;
using StateMachine;
using System.Collections.Generic;

public enum EnemyAIState {

}

public class EnemyAIStateMachine : StateMachine<EnemyAIState> {
    public EnemyAIStateMachine(EnemyAIState initialState, List<Transition<EnemyAIState>> transitions) : base(initialState, transitions) {

    }

    public override void Update() {
        base.Update();
    }
}
