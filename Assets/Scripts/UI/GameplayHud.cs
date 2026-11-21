using System.Collections;
using Saves;
using Speam.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Speam.UI
{
    /// <summary>Игровой HUD: подсказка, пауза, сохранение и выход в меню.</summary>
    public class GameplayHud : MonoBehaviour
    {
        GameObject m_pausePanel;
        Text m_toast;
        Text m_saveStatus;
        Coroutine m_toastRoutine;

        void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Build();
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || GameManager.Instance == null) return;

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (GameManager.Instance.State == GameFlowState.Paused) Resume();
                else if (GameManager.Instance.State == GameFlowState.Gameplay) Pause();
            }

            if (keyboard.f5Key.wasPressedThisFrame &&
                GameManager.Instance.State == GameFlowState.Gameplay)
            {
                SaveGame();
            }
        }

        void Build()
        {
            var canvas = UiFactory.CreateCanvas("GameplayCanvas", 500);
            canvas.transform.SetParent(transform, false);

            var hint = UiFactory.CreateText(
                "Hint",
                canvas.transform,
                "Esc — пауза    F5 — сохранить",
                22,
                new Color(1f, 1f, 1f, 0.85f),
                TextAnchor.UpperLeft);

            var hintRect = hint.rectTransform;
            hintRect.anchorMin = new Vector2(0f, 1f);
            hintRect.anchorMax = new Vector2(0f, 1f);
            hintRect.pivot = new Vector2(0f, 1f);
            hintRect.anchoredPosition = new Vector2(28f, -24f);
            hintRect.sizeDelta = new Vector2(700f, 40f);
            UiFactory.AddShadow(hint);

            BuildPausePanel(canvas.transform);
            BuildToast(canvas.transform);
        }

        void BuildPausePanel(Transform canvas)
        {
            var dim = UiFactory.CreateFullScreenImage(
                "PauseDim",
                canvas,
                new Color(0f, 0f, 0f, 0.72f));

            m_pausePanel = dim.gameObject;

            var panel = UiFactory.CreateImage(
                "PausePanel",
                m_pausePanel.transform,
                new Color(0.07f, 0.08f, 0.11f, 1f));

            var panelRect = panel.rectTransform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(560f, 430f);
            panelRect.anchoredPosition = Vector2.zero;

            var title = UiFactory.CreateText(
                "Title",
                panelRect,
                "Пауза",
                54,
                new Color(0.92f, 0.86f, 0.72f, 1f));

            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -36f);
            titleRect.sizeDelta = new Vector2(500f, 70f);

            var buttons = UiFactory.CreateRect("Buttons", panelRect);
            buttons.anchorMin = new Vector2(0.5f, 0.5f);
            buttons.anchorMax = new Vector2(0.5f, 0.5f);
            buttons.sizeDelta = new Vector2(440f, 230f);
            buttons.anchoredPosition = new Vector2(0f, -30f);
            UiFactory.AddVerticalLayout(buttons, 16f);

            UiFactory.CreateButton("Resume", "Продолжить", buttons, Resume);
            UiFactory.CreateButton("Save", "Сохранить", buttons, SaveGame);
            UiFactory.CreateButton("MainMenu", "В главное меню", buttons, GoToMainMenu);

            m_saveStatus = UiFactory.CreateText(
                "SaveStatus",
                panelRect,
                string.Empty,
                20,
                new Color(0.55f, 0.58f, 0.65f, 1f));

            var statusRect = m_saveStatus.rectTransform;
            statusRect.anchorMin = new Vector2(0.5f, 0f);
            statusRect.anchorMax = new Vector2(0.5f, 0f);
            statusRect.pivot = new Vector2(0.5f, 0f);
            statusRect.anchoredPosition = new Vector2(0f, 24f);
            statusRect.sizeDelta = new Vector2(500f, 40f);

            m_pausePanel.SetActive(false);
        }

        void BuildToast(Transform canvas)
        {
            m_toast = UiFactory.CreateText(
                "Toast",
                canvas,
                string.Empty,
                28,
                new Color(0.75f, 0.95f, 0.8f, 1f));

            var rect = m_toast.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 80f);
            rect.sizeDelta = new Vector2(800f, 50f);
            UiFactory.AddShadow(m_toast);

            m_toast.gameObject.SetActive(false);
            m_toast.transform.SetAsLastSibling();
        }

        void Pause()
        {
            GameManager.Instance.SetPaused(true);
            m_pausePanel.SetActive(true);
            RefreshSaveStatus();
        }

        void Resume()
        {
            m_pausePanel.SetActive(false);
            GameManager.Instance.SetPaused(false);
        }

        void SaveGame()
        {
            bool saved = GameManager.Instance.SaveGame(0);
            ShowToast(saved ? "Сохранено" : "Не удалось сохранить");
            RefreshSaveStatus();
        }

        void GoToMainMenu()
        {
            Time.timeScale = 1f;
            GameManager.Instance.GoToMainMenu();
        }

        void RefreshSaveStatus()
        {
            if (m_saveStatus == null) return;

            var saveManager = SaveManager.Instance;
            if (saveManager == null || !saveManager.HasSave(0))
            {
                m_saveStatus.text = "Сохранений пока нет";
                return;
            }

            var data = saveManager.Read(0);
            m_saveStatus.text = data != null && !string.IsNullOrEmpty(data.savedAt)
                ? $"Последнее сохранение: {data.savedAt}"
                : "Сохранение найдено";
        }

        void ShowToast(string message)
        {
            if (m_toast == null) return;

            if (m_toastRoutine != null) StopCoroutine(m_toastRoutine);
            m_toastRoutine = StartCoroutine(ToastRoutine(message));
        }

        IEnumerator ToastRoutine(string message)
        {
            m_toast.text = message;
            m_toast.gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(2f);

            m_toast.gameObject.SetActive(false);
            m_toastRoutine = null;
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameFlowState.Paused)
                Time.timeScale = 1f;
        }
    }
}
