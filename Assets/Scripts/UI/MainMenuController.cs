using System;
using Saves;
using Speam.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Speam.UI
{
    /// <summary>Меню главного экрана, создаётся через prefab.</summary>
    public class MainMenuController : MonoBehaviour
    {
        // Designer assigns these in the Prefab Inspector
        [SerializeField] private Button m_newGameButton;
        [SerializeField] private Button m_continueButton;
        [SerializeField] private Button m_quitButton;
        [SerializeField] private Text m_saveInfoText;

        private void Awake()
        {
            // Set up button actions once when prefab is instantiated
            if (m_newGameButton != null)
            {
                m_newGameButton.onClick.AddListener(() => GameManager.Instance.StartNewGame());
            }
            if (m_continueButton != null)
            {
                m_continueButton.onClick.AddListener(() => GameManager.Instance.ContinueGame(0));
            }
            if (m_quitButton != null)
            {
                m_quitButton.onClick.AddListener(() => GameManager.Instance.Quit());
            }

            // Read last save info to display last saved timestamp
            UpdateSaveInfo();
        }

        /// <summary>
        /// Update save info text with last save timestamp.
        /// Reads from SaveManager if available, otherwise shows no-save state.
        /// </summary>
        public void UpdateSaveInfo()
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
    }
}