using UnityEngine;
using UnityEngine.UI;

namespace Speam.UI
{
    /// <summary>Полноэкранный экран загрузки, живёт на [GameRoot].</summary>
    public class LoadingScreen : MonoBehaviour
    {
        GameObject m_root;
        RectTransform m_fill;
        Text m_label;

        void Awake()
        {
            Build();
            Hide();
        }

        void Build()
        {
            var canvas = UiFactory.CreateCanvas("LoadingCanvas", 1000);
            canvas.transform.SetParent(transform, false);
            m_root = canvas.gameObject;

            UiFactory.CreateFullScreenImage(
                "Background",
                canvas.transform,
                new Color(0.03f, 0.035f, 0.05f, 1f));

            m_label = UiFactory.CreateText(
                "Label",
                canvas.transform,
                "Загрузка...",
                34,
                Color.white);

            var labelRect = m_label.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = new Vector2(0f, 50f);
            labelRect.sizeDelta = new Vector2(800f, 60f);

            var barBackground = UiFactory.CreateImage(
                "BarBackground",
                canvas.transform,
                new Color(0.15f, 0.16f, 0.2f, 1f));

            var barRect = barBackground.rectTransform;
            barRect.anchorMin = new Vector2(0.5f, 0.5f);
            barRect.anchorMax = new Vector2(0.5f, 0.5f);
            barRect.anchoredPosition = new Vector2(0f, -20f);
            barRect.sizeDelta = new Vector2(640f, 18f);

            var fill = UiFactory.CreateImage(
                "BarFill",
                barRect,
                new Color(0.35f, 0.75f, 0.95f, 1f));

            m_fill = fill.rectTransform;
            m_fill.anchorMin = new Vector2(0f, 0f);
            m_fill.anchorMax = new Vector2(0f, 1f);
            m_fill.offsetMin = Vector2.zero;
            m_fill.offsetMax = Vector2.zero;
        }

        public void Show()
        {
            if (m_root != null) m_root.SetActive(true);
            SetProgress(0f);
        }

        public void Hide()
        {
            if (m_root != null) m_root.SetActive(false);
        }

        public void SetProgress(float value)
        {
            value = Mathf.Clamp01(value);

            if (m_fill != null)
                m_fill.anchorMax = new Vector2(value, 1f);

            if (m_label != null)
                m_label.text = value >= 1f
                    ? "Готово"
                    : $"Загрузка... {Mathf.RoundToInt(value * 100f)}%";
        }
    }
}
