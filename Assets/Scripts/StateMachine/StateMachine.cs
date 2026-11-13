// StateMachine.cs

using System;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class StateMachine<TState> where TState : class, IState
    {
        public TState currentState => m_currentState;

        private readonly Dictionary<Type, TState> m_states = new();
        private TState m_currentState;

        public void AddState(TState state)
        {
            m_states[state.GetType()] = state;
        }

        public void ChangeState<T>() where T : TState
        {
            if (!m_states.TryGetValue(typeof(T), out TState state))
            {
                Debug.LogError($"State {typeof(T).Name} is not registered");
                return;
            }

            m_currentState?.Exit();
            m_currentState = state;
            m_currentState.Enter();
        }

        public void StateUpdate()
        {
            m_currentState?.StateUpdate();
        }
        public void FixedStateUpdate()
        {
            m_currentState?.FixedStateUpdate();
        }
    }
}