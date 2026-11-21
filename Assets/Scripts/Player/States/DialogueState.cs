using StateMachine;
using UnityEngine;
namespace Player.States
{
    public class DialogueState : PlayerState
    {
        public DialogueState(StateMachine<PlayerState> machine, PlayerContext data)
            : base(machine, data) { }
        public override void Enter() 
        {
           
        }

        public override void Exit() 
        {
            
        }

        public override void StateUpdate()  { }
        
        public override void FixedStateUpdate()
        {
            // physics
        }
    }
}
