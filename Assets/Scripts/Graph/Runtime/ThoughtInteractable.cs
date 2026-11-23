using System.Collections.Generic;
using UnityEngine;

public class ThoughtInteractable : Interactable
{
    [TextArea(1, 3)]
    [SerializeField] List<string> thoughts = new();

    [Tooltip("Не повторять подряд одну и ту же мысль")]
    [SerializeField] bool avoidImmediateRepeat = true;

    int lastIndex = -1;

    public override void Interact()
    {
        if (thoughts.Count == 0)
        {
            Debug.LogWarning("ThoughtInteractable: список мыслей пуст.", this);
            return;
        }

        if (ThoughtPresenter.Instance == null)
        {
            Debug.LogWarning("ThoughtInteractable: ThoughtPresenter не найден на сцене.", this);
            return;
        }

        int index = Random.Range(0, thoughts.Count);

        if (avoidImmediateRepeat && thoughts.Count > 1)
        {
            while (index == lastIndex)
                index = Random.Range(0, thoughts.Count);
        }

        lastIndex = index;
        ThoughtPresenter.Instance.Show(thoughts[index]);
    }
}