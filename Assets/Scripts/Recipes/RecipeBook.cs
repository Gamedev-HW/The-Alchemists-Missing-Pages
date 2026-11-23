using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Adds a recipe (e.g. a live event's) and makes it the current one. Once it's brewed, the book carries on
        /// with the recipe that was current before.
        /// </summary>
        public void Insert(RecipeData recipe)
        {
            var list = new List<RecipeData>(recipes);
            list.Insert(Mathf.Min(index, list.Count), recipe);
            recipes = list.ToArray();
            Changed?.Invoke();
        }

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
