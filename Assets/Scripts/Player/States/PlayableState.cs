using StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.States
{
    public class PlayableState : PlayerState
    {
        public PlayableState(StateMachine<PlayerState> machine, PlayerContext data)
            : base(machine, data) { }

        public override void Enter() 
        {
            m_data.inputReader.onPoint += GoToMousePos;
        }

        public override void Exit() 
        {
            m_data.inputReader.onPoint -= GoToMousePos;
        }

        public override void StateUpdate()  { }
        
        public override void FixedStateUpdate()
        {
            // physics
        }
        
        private void GoToMousePos(Vector2 mousePos)
        {
            Debug.Log($"camera = {m_data.camera}");
            Ray ray = m_data.camera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("[PlayableState] Got the ray hit on " + hit.point);
                m_data.walkControl.GoToPoint(hit.point);
            }
        }
    }
}
