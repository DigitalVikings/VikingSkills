using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VikingSkills.UI
{
    /// <summary>
    /// Fluent procedural UI builder and extension methods for concise, maintainable Unity UI layout.
    /// </summary>
    public static class UIBuilder
    {
        public static GameObject CreateChild(this Transform parent, string name, params Type[] components)
        {
            var go = new GameObject(name, components);
            go.transform.SetParent(parent, false);
            return go;
        }

        public static RectTransform SetAnchors(this RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            return rect;
        }

        public static RectTransform Fill(this RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        public static Image AddImage(this Transform parent, string name, Sprite? sprite, Color color, Image.Type type = Image.Type.Simple)
        {
            var go = parent.CreateChild(name, typeof(RectTransform), typeof(Image));
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = type;
            return img;
        }

        public static TextMeshProUGUI AddText(this Transform parent, string name, string text, TMP_FontAsset? font, float fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var go = parent.CreateChild(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            return tmp;
        }
    }
}
