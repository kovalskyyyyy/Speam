using StateMachine;

namespace Player.States
{
    public abstract class PlayerState : IState
    {
        protected readonly StateMachine<PlayerState> m_machine;
        protected readonly PlayerContext m_data;

        protected PlayerState(StateMachine<PlayerState> machine, PlayerContext data)
        {
            m_machine = machine;
            m_data = data;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void StateUpdate() { }
        public virtual void FixedStateUpdate() { }
    }
}