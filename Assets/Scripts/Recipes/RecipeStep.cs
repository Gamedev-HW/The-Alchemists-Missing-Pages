using System;

namespace PotionLab
{
    /// <summary>One ingredient in a recipe (or in the cauldron), and how it was prepared.</summary>
    [Serializable]
    public class RecipeStep
    {
        public IngredientData ingredient;
        public Prep prep;
    }
}
