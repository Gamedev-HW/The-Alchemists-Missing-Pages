using UnityEngine;
using UnityEngine.UI;

namespace PotionLab
{
    /// <summary>
    /// On phones, draws some panels bigger (the book, the speech bubble, the goal line) so the text is easy to read.
    /// The text is re-rendered at the bigger size instead of being stretched, so it stays sharp.
    /// </summary>
    public class PhoneTextScaler : MonoBehaviour
    {
        [SerializeField, Range(1f, 1.5f)] float scale = 1.2f;
        [SerializeField] RectTransform[] panels = new RectTransform[0];

        void Awake()
        {
            if (!Application.isMobilePlatform || scale <= 1f) return;
            foreach (RectTransform panel in panels) Enlarge(panel);
        }

        void Enlarge(RectTransform panel)
        {
            panel.localScale = Vector3.one * scale;
            foreach (Text text in panel.GetComponentsInChildren<Text>(true))
            {
                text.fontSize = Mathf.RoundToInt(text.fontSize * scale);
                text.resizeTextMinSize = Mathf.RoundToInt(text.resizeTextMinSize * scale);
                text.resizeTextMaxSize = Mathf.RoundToInt(text.resizeTextMaxSize * scale);
                RectTransform box = text.rectTransform;
                box.sizeDelta *= scale;
                box.localScale /= scale;
            }
        }
    }
}
