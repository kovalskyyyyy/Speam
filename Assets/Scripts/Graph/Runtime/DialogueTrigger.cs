using Saves;
using UnityEditor.Animations;
using UnityEngine;

namespace DialogueSystem.Runtime
{
    /// <summary>
    /// Вешается на NPC. Запускает диалог, когда игрок кликнул по NPC и дошёл до него.
    /// </summary>
    public class DialogueTrigger : Interactable
    {
        [SerializeField] DialogueAsset dialogue;
        [SerializeField] private Animator animatorController;

        NpcSaveable npcSaveable;

        protected override void Awake()
        {
            base.Awake();
            npcSaveable = GetComponent<NpcSaveable>();
        }
        void Start()
        {
            // Subscribe to dialogue events
            if (DialogueRunner.Instance != null)
            {
                DialogueRunner.Instance.Ended += OnDialogueEnded;
            }
        }

        void OnDisable()
        {
            if (DialogueRunner.Instance != null)
            {
                DialogueRunner.Instance.Ended -= OnDialogueEnded;
            }
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
            OnDialogueStarted();
            DialogueRunner.Instance.StartDialogue(asset);
        }
        private void OnDialogueStarted()
        {
            animatorController.SetBool("IsTalking", true);
        }

        private void OnDialogueEnded()
        {
            animatorController.SetBool("IsTalking", false);
        }  
    }
}