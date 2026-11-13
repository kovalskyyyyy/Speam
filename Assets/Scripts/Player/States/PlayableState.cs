using StateMachine;

namespace Player.States
{
    public class PlayableState : PlayerState
    {
        public PlayableState(StateMachine<PlayerState> machine, PlayerContext data)
            : base(machine, data) { }

        public override void Enter() { }

        public override void Exit() { }

        public override void StateUpdate()  
        {
            // ������� ��������� � ���������
        }
        public override void FixedStateUpdate()
        {
            // physics
        }
    }
}
