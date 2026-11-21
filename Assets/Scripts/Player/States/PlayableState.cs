using StateMachine;
using Speam.Core;
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
        }

        public override void Exit() 
        {
            m_data.inputReader.onPoint -= UpdateMouseCancellation;
        }

        public override void StateUpdate()  { }
        
        public override void FixedStateUpdate()
        {
            // Пока открыто меню/пауза или идёт загрузка, игрок не двигается.
            if (GameManager.Instance != null && !GameManager.Instance.CanPlayerAct) return;

            if (!m_wasMouseCancelled)
            {
                var mouse = Mouse.current;
                if (mouse != null) GoToMousePos(mouse.position.ReadValue());
            }
        }

        private bool m_wasMouseCancelled = true;

        private void UpdateMouseCancellation(InputAction.CallbackContext context) {
            if (context.performed)
                m_wasMouseCancelled = false;
            else if (context.canceled)
                m_wasMouseCancelled = true;
        }
        
        private void GoToMousePos(Vector2 mousePos)
        {
            Ray ray = m_data.camera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                m_data.walkControl.GoToPoint(hit.point);
            }
        }
    }
}
