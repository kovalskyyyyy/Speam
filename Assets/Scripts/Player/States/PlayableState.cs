using StateMachine;
using UnityEngine;

namespace Player.States
{
    public class PlayableState : PlayerState
    {
        public PlayableState(StateMachine<PlayerState> machine, PlayerContext data)
            : base(machine, data) { }

        public override void Enter()
        {
            m_data.pointClickAction.action.Enable();
            m_data.pointClickAction.action.started += PointClickHandler;
        }

        public override void Exit() { }

        public override void StateUpdate()  
        {
        }
        public override void FixedStateUpdate()
        {
            // physics
        }
        
        private void PointClickHandler()
        {
            Ray ray = m_data.camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("[PlayableState] Got the ray hit on " + hit.point);
                m_data.walkControl.GoToPoint(hit.point);
            }
        }
    }
}
