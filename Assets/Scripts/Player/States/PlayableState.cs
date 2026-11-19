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
            m_data.walkControl.animationView = m_data.animationView; // I don't like this referencing
            m_data.inputReader.onPoint += GoToMousePos;

            Debug.Log(m_data.body);
        }

        public override void Exit() 
        {
            m_data.inputReader.onPoint -= GoToMousePos;
        }

        public override void StateUpdate()  { }
        
        public override void FixedStateUpdate()
        {
            // Vector3 velocity = m_data.walkControl.velocity;
            // velocity.y = 0f;
            // Vector3 velocityNormilized = velocity/3.6f;
            // float speedNormalized = Vector3.Dot(velocityNormilized, m_data.body.transform.forward);
            //
            // m_data.animationView.SetSpeed(speedNormalized);
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
