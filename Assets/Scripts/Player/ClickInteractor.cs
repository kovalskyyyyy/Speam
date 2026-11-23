using DialogueSystem.Runtime;
using Entities;
using Player;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class ClickInteractor : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Walkable agent;
    [SerializeField] float maxRayDistance = 100f;

    Interactable hovered;   // NPC под курсором
    Interactable target;    // NPC, к которому идём после клика

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (agent == null) agent = GetComponent<Walkable>();
    }

    void Update()
    {
        UpdateHover();

        if (Input.GetMouseButtonDown(0))
        {
            if (hovered != null)
            {
                target = hovered;
            }
            else if (!IsPointerOverUI())
            {
                // Кликнули в другое место, значит идём туда (это делает ваш скрипт движения), а цель отменяем
                target = null;
            }
        }

        UpdateApproach();
    }

    void UpdateHover()
    {
        Interactable found = null;

        if (!IsPointerOverUI())
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, maxRayDistance))
                found = hit.collider.GetComponentInParent<Interactable>();
        }

        if (found == hovered) return;

        if (hovered != null) hovered.SetHighlighted(false);
        hovered = found;
        if (hovered != null) hovered.SetHighlighted(true);
    }

    void UpdateApproach()
    {
        if (target == null) return;

        Vector3 delta = target.transform.position - transform.position;
        delta.y = 0f;

        if (delta.magnitude <= target.InteractDistance)
        {
            var t = target;
            target = null;
            t.Interact();
        }
        else
        {
            agent.GoToPoint(target.transform.position);
        }
    }

    static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}