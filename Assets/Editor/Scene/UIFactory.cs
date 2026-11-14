using UnityEngine;
using UnityEngine.UI;

namespace PotionLab.EditorTools
{
    /// <summary>
    /// Creates UI elements. Positions are in the pixels of the 1920x1080 art layout:
    /// each element is anchored to its parent's bottom-left corner and centred on (x, y).
    /// </summary>
    public static class UIFactory
    {
        public static RectTransform NewRect(string name, Transform parent, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        public static Image NewImage(string name, Transform parent, Sprite sprite, float x, float y, float width, float height, bool blocksClicks)
        {
            var image = NewRect(name, parent, x, y, width, height).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = blocksClicks;
            return image;
        }

        public static Text NewText(string name, Transform parent, float x, float y, float width, float height, int size, Color color,
            TextAnchor alignment, FontStyle style)
        {
            var text = NewRect(name, parent, x, y, width, height).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        public static Button NewButton(Image image, Selectable.Transition transition)
        {
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = transition;
            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
            return button;
        }
    }
}
