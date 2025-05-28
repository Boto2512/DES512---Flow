using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StateMachine {
    public class StateMachine<TEnum> where TEnum : Enum {
        //private IEnumerable<TEnum> states => transitions.Keys;
        private Dictionary<TEnum, List<Transition<TEnum>>> transitions = new();

        public TEnum CurrentState { get; protected set; }

        /// <summary>
        /// Creates a finite state machine based on the enum states and transitions passed in.
        /// TEnum MUST have consecutive positive int values from 0-n. 
        /// </summary>
        /// <param name="initialState">State the finite state machine starts in</param>
        /// <param name="transitions"></param>
        public StateMachine(TEnum initialState, List<Transition<TEnum>> transitions) {
            CurrentState = initialState;

            foreach (var transition in transitions) {
                if (!this.transitions.ContainsKey(transition.From)) {
                    this.transitions[transition.From] = new List<Transition<TEnum>> { transition };
                }
                else {
                    this.transitions[transition.From].Add(transition);
                }
            }
        }

        public virtual void Update() {
            TransitionCheck();
        }

        protected void TransitionCheck() {
            foreach (var transition in transitions[CurrentState]) {
                if (transition.Evaluate) {
                    CurrentState = transition.From;
                    transition.Invoke();
                    break;
                }
            }
        }
    }

    public readonly struct Transition<TEnum> where TEnum : Enum {
        public TEnum From { get; }
        public TEnum To { get; }

        private readonly Func<bool> predicate;
        private readonly Action callback;

        public Transition(TEnum from, TEnum to, Func<bool> predicate, Action callback = null) {
            From = from;
            To = to;
            this.predicate = predicate;
            this.callback = callback;
        }

        public readonly bool Evaluate => predicate();
        public readonly void Invoke() => callback?.Invoke();
    }
}