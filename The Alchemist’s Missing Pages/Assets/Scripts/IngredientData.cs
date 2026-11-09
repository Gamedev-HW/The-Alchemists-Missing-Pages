using UnityEngine;

namespace PotionLab
{
    /// <summary>One ingredient: names, the colour it gives the potion, what you may do with it, and its sprites.</summary>
    [CreateAssetMenu(menuName = "Potion Lab/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        public string id;
        public string displayName;
        public string hebrewName;
        [Tooltip("The potion takes on some of this colour when the ingredient goes into the cauldron.")]
        public Color liquidTint = Color.white;
        [Tooltip("Liquids can't be sliced: trying it fails with smoke (a consistent, learnable reaction).")]
        public bool canSlice = true;
        public bool canHeat = true;

        [Header("Sprites")]
        public Sprite shelf;
        public Sprite glow;
        public Sprite raw;
        public Sprite sliced;
        public Sprite heated;
        public Sprite slicedHeated;

        public Sprite SpriteFor(Prep prep)
        {
            switch (prep)
            {
                case Prep.Sliced: return sliced != null ? sliced : raw;
                case Prep.Heated: return heated != null ? heated : raw;
                case Prep.SlicedHeated: return slicedHeated != null ? slicedHeated : raw;
                default: return raw;
            }
        }

        public static string PrepName(Prep prep)
        {
            switch (prep)
            {
                case Prep.Sliced: return "sliced";
                case Prep.Heated: return "heated";
                case Prep.SlicedHeated: return "sliced and heated";
                default: return "raw";
            }
        }
    }
}
