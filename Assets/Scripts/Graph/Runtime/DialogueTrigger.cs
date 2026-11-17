using UnityEngine;

namespace DialogueSystem.Runtime
{
    /// <summary>
    /// Вешается на NPC. Следит, подошёл ли игрок, и просит DialogueRunner проиграть диалог этого NPC.
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

        void Start()
        {
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) player = go.transform;
                else Debug.LogWarning("DialogueTrigger: игрок не найден (нужен тег Player или ссылка в инспекторе).", this);
            }
        }

        void Update()
        {
            if (player == null) return;

            bool inRange = (player.position - transform.position).sqrMagnitude <= triggerDistance * triggerDistance;

            // Автозапуск срабатывает только в момент входа в зону, поэтому после конца диалога
            // он не начнётся заново, пока игрок не отойдёт и не вернётся.
            if (inRange && !PlayerInRange && startAutomatically)
                StartDialogue();

            PlayerInRange = inRange;
        }

        /// <summary>Вызовите, например, при нажатии кнопки взаимодействия. Работает, только если игрок рядом.</summary>
        public void Interact()
        {
            if (PlayerInRange) StartDialogue();
        }

        void StartDialogue()
        {
            if (DialogueRunner.Instance == null)
            {
                Debug.LogWarning("DialogueTrigger: на сцене нет DialogueRunner.", this);
                return;
            }
            DialogueRunner.Instance.StartDialogue(dialogue);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
    }
}