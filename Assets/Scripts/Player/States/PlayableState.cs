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
            m_data.walkControl.animationView = m_data.animationView; // I don't like this referencing
            m_data.inputReader.onPoint += UpdateMouseCancellation;

            Debug.Log(m_data.body);
        }

        public override void Exit() 
        {
            m_data.inputReader.onPoint -= UpdateMouseCancellation;
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
            //
            
            if (!m_wasMouseCancelled && !m_startedOverUI)
            {
                if (!m_data.inputReader.IsPointerOverUI())
                    GoToMousePos(m_data.inputReader.MousePosition);
            }
        }

        private bool m_wasMouseCancelled = true;
        private bool m_startedOverUI = false;

        private void UpdateMouseCancellation(InputAction.CallbackContext context) {
            if (context.performed)
            {
                if (m_data.inputReader.IsPointerOverUI())
                {
                    m_startedOverUI = true;
                    m_wasMouseCancelled = true;
                    return;
                }

                m_startedOverUI = false;
                m_wasMouseCancelled = false;
            }
            else if (context.canceled)
            {
                m_wasMouseCancelled = true;
                m_startedOverUI = false;
            }
        }
        
        private void GoToMousePos(Vector2 mousePos)
        {
            Ray ray = m_data.camera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("[PlayableState] Got the ray hit on " + hit.point);
                m_data.walkControl.GoToPoint(hit.point);
            }
        }
    }
}
