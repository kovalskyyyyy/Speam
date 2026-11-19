using UnityEngine.InputSystem;
using UnityEngine;
using System;

namespace Player
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
    public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions
    {
        public event Action<InputAction.CallbackContext> onPoint;

        public void OnPoint(InputAction.CallbackContext context) 
        {
            // if (context.performed)
            //     onPoint?.Invoke(Mouse.current.position.ReadValue());

            onPoint?.Invoke(context);
        }

        public void OnMove    (InputAction.CallbackContext context) {}
        public void OnLook    (InputAction.CallbackContext context) {}
        public void OnAttack  (InputAction.CallbackContext context) {}
        public void OnInteract(InputAction.CallbackContext context) {}
        public void OnCrouch  (InputAction.CallbackContext context) {}
        public void OnJump    (InputAction.CallbackContext context) {}
        public void OnPrevious(InputAction.CallbackContext context) {}
        public void OnNext    (InputAction.CallbackContext context) {}
        public void OnSprint  (InputAction.CallbackContext context) {}

        private InputSystem_Actions m_inputActions;

        private void OnEnable() 
        {
            if (m_inputActions == null) 
            {
                m_inputActions = new InputSystem_Actions();
                m_inputActions.Player.SetCallbacks(this);
            }
            m_inputActions?.Player.Enable();
        }

        private void OnDisable()
        {
            m_inputActions?.Player.Disable();
        }
    }

} 
