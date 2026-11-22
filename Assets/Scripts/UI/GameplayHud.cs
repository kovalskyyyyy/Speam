using System;
using Saves;
using Speam.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Speam.UI
{
    /// <summary>Игровой HUD: подсказка, пауза, сохранение и выход в меню.</summary>
    public class GameplayHud : MonoBehaviour
    {
        // Designer assigns these in the Prefab Inspector
        [SerializeField] private Text m_hintText;
        [SerializeField] private GameObject m_pausePanel;
        [SerializeField] private Text m_saveInfoText;
        [SerializeField] private Button m_resumeButton;
        [SerializeField] private Button m_saveButton;
        [SerializeField] private Button m_mainMenuButton;
        [SerializeField] private Text m_toastText;
        [SerializeField] private float m_toastDuration = 2f;

        private Coroutine m_toastRoutine;

        private void Start()
        {
            // Initial setup - just cache, don't create
            m_pausePanel.SetActive(false);
            if (m_hintText != null)
            {
                m_hintText.text = "Esc — пауза    F5 — сохранить";
            }

            // Set up pause panel button actions once
            if (m_resumeButton != null)
            {
                m_resumeButton.onClick.AddListener(() =>
                {
                    HidePause();
                });
            }

            if (m_saveButton != null)
            {
                m_saveButton.onClick.AddListener(() =>
                {
                    SaveGame();
                });
            }

            if (m_mainMenuButton != null)
            {
                m_mainMenuButton.onClick.AddListener(() =>
                {
                    GoToMainMenu();
                });
            }
        }

        private void Update()
        {
            // Handle Escape key to toggle pause
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (GameManager.Instance != null && GameManager.Instance.State == GameFlowState.Paused)
                {
                    HidePause();
                }
                else if (GameManager.Instance != null && GameManager.Instance.State == GameFlowState.Gameplay)
                {
                    ShowPause();
                }
            }
        }

        /// <summary>
        /// Show the pause panel and pause the game.
        /// </summary>
        public void ShowPause()
        {
            if (m_pausePanel != null)
            {
                m_pausePanel.SetActive(true);
            }
            // Update save info text in pause menu
            UpdatePauseSaveInfo();
            // Pause the game via GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPaused(true);
            }
        }

        /// <summary>
        /// Hide the pause panel and resume the game.
        /// </summary>
        public void HidePause()
        {
            if (m_pausePanel != null)
            {
                m_pausePanel.SetActive(false);
            }
            // Resume the game via GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPaused(false);
            }
        }

        /// <summary>
        /// Update save info text in the pause menu with last save timestamp.
        /// </summary>
        private void UpdatePauseSaveInfo()
        {
            if (m_saveInfoText == null) return;

            if (SaveManager.Instance == null || !SaveManager.Instance.HasSave(0))
            {
                m_saveInfoText.text = "Сохранений пока нет";
                return;
            }

            var saveData = SaveManager.Instance.Read(0);
            if (saveData != null && !string.IsNullOrEmpty(saveData.savedAt))
            {
                m_saveInfoText.text = $"Сохранение: {saveData.savedAt}";
            }
            else
            {
                m_saveInfoText.text = "Сохранение найдено";
            }
        }

        /// <summary>
        /// Save the current game.
        /// </summary>
        public void SaveGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame(0);
                ShowToast($"Сохранение {DateTime.Now:HH:mm}");
            }
        }

        /// <summary>
        /// Go to main menu.
        /// </summary>
        public void GoToMainMenu()
        {
            // Resume game first if paused
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPaused(false);
            }
            GameManager.Instance.GoToMainMenu();
        }

        /// <summary>
        /// Show a toast message briefly.
        /// </summary>
        public void ShowToast(string message)
        {
            if (m_toastText == null) return;

            if (m_toastRoutine != null) StopCoroutine(m_toastRoutine);
            m_toastRoutine = StartCoroutine(ToastRoutine(message));
        }

        IEnumerator ToastRoutine(string message)
        {
            m_toastText.text = message;
            m_toastText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(m_toastDuration);
            m_toastText.gameObject.SetActive(false);
            m_toastRoutine = null;
        }
    }
}