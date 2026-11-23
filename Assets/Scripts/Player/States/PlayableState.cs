using StateMachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Player.States
{
    public class PlayableState : PlayerState
    {
        public PlayableState(StateMachine<PlayerState> machine, PlayerContext data)
            : base(machine, data) { }

        private bool m_wasMouseCancelled = true;

        private Interactable m_hovered;   // объект под курсором
        private Interactable m_target;    // объект, к которому идём после клика

        public override void Enter()
        {
            m_data.walkControl.animationView = m_data.animationView; // I don't like this referencing
            m_data.inputReader.onPoint += OnPoint;
        }

        public override void Exit()
        {
            m_data.inputReader.onPoint -= OnPoint;

            // Выходим из состояния (например, начался диалог): гасим подсветку и забываем цель
            SetHovered(null);
            m_target = null;
            m_wasMouseCancelled = true;
        }

        public override void StateUpdate()
        {
            UpdateHover();
        }

        public override void FixedStateUpdate()
        {
            if (m_target != null)
            {
                UpdateApproach();
                return;
            }

            if (!m_wasMouseCancelled)
                GoToMousePos(Mouse.current.position.ReadValue());
        }

        private void OnPoint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (IsPointerOverUI()) return;

                if (m_hovered != null)
                {
                    // Путь считаем один раз. Если дойти нельзя, цель не ставим.
                    m_wasMouseCancelled = true;
                    m_target = m_data.walkControl.GoToPoint(m_hovered.transform.position)
                        ? m_hovered
                        : null;
                }
                else
                {
                    // Клик по земле: отменяем цель, дальше всё как раньше
                    m_target = null;
                    m_wasMouseCancelled = false;
                }
            }
            else if (context.canceled)
            {
                m_wasMouseCancelled = true;
            }
        }

        private void UpdateHover()
        {
            if (Mouse.current == null) return;

            Interactable found = null;

            if (!IsPointerOverUI())
            {
                Ray ray = m_data.camera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit))
                    found = hit.collider.GetComponentInParent<Interactable>();
            }

            SetHovered(found);
        }

        private void SetHovered(Interactable value)
        {
            if (value == m_hovered) return;

            if (m_hovered != null) m_hovered.SetHighlighted(false);
            m_hovered = value;
            if (m_hovered != null) m_hovered.SetHighlighted(true);
        }

        private void UpdateApproach()
        {
            if (m_target == null) return; // объект могли уничтожить

            Vector3 delta = m_target.transform.position - m_data.body.position;
            delta.y = 0f;

            if (delta.magnitude <= m_target.InteractDistance)
            {
                var t = m_target;
                m_target = null;             // сбрасываем до вызова, чтобы не сработало дважды
                m_data.walkControl.Stop();   // плавно тормозим
                t.Interact();                // может сменить состояние (диалог), это нормально
                return;
            }

            // Персонаж остановился, но до нужной дистанции не дошёл: отменяем цель
            if (!m_data.walkControl.IsMoving)
                m_target = null;
        }

        private void GoToMousePos(Vector2 mousePos)
        {
            Ray ray = m_data.camera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
                m_data.walkControl.GoToPoint(hit.point);
        }

        private static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
