using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DialogueSystem.Runtime
{
    /// <summary>
    /// Проигрывает DialogueAsset в виде ленты (как в Disco Elysium):
    /// реплики добавляются в ScrollRect, имя говорящего пишется цветом перед текстом,
    /// портрет один общий и меняется при смене говорящего.
    /// </summary>
    public class DialogueRunner : MonoBehaviour
    {
        [Header("Dialogue")]
        public DialogueAsset dialogue;

        [Header("UI")]
        [Tooltip("Корневая панель диалога. Включается при старте и выключается в конце.")]
        [SerializeField] GameObject dialoguePanel;
        [SerializeField] ScrollRect scrollRect;
        [Tooltip("Content внутри ScrollRect (VerticalLayoutGroup + ContentSizeFitter).")]
        [SerializeField] RectTransform content;
        [Tooltip("Префаб реплики: один TMP_Text на корне.")]
        [SerializeField] TMP_Text linePrefab;
        [Tooltip("Префаб кнопки: Button + дочерний TMP_Text. Для вариантов и для «Дальше».")]
        [SerializeField] Button choicePrefab;
        [Tooltip("Необязательно: общий портрет говорящего (вне ленты).")]
        [SerializeField] Image speakerIcon;
        [SerializeField] string continueText = "Дальше";

        [Header("Settings")]
        [Tooltip("Время сглаживания скролла, сек. Больше = плавнее и медленнее.")]
        [SerializeField] float scrollSmoothTime = 0.25f;
        [Tooltip("Прозрачность старых реплик (1 = не затемнять).")]
        [SerializeField, Range(0f, 1f)] float oldLineAlpha = 0.5f;
        [Tooltip("Добавлять выбранный вариант в ленту как реплику игрока.")]
        [SerializeField] bool echoChoice = true;
        [Tooltip("Имя игрока в ленте. Пусто = без имени.")]
        [SerializeField] string playerName = "";
        [SerializeField] Color playerColor = Color.white;

        public event Action Started;
        public event Action<SpeakerData, string> LineShown;
        public event Action<IReadOnlyList<string>> ChoicesShown;
        public event Action Ended;

        public bool IsRunning { get; private set; }

        /// <summary>Единственный раннер на сцене. NPC обращаются к нему через DialogueRunner.Instance.</summary>
        public static DialogueRunner Instance { get; private set; }

        int current = -1;
        bool waitingForAdvance;
        Coroutine actionRoutine;
        Coroutine scrollRoutine;
        TMP_Text lastLine;
        readonly List<ChoiceOption> visibleOptions = new List<ChoiceOption>();
        readonly List<GameObject> transient = new List<GameObject>();   // кнопки выбора / «Дальше»

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("На сцене уже есть DialogueRunner, лишний компонент удалён.", this);
                Destroy(this);
                return;
            }
            Instance = this;

            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        // ---------- Запуск ----------

        public void StartDialogue() => StartDialogue(dialogue);

        public void StartDialogue(DialogueAsset asset)
        {
            if (IsRunning) return;

            if (asset == null || asset.startIndex < 0)
            {
                Debug.LogWarning("Диалог пуст или не подключён.");
                return;
            }
            if (content == null || linePrefab == null || choicePrefab == null)
            {
                Debug.LogError("DialogueRunner: не заданы content / linePrefab / choicePrefab.", this);
                return;
            }

            dialogue = asset;
            IsRunning = true;
            current = asset.startIndex;

            ClearLog();
            if (speakerIcon != null) speakerIcon.enabled = false;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);

            Started?.Invoke();
            Process();
        }

        // ---------- Управление ----------

        /// <summary>Переход к следующей ноде (кнопка «Дальше»).</summary>
        public void Advance()
        {
            if (!IsRunning || !waitingForAdvance) return;

            waitingForAdvance = false;
            ClearTransient();
            current = dialogue.nodes[current].next;
            Process();
        }

        /// <summary>Выбор варианта (индекс в списке показанных вариантов).</summary>
        public void Choose(int visibleIndex)
        {
            if (!IsRunning || visibleIndex < 0 || visibleIndex >= visibleOptions.Count) return;

            var option = visibleOptions[visibleIndex];
            ClearTransient();

            if (echoChoice)
                AddEntry(playerName, playerColor, option.text);

            visibleOptions.Clear();
            current = option.nextIndex;
            Process();
        }

        void Process()
        {
            if (current < 0 || current >= dialogue.nodes.Count)
            {
                Finish();
                return;
            }

            switch (dialogue.nodes[current])
            {
                case LineNodeData line:     ShowLine(line);      break;
                case ChoiceNodeData choice: ShowChoices(choice); break;
                case ActionNodeData action: RunAction(action);   break;
                default:                    Finish();            break;   // EndNodeData и всё неизвестное
            }
        }

        // ---------- Лента ----------

        /// <summary>Добавляет запись в ленту. Имя (если есть) пишется цветом перед текстом.</summary>
        TMP_Text AddEntry(string speakerName, Color nameColor, string text)
        {
            DimLastLine();

            var tmp = Instantiate(linePrefab, content);
            string body = text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(speakerName))
            {
                tmp.text = body;
            }
            else
            {
                string hex = ColorUtility.ToHtmlStringRGB(nameColor);
                tmp.text = $"<b><color=#{hex}>{speakerName}</color></b>  {body}";
            }

            lastLine = tmp;
            return tmp;
        }

        void DimLastLine()
        {
            if (lastLine != null) lastLine.alpha = oldLineAlpha;
        }

        void ClearLog()
        {
            ClearTransient();
            lastLine = null;
            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);
        }

        void SpawnButton(string label, UnityAction onClick)
        {
            var button = Instantiate(choicePrefab, content);
            var tmp = button.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = label;
            button.onClick.AddListener(onClick);
            transient.Add(button.gameObject);
        }

        void ClearTransient()
        {
            foreach (var go in transient)
            {
                if (go == null) continue;
                go.SetActive(false);   // сразу убираем из лэйаута, Destroy сработает только в конце кадра
                Destroy(go);
            }
            transient.Clear();
        }

        void ScrollDown()
        {
            if (scrollRoutine != null) StopCoroutine(scrollRoutine);
            scrollRoutine = StartCoroutine(ScrollDownRoutine());
        }

        IEnumerator ScrollDownRoutine()
        {
            // Позиция сейчас та же, что была до добавления записи. Ждём, пока Layout посчитает новую высоту.
            yield return null;

            if (scrollRect == null) { scrollRoutine = null; yield break; }

            var viewport = scrollRect.viewport != null
                ? scrollRect.viewport
                : (RectTransform)scrollRect.transform;

            scrollRect.StopMovement();

            float velocity = 0f;
            float lastSetY = content.anchoredPosition.y;
            int settledFrames = 0;

            while (settledFrames < 2)
            {
                // Позицию изменили не мы (колесо, перетаскивание, инерция): игрок взял управление.
                if (Mathf.Abs(content.anchoredPosition.y - lastSetY) > 0.5f)
                {
                    scrollRoutine = null;
                    yield break;
                }

                float target = Mathf.Max(0f, content.rect.height - viewport.rect.height);
                float y = scrollSmoothTime <= 0f
                    ? target
                    : Mathf.SmoothDamp(content.anchoredPosition.y, target, ref velocity,
                        scrollSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

                var pos = content.anchoredPosition;
                pos.y = y;
                content.anchoredPosition = pos;
                lastSetY = y;

                settledFrames = Mathf.Abs(y - target) < 0.5f ? settledFrames + 1 : 0;
                yield return null;
            }

            scrollRoutine = null;
        }

        // ---------- Действия ----------

        void RunAction(ActionNodeData node)
        {
            waitingForAdvance = false;
            ClearTransient();
            StopAction();
            actionRoutine = StartCoroutine(ActionRoutine(node));
        }

        IEnumerator ActionRoutine(ActionNodeData node)
        {
            if (node.action != null)
                yield return node.action.Execute();   // ждём, пока действие закончится

            actionRoutine = null;
            if (!IsRunning) yield break;              // диалог могли прервать, пока действие шло

            current = node.next;
            Process();
        }

        void StopAction()
        {
            if (actionRoutine != null)
            {
                StopCoroutine(actionRoutine);
                actionRoutine = null;
            }
        }

        // ---------- Реплика ----------

        void ShowLine(LineNodeData line)
        {
            ClearTransient();
            waitingForAdvance = false;

            var speaker = line.speaker;

            // общий портрет
            if (speakerIcon != null)
            {
                var sprite = speaker != null ? speaker.sprite1 : null;
                speakerIcon.sprite = sprite;
                speakerIcon.enabled = sprite != null;
            }

            AddEntry(
                speaker != null ? speaker.displayName : string.Empty,
                speaker != null ? speaker.nameColor : Color.white,
                line.text);

            LineShown?.Invoke(speaker, line.text);

            // Текст показан целиком: если дальше выбор, показываем его сразу, иначе кнопка «Дальше».
            int next = line.next;
            if (next >= 0 && next < dialogue.nodes.Count && dialogue.nodes[next] is ChoiceNodeData)
            {
                current = next;
                Process();
                return;
            }

            waitingForAdvance = true;
            SpawnButton(continueText, Advance);
            ScrollDown();
        }

        // ---------- Выбор ----------

        void ShowChoices(ChoiceNodeData choice)
        {
            ClearTransient();
            visibleOptions.Clear();

            foreach (var option in choice.options)
            {
                if (string.IsNullOrWhiteSpace(option.text)) continue;
                if (option.condition != null && !option.condition.IsMet()) continue;
                visibleOptions.Add(option);
            }

            if (visibleOptions.Count == 0)
            {
                Debug.LogWarning("В ноде выбора нет ни одного доступного варианта, диалог завершён.");
                Finish();
                return;
            }

            var texts = new List<string>();
            for (int i = 0; i < visibleOptions.Count; i++)
            {
                int index = i;   // копия для замыкания
                SpawnButton($"{i + 1}. {visibleOptions[i].text}", () => Choose(index));
                texts.Add(visibleOptions[i].text);
            }

            ScrollDown();
            ChoicesShown?.Invoke(texts);
        }

        // ---------- Завершение ----------

        void Finish()
        {
            StopAction();
            if (scrollRoutine != null) { StopCoroutine(scrollRoutine); scrollRoutine = null; }
            IsRunning = false;
            waitingForAdvance = false;
            current = -1;
            visibleOptions.Clear();
            ClearTransient();
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            Ended?.Invoke();
        }
    }
}