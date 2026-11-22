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
        }

        /// <summary>
        /// Call from outside to update save info text.
        /// </summary>
        public void UpdateSaveInfo(bool hasSave, string savedAt = "")
        {
            if (m_saveInfoText == null) return;

            if (!hasSave)
            {
                m_saveInfoText.text = "Сохранений пока нет";
                return;
            }

            m_saveInfoText.text = !string.IsNullOrEmpty(savedAt)
                ? $"Сохранение: {savedAt}"
                : "Сохранение найдено";
        }
    }
}