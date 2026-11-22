using Saves;
using Speam.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Speam.UI
{
    /// <summary>Игровой HUD: подсказка, пауза, сохранение и выход в меню.</summary>
    public class GameplayHud : MonoBehaviour
    {
        // Designer assigns these in the Prefab Inspector
        [SerializeField] private Text m_hintText;
        [SerializeField] private GameObject m_pausePanel;
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
        }

        /// <summary>
        /// Show the pause panel.
        /// </summary>
        public void ShowPause()
        {
            if (m_pausePanel != null)
            {
                m_pausePanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the pause panel.
        /// </summary>
        public void HidePause()
        {
            if (m_pausePanel != null)
            {
                m_pausePanel.SetActive(false);
            }
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