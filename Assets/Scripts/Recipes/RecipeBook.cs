using System;
using UnityEngine;

namespace PotionLab
{
    /// <summary>The recipes of this level, in order, and which one the player is brewing now.</summary>
    public class RecipeBook : MonoBehaviour
    {
        [Tooltip("Brewed in this order. After the last one the book starts again.")]
        [SerializeField] RecipeData[] recipes = new RecipeData[0];

        int index;

        public event Action Changed;

        public RecipeData Current => recipes.Length > 0 ? recipes[index] : null;
        public int Number => index + 1;
        public int Count => recipes.Length;

        /// <summary>Moves on to the next recipe. Returns true when the book starts over from the first one.</summary>
        public bool Advance()
        {
            if (recipes.Length == 0) return false;
            index = (index + 1) % recipes.Length;
            Changed?.Invoke();
            return index == 0;
        }
    }
}
