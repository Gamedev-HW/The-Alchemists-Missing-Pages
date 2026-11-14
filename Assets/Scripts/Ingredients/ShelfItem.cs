using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// An ingredient standing on the shelf. It glows under the mouse and announces when it's clicked;
    /// it doesn't know what happens next (PickUpIngredient listens).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShelfItem : MonoBehaviour, IClickable, IHoverable
    {
        [SerializeField] IngredientData data;
        [Tooltip("Child SpriteRenderer with the ing_*_glow sprite, drawn just behind the item.")]
        [SerializeField] SpriteRenderer glow;
        [SerializeField] ShelfItemEvent clicked;
        [Tooltip("The click area is the sprite's size times this.")]
        [SerializeField, Range(0.3f, 1f)] float clickAreaScale = 0.8f;

        public IngredientData Data => data;

        void Awake()
        {
            if (glow != null) glow.enabled = false;
        }

        public void OnClick()
        {
            if (data != null && clicked != null) clicked.Raise(this);
        }

        public void OnHoverStart()
        {
            if (glow != null) glow.enabled = true;
        }

        public void OnHoverEnd()
        {
            if (glow != null) glow.enabled = false;
        }

#if UNITY_EDITOR
        /// <summary>Choosing an ingredient in the Inspector fills in its sprites and fits the click area.</summary>
        void OnValidate()
        {
            // other components can't be changed during OnValidate, so do it on the editor's next update
            UnityEditor.EditorApplication.delayCall += MatchIngredient;
        }

        void MatchIngredient()
        {
            if (this == null || data == null) return;
            var body = GetComponent<SpriteRenderer>();
            body.sprite = data.shelf;
            if (glow != null) glow.sprite = data.glow;
            var area = GetComponent<BoxCollider2D>();
            if (area != null && body.sprite != null)
            {
                area.size = body.sprite.bounds.size * clickAreaScale;
                area.offset = body.sprite.bounds.center;
            }
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(body);
            if (glow != null) UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(glow);
            if (area != null) UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(area);
        }
#endif
    }
}
