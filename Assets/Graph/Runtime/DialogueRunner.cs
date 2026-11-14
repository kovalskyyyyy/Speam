using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem.Runtime
{
    /// <summary>
    /// Проигрывает DialogueAsset и выводит его в UI: текст (с эффектом печати), имя, иконку и варианты выбора.
    /// Все поля UI необязательны: пустые просто пропускаются.
    /// </summary>
    public class DialogueRunner : MonoBehaviour
    {
        [Header("Dialogue")]
        public DialogueAsset dialogue;

        [Header("UI")]
        [Tooltip("Корневая панель диалога. Включается при старте диалога и выключается в конце.")]
        [SerializeField] GameObject dialoguePanel;
        [Tooltip("Сюда выводится текст реплики.")]
        [SerializeField] TMP_Text dialogueText;
        [Tooltip("Необязательно: имя говорящего.")]
        [SerializeField] TMP_Text speakerNameText;
        [Tooltip("Сюда выводится sprite1 говорящего.")]
        [SerializeField] Image speakerIcon;
        [Tooltip("Тексты вариантов выбора. Если TMP лежит внутри Button, клик по кнопке выберет вариант.")]
        [SerializeField] List<TMP_Text> choiceTexts = new List<TMP_Text>();

        [Header("Input")]
        [Tooltip("Необязательно: кнопка «дальше» (например, невидимая кнопка на весь экран). Либо вызывайте Advance() из своего кода.")]
        [SerializeField] Button advanceButton;

        [Header("Settings")]
        [Tooltip("Скорость печати, если у говорящего не задан textSpeed (символов в секунду).")]
        [SerializeField] float defaultTextSpeed = 30f;

        public event Action Started;
        public event Action<SpeakerData, string> LineShown;
        public event Action<IReadOnlyList<string>> ChoicesShown;
        public event Action Ended;

        public bool IsRunning { get; private set; }

        /// <summary>Единственный раннер на сцене. NPC обращаются к нему через DialogueRunner.Instance.</summary>
        public static DialogueRunner Instance { get; private set; }

        int current = -1;
        bool waitingForAdvance;
        Coroutine typingRoutine;
        readonly List<ChoiceOption> visibleOptions = new List<ChoiceOption>();
        readonly List<GameObject> choiceRoots = new List<GameObject>();

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

            // Для каждого текста варианта находим кнопку выше по иерархии и вешаем на неё выбор.
            for (int i = 0; i < choiceTexts.Count; i++)
            {
                var label = choiceTexts[i];
                if (label == null) { choiceRoots.Add(null); continue; }

                var button = label.GetComponentInParent<Button>();
                if (button != null)
                {
                    int index = i;
                    button.onClick.AddListener(() => Choose(index));
                    choiceRoots.Add(button.gameObject);
                }
                else
                {
                    Debug.LogWarning($"У варианта выбора «{label.name}» нет родительской Button: клик по нему ничего не выберет.", label);
                    choiceRoots.Add(label.gameObject);
                }
            }

            if (advanceButton != null)
                advanceButton.onClick.AddListener(Advance);

            HideChoices();
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        public void StartDialogue() => StartDialogue(dialogue);

        public void StartDialogue(DialogueAsset asset)
        {
            if (IsRunning) return;

            if (asset == null || asset.startIndex < 0)
            {
                Debug.LogWarning("Диалог пуст или не подключён.");
                return;
            }

            dialogue = asset;
            IsRunning = true;
            current = asset.startIndex;

            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            Started?.Invoke();
            Process();
        }

        /// <summary>
        /// Кнопка «дальше». Если текст ещё печатается, показывает его целиком,
        /// иначе переходит к следующей ноде.
        /// </summary>
        public void Advance()
        {
            if (!IsRunning) return;

            if (typingRoutine != null)
            {
                CompleteTyping();
                return;
            }

            if (!waitingForAdvance) return;

            waitingForAdvance = false;
            current = dialogue.nodes[current].next;
            Process();
        }

        /// <summary>Выбор варианта (индекс в списке показанных вариантов).</summary>
        public void Choose(int visibleIndex)
        {
            if (!IsRunning || visibleIndex < 0 || visibleIndex >= visibleOptions.Count) return;

            current = visibleOptions[visibleIndex].nextIndex;
            visibleOptions.Clear();
            HideChoices();
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
                case LineNodeData line:
                    ShowLine(line);
                    break;

                case ChoiceNodeData choice:
                    ShowChoices(choice);
                    break;

                default:   // EndNodeData и всё неизвестное
                    Finish();
                    break;
            }
        }

        // ---------- Реплика ----------

        void ShowLine(LineNodeData line)
        {
            HideChoices();
            waitingForAdvance = true;

            var speaker = line.speaker;

            if (speakerNameText != null)
                speakerNameText.text = speaker != null ? speaker.displayName : string.Empty;

            if (speakerIcon != null)
            {
                var sprite = speaker != null ? speaker.sprite1 : null;
                speakerIcon.sprite = sprite;
                speakerIcon.enabled = sprite != null;
            }

            if (dialogueText != null)
            {
                float speed = speaker != null && speaker.textSpeed > 0f ? speaker.textSpeed : defaultTextSpeed;
                StopTyping();
                typingRoutine = StartCoroutine(Typewrite(line.text ?? string.Empty, speed));
            }

            LineShown?.Invoke(speaker, line.text);
        }

        IEnumerator Typewrite(string text, float charsPerSecond)
        {
            dialogueText.text = text;
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate();

            int total = dialogueText.textInfo.characterCount;
            float shown = 0f;

            while (shown < total)
            {
                shown += charsPerSecond * Time.unscaledDeltaTime;
                dialogueText.maxVisibleCharacters = Mathf.Min(total, (int)shown);
                yield return null;
            }

            dialogueText.maxVisibleCharacters = int.MaxValue;
            typingRoutine = null;
        }

        void CompleteTyping()
        {
            StopTyping();
            if (dialogueText != null) dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        void StopTyping()
        {
            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
                typingRoutine = null;
            }
        }

        // ---------- Выбор ----------

        void ShowChoices(ChoiceNodeData choice)
        {
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

            if (visibleOptions.Count > choiceTexts.Count)
            {
                Debug.LogWarning($"Доступно вариантов: {visibleOptions.Count}, а TMP-слотов только {choiceTexts.Count}. Лишние не показаны.");
                visibleOptions.RemoveRange(choiceTexts.Count, visibleOptions.Count - choiceTexts.Count);
            }

            var texts = new List<string>();
            for (int i = 0; i < choiceTexts.Count; i++)
            {
                bool used = i < visibleOptions.Count;
                if (choiceRoots[i] != null) choiceRoots[i].SetActive(used);
                if (used)
                {
                    if (choiceTexts[i] != null) choiceTexts[i].text = visibleOptions[i].text;
                    texts.Add(visibleOptions[i].text);
                }
            }

            ChoicesShown?.Invoke(texts);
        }

        void HideChoices()
        {
            foreach (var root in choiceRoots)
                if (root != null) root.SetActive(false);
        }

        // ---------- Завершение ----------

        void Finish()
        {
            StopTyping();
            IsRunning = false;
            waitingForAdvance = false;
            current = -1;
            visibleOptions.Clear();
            HideChoices();
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            Ended?.Invoke();
        }
    }
}