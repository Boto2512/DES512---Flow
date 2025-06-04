using System;
using System.Collections.Generic;

namespace StateMachine {
    public class StateMachine<TEnum> where TEnum : Enum {
        //private IEnumerable<TEnum> states => transitions.Keys;
        private readonly Dictionary<TEnum, List<Transition<TEnum>>> transitions = new();

        public TEnum CurrentState { get; protected set; }

        /// <summary>
        /// Default parameterless constructor. Avoid using.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public StateMachine() {
            Array enums = Enum.GetValues(typeof(TEnum));

            if (enums.Length == 0) {
                throw new Exception("Generic enum type has no values.");
            }

            foreach (TEnum value in enums) {
                transitions.Add(value, new());
            }

            CurrentState = (TEnum)enums.GetValue(0);
        }

        /// <summary>
        /// Creates a finite state machine based on the enum states and transitions passed in.
        /// TEnum MUST have consecutive positive int values from 0-n. 
        /// </summary>
        /// <param name="initialState">State the finite state machine starts in</param>
        /// <param name="transitions">How one state can move into another state</param>
        public StateMachine(TEnum initialState, List<Transition<TEnum>> transitions) {
            CurrentState = initialState;

            // initialises lists of transitions in dictionary
            foreach (TEnum key in Enum.GetValues(typeof(TEnum))) {
                this.transitions.Add(key, new());
            }

            // adds transitions to relevant dictionary entries
            foreach (var transition in transitions) {
                this.transitions[transition.From].Add(transition);
            }
        }

        public virtual void Update() {
            TransitionCheck();
        }

        /// <summary>
        /// Checks all transitions from the CurrentState and applies the first one that's valid
        /// </summary>
        protected void TransitionCheck() {
            if (!transitions.ContainsKey(CurrentState))
                return;

            foreach (var transition in transitions[CurrentState]) {
                if (transition.Evaluate) {
                    CurrentState = transition.To;
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