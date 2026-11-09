using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// An ingredient standing on the shelf. Drag an Ingredient asset into "Data" and the shelf sprite and glow
    /// are filled in for you. Clicking it makes the ingredient float over the cauldron.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShelfItem : MonoBehaviour
    {
        public IngredientData data;
        [Tooltip("Child SpriteRenderer with the ing_*_glow sprite, sorting order 10. Shown on hover.")]
        public SpriteRenderer glow;

        void Awake()
        {
            if (glow != null) glow.enabled = false;
            ColliderUtil.Ensure(gameObject, 0.8f);
        }

        public void SetHover(bool on)
        {
            if (glow != null) glow.enabled = on;
        }

        void OnValidate()
        {
            if (data == null) return;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && data.shelf != null) sr.sprite = data.shelf;
            if (glow != null && data.glow != null) glow.sprite = data.glow;
        }
    }
}
