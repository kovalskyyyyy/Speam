using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

namespace Player
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
    public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions
    {
        public event Action<InputAction.CallbackContext> onPoint;

        public Vector2 MousePosition => Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        private readonly List<RaycastResult> m_uiRaycastResults = new();
        private PointerEventData m_pointerEventData;

        public bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            if (EventSystem.current.IsPointerOverGameObject()) return true;

            if (Mouse.current == null) return false;
            m_pointerEventData ??= new PointerEventData(EventSystem.current);
            m_pointerEventData.position = MousePosition;
            m_uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(m_pointerEventData, m_uiRaycastResults);
            return m_uiRaycastResults.Count > 0;
        }

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
