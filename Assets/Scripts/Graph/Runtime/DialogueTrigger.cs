using Saves;
using UnityEngine;

namespace DialogueSystem.Runtime
{
    /// <summary>
    /// Вешается на NPC. Запускает диалог, когда игрок кликнул по NPC и дошёл до него.
    /// </summary>
    public class DialogueTrigger : Interactable
    {
        [SerializeField] DialogueAsset dialogue;

        NpcSaveable npcSaveable;

        protected override void Awake()
        {
            base.Awake();
            npcSaveable = GetComponent<NpcSaveable>();
        }

        public override void Interact()
        {
            // Приоритет: диалог из сейва, затем из инспектора
            var asset = (npcSaveable != null && npcSaveable.currentDialogue != null)
                ? npcSaveable.currentDialogue
                : dialogue;

            if (asset == null)
            {
                Debug.LogWarning("DialogueTrigger: диалог не назначен.", this);
                return;
            }

            if (DialogueRunner.Instance == null)
            {
                Debug.LogWarning("DialogueTrigger: DialogueRunner не найден на сцене.", this);
                return;
            }

            DialogueRunner.Instance.StartDialogue(asset);
        }
    }
}