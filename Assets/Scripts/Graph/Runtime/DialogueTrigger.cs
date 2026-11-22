using Saves;
using UnityEngine;

namespace DialogueSystem.Runtime
{
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
            var asset = (npcSaveable != null && npcSaveable.currentDialogue != null)
                ? npcSaveable.currentDialogue
                : dialogue;

            if (asset == null)
            {
                Debug.LogWarning("DialogueTrigger: диалог не назначен.", this);
                return;
            }

            DialogueRunner.Instance.StartDialogue(asset);
        }
    }
}