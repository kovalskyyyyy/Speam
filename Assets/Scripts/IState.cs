// IState.cs
public interface IState
{
    void Enter(); 
    void Exit();
    void StateUpdate();
    void FixedStateUpdate();
}