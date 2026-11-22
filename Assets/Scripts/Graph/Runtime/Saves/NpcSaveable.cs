using System.Collections.Generic;
using UnityEngine;
using DialogueSystem.Runtime;

namespace Saves
{
    /// <summary>
    /// Вешается на каждого НПС. Хранит его текущий диалог, умеет его запускать
    /// и умеет сохранять/восстанавливать своё состояние.
    /// </summary>
    public class NpcSaveable : MonoBehaviour
    {
        [Tooltip("Уникальный ID, например 'npc_blacksmith'. Не меняйте после релиза.")]
        public string id;

        [Tooltip("Заполняется только у префабов, которые спавнятся кодом.")]
        public string prefabId;

        [Tooltip("Текущий диалог этого НПС. Именно он сохраняется и запускается.")]
        public DialogueAsset currentDialogue;

        // ---- Реестр активных НПС (чтобы действия могли найти НПС по id) ----
        static readonly Dictionary<string, NpcSaveable> registry = new();

        public static NpcSaveable Find(string npcId)
        {
            if (registry.TryGetValue(npcId, out var npc) && npc != null) return npc;

            // запасной путь: НПС может быть выключен
            foreach (var n in FindObjectsByType<NpcSaveable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (n.id == npcId)
                    return n;
            return null;
        }

        void OnEnable()
        {
            if (!string.IsNullOrEmpty(id)) registry[id] = this;
        }

        void OnDisable()
        {
            if (!string.IsNullOrEmpty(id) && registry.TryGetValue(id, out var n) && n == this) registry.Remove(id);
        }

        // ---- Диалог ----
        public void SetDialogue(DialogueAsset dialogue) => currentDialogue = dialogue;

        /// <summary>Запустить текущий диалог. Вызывайте из вашей системы взаимодействия.</summary>
        public void StartDialogue()
        {
            if (DialogueRunner.Instance == null)
            {
                Debug.LogWarning("На сцене нет DialogueRunner", this);
                return;
            }

            DialogueRunner.Instance.StartDialogue(currentDialogue);
        }

        // ---- Сохранение ----
        public NpcState Capture() => new NpcState
        {
            id = id,
            prefabId = prefabId,
            position = transform.position,
            rotation = transform.rotation,
            dialogueId = currentDialogue != null ? currentDialogue.id : null
        };

        public void Restore(NpcState s, DialogueDatabase db)
        {
            if (s == null) return;

            transform.SetPositionAndRotation(s.position, s.rotation);
            currentDialogue = db != null ? db.Get(s.dialogueId) : null;

            // Если есть NavMeshAgent — позицию надо ставить через Warp:
            // if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent)) agent.Warp(s.position);
        }
    }
}
