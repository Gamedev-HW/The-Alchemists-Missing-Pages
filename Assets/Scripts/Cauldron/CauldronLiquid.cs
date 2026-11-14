using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Draws the potion inside the cauldron. Listens to the Cauldron and fades smoothly to its new colour,
    /// in when it's filled and out when it's emptied.
    /// </summary>
    public class CauldronLiquid : MonoBehaviour
    {
        [SerializeField] Cauldron cauldron;
        [Tooltip("The liquid and the bubbles: both shown only inside the cauldron's Sprite Mask.")]
        [SerializeField] SpriteRenderer[] parts;
        [Tooltip("How quickly the colour follows a change (higher = faster).")]
        [SerializeField] float fadeSpeed = 6f;

        Color shown;
        Color target;

        void OnEnable()
        {
            cauldron.Changed += OnCauldronChanged;
            OnCauldronChanged();
            shown = target;
            Draw();
        }

        void OnDisable()
        {
            cauldron.Changed -= OnCauldronChanged;
        }

        void OnCauldronChanged()
        {
            target = cauldron.Color;
            target.a = cauldron.HasLiquid ? 1f : 0f;
        }

        void Update()
        {
            if (shown == target) return;
            shown = Color.Lerp(shown, target, 1f - Mathf.Exp(-fadeSpeed * Time.deltaTime));
            Draw();
        }

        void Draw()
        {
            foreach (SpriteRenderer part in parts)
            {
                part.color = shown;
                part.enabled = shown.a > 0.01f;
            }
        }
    }
}
