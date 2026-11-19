// IState.cs
namespace StateMachine
{
    public interface IState
    {
        void Enter(); 
        void Exit();
        void StateUpdate();
        void FixedStateUpdate();
    }
}