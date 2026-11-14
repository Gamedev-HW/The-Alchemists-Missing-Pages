using UnityEngine;

namespace PotionLab
{
    /// <summary>One ingredient: its names, the colour it gives a potion, which spells work on it, and its sprites.</summary>
    [CreateAssetMenu(menuName = "Potion Lab/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        public string id;
        public string displayName;
        public string hebrewName;
        [Tooltip("The potion takes on some of this colour when the ingredient goes into the cauldron.")]
        public Color liquidTint = Color.white;
        [Tooltip("Which spells work on it. Liquids can't be sliced: trying fails with smoke, the same way every time.")]
        public Prep canBe = Prep.Sliced | Prep.Heated;

        [Header("Sprites")]
        public Sprite shelf;
        public Sprite glow;
        public Sprite raw;
        public Sprite sliced;
        public Sprite heated;
        public Sprite slicedHeated;

        public bool Allows(Prep prep)
        {
            return canBe.Includes(prep);
        }

        public Sprite SpriteFor(Prep prep)
        {
            Sprite sprite = null;
            switch (prep)
            {
                case Prep.Sliced: sprite = sliced; break;
                case Prep.Heated: sprite = heated; break;
                case Prep.SlicedHeated: sprite = slicedHeated; break;
            }
            return sprite != null ? sprite : raw;
        }
    }
}
