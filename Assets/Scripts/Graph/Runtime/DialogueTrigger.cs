using Saves;
using UnityEngine;

namespace DialogueSystem.Runtime
{
    /// <summary>
    /// Вешается на NPC. Следит, подошёл ли игрок, и просит DialogueRunner проиграть диалог этого NPC.
    /// Автоматически пытается получитьDialogueAsset из NpcSaveable, если поле dialogue не задано в инспекторе.
    /// </summary>
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] DialogueAsset dialogue;

        [Tooltip("Игрок. Если не задан, ищется объект с тегом Player.")]
        [SerializeField] Transform player;

        [SerializeField] float triggerDistance = 2f;

        [Tooltip("Включено: диалог начинается сам, когда игрок подошёл. " +
                 "Выключено: диалог начинается при вызове Interact() (например, по кнопке).")]
        [SerializeField] bool startAutomatically = false;

        public bool PlayerInRange { get; private set; }

        // Кэшированная ссылка на NpcSaveable на том же объекте
        NpcSaveable npcSaveable;

        void Awake()
        {
            npcSaveable = GetComponent<NpcSaveable>();

            // Если dialogue не задано в инспекторе, пробуем взять его из NpcSaveable
            if (dialogue == null && npcSaveable != null && npcSaveable.currentDialogue != null)
            {
                dialogue = npcSaveable.currentDialogue;
                Debug.Log($"DialogueTrigger: диалог восстановлен из NpcSaveable ({dialogue.id})", this);
            }

            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) player = go.transform;
                else Debug.LogWarning("DialogueTrigger: игрок не найден (нужен тег Player или ссылка в инспекторе).", this);
            }
        }

        void Start()
        {
            // Если dialogue до сих пор не установлен, пытаемся найти NpcSaveable на Awake уже прошел,
            // но на всякий случай ещё раз проверим здесь
            if (dialogue == null && npcSaveable != null && npcSaveable.currentDialogue != null)
            {
                dialogue = npcSaveable.currentDialogue;
            }
        }

        void Update()
        {
            if (player == null) return;

            bool inRange = (player.position - transform.position).sqrMagnitude <= triggerDistance * triggerDistance;

            // Автозапуск срабатывает только в момент входа в зону, поэтому после конца диалога
            // он не начнётся снова, пока игрок не отойдёт и не вернётся.
            if (inRange && !PlayerInRange && startAutomatically)
                StartDialogue();

            PlayerInRange = inRange;
        }

        /// <summary>Вызовите, например, при нажатии кнопки взаимодействия. Работает, только если игрок рядом.</summary>
        public void Interact()
        {
            if (PlayerInRange) StartDialogue();
        }

        /// <summary>
        /// Запускает диалог, 우선적으로 используя dialogue из NpcSaveable, затем свое поле dialogue,
        /// и наконец ищем по имени среди всех диалогов в сцене.
        /// </summary>
        void StartDialogue()
        {
            // 1. Попробуем взять dialogue из NpcSaveable (актуально после загрузки сейва)
            if (npcSaveable != null && npcSaveable.currentDialogue != null)
            {
                DialogueRunner.Instance.StartDialogue(npcSaveable.currentDialogue);
                return;
            }

            // 2. Используем поле dialogue (задано в инспекторе или восстановлено в Awake)
            if (dialogue != null)
            {
                DialogueRunner.Instance.StartDialogue(dialogue);
                return;
            }

            // 3. Fallback: ищем DialogueAsset по имени "dialogue_{id}" в Resources

            Debug.LogWarning("DialogueTrigger: диалог не назначен и не удалось найти автоматически.", this);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
    }
}