using UnityEngine;
using UnityEngine.Serialization;

public abstract class Interactable : MonoBehaviour
{
    [FormerlySerializedAs("triggerDistance")]
    [SerializeField] float interactDistance = 2f;

    public float InteractDistance => interactDistance;

    Outline outline;

    protected virtual void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }

    public void SetHighlighted(bool value)
    {
        if (outline != null) outline.enabled = value;
    }

    /// <summary>Вызывается, когда игрок кликнул по объекту и дошёл до него.</summary>
    public abstract void Interact();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}