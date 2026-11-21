using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Speam.UI
{
    /// <summary>Создание простого uGUI-интерфейса кодом, без префабов.</summary>
    public static class UiFactory
    {
        static Font s_font;

        public static Font DefaultFont
        {
            get
            {
                if (s_font != null) return s_font;

                s_font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (s_font == null)
                    s_font = Font.CreateDynamicFontFromOSFont("Arial", 24);

                return s_font;
            }
        }

        public static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        public static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        public static Image CreateFullScreenImage(string name, Transform parent, Color color)
        {
            var image = CreateImage(name, parent, color);
            var rect = image.rectTransform;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return image;
        }

        public static Text CreateText(
            string name,
            Transform parent,
            string content,
            int fontSize,
            Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            var text = go.GetComponent<Text>();
            text.font = DefaultFont;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }

        public static Button CreateButton(
            string name,
            string label,
            Transform parent,
            UnityAction onClick,
            int fontSize = 30)
        {
            var go = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));

            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.16f, 0.18f, 0.24f, 1f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var colors = button.colors;
            colors.normalColor = new Color(0.16f, 0.18f, 0.24f, 1f);
            colors.highlightedColor = new Color(0.26f, 0.30f, 0.40f, 1f);
            colors.pressedColor = new Color(0.11f, 0.13f, 0.18f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.10f, 0.10f, 0.12f, 0.55f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            if (onClick != null) button.onClick.AddListener(onClick);

            var text = CreateText("Label", rect, label, fontSize, Color.white);
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 0f);
            textRect.offsetMax = new Vector2(-24f, 0f);

            var layout = go.GetComponent<LayoutElement>();
            layout.preferredHeight = 64f;
            layout.preferredWidth = -1f;

            return button;
        }

        public static VerticalLayoutGroup AddVerticalLayout(
            RectTransform rect,
            float spacing,
            TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static void AddShadow(Graphic graphic)
        {
            var shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }
    }
}
