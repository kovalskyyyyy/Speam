using Saves;
using Speam.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Speam.UI
{
    /// <summary>Меню, которое GameManager создаёт при загрузке сцены MainMenu.</summary>
    public class MainMenuController : MonoBehaviour
    {
        Button m_continueButton;
        Text m_saveInfo;

        void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Build();
        }

        void Build()
        {
            var canvas = UiFactory.CreateCanvas("MainMenuCanvas", 100);
            canvas.transform.SetParent(transform, false);

            UiFactory.CreateFullScreenImage(
                "Background",
                canvas.transform,
                new Color(0.045f, 0.05f, 0.07f, 1f));

            var title = UiFactory.CreateText(
                "Title",
                canvas.transform,
                "SPEAM",
                110,
                new Color(0.92f, 0.86f, 0.72f, 1f));

            title.fontStyle = FontStyle.Bold;
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -110f);
            titleRect.sizeDelta = new Vector2(1200f, 140f);

            var subtitle = UiFactory.CreateText(
                "Subtitle",
                canvas.transform,
                "интерактивная история",
                28,
                new Color(0.6f, 0.63f, 0.7f, 1f));

            var subtitleRect = subtitle.rectTransform;
            subtitleRect.anchorMin = new Vector2(0.5f, 1f);
            subtitleRect.anchorMax = new Vector2(0.5f, 1f);
            subtitleRect.pivot = new Vector2(0.5f, 1f);
            subtitleRect.anchoredPosition = new Vector2(0f, -250f);
            subtitleRect.sizeDelta = new Vector2(900f, 40f);

            var panel = UiFactory.CreateRect("Buttons", canvas.transform);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(520f, 300f);
            panel.anchoredPosition = new Vector2(0f, -40f);
            UiFactory.AddVerticalLayout(panel, 18f);

            UiFactory.CreateButton(
                "NewGame",
                "Новая игра",
                panel,
                () => GameManager.Instance.StartNewGame());

            m_continueButton = UiFactory.CreateButton(
                "Continue",
                "Продолжить",
                panel,
                () => GameManager.Instance.ContinueGame(0));

            UiFactory.CreateButton(
                "Quit",
                "Выход",
                panel,
                () => GameManager.Instance.Quit());

            m_saveInfo = UiFactory.CreateText(
                "SaveInfo",
                canvas.transform,
                string.Empty,
                22,
                new Color(0.55f, 0.58f, 0.65f, 1f));

            var infoRect = m_saveInfo.rectTransform;
            infoRect.anchorMin = new Vector2(0.5f, 0f);
            infoRect.anchorMax = new Vector2(0.5f, 0f);
            infoRect.pivot = new Vector2(0.5f, 0f);
            infoRect.anchoredPosition = new Vector2(0f, 80f);
            infoRect.sizeDelta = new Vector2(900f, 40f);

            var footer = UiFactory.CreateText(
                "Footer",
                canvas.transform,
                "v0.1",
                20,
                new Color(0.4f, 0.43f, 0.5f, 1f));

            var footerRect = footer.rectTransform;
            footerRect.anchorMin = new Vector2(1f, 0f);
            footerRect.anchorMax = new Vector2(1f, 0f);
            footerRect.pivot = new Vector2(1f, 0f);
            footerRect.anchoredPosition = new Vector2(-32f, 24f);
            footerRect.sizeDelta = new Vector2(200f, 30f);

            RefreshSaveInfo();
        }

        void RefreshSaveInfo()
        {
            var saveManager = SaveManager.Instance;
            bool hasSave = saveManager != null && saveManager.HasSave(0);

            if (m_continueButton != null)
                m_continueButton.interactable = hasSave;

            if (m_saveInfo == null) return;

            if (!hasSave)
            {
                m_saveInfo.text = "Сохранений пока нет";
                return;
            }

            var data = saveManager.Read(0);
            m_saveInfo.text = data != null && !string.IsNullOrEmpty(data.savedAt)
                ? $"Сохранение: {data.savedAt}"
                : "Сохранение найдено";
        }
    }
}
