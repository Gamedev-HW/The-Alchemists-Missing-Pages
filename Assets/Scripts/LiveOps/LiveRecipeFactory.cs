using System;
using System.Collections.Generic;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Turns a recipe written in the live config into a real RecipeData, so the rest of the game can't tell it apart
    /// from the recipes in Data/Recipes. Static because it is a pure conversion: it keeps no state.
    /// </summary>
    public static class LiveRecipeFactory
    {
        public static RecipeData Create(LiveRecipe source, IReadOnlyList<IngredientData> ingredients, out string problem)
        {
            problem = null;
            var recipe = ScriptableObject.CreateInstance<RecipeData>();
            recipe.name = string.IsNullOrEmpty(source.potionName) ? "Live Recipe" : source.potionName;
            recipe.potionName = source.potionName;
            recipe.bookText = source.bookText;
            recipe.pourMin = source.pourMin;
            recipe.pourMax = source.pourMax;
            recipe.orderMatters = source.orderMatters;
            recipe.mixTimes = source.mixTimes;
            if (!string.IsNullOrEmpty(source.color) && ColorUtility.TryParseHtmlString(source.color, out Color color)) recipe.potionColor = color;

            foreach (LiveStep step in source.steps)
            {
                IngredientData ingredient = FindIngredient(ingredients, step.ingredient);
                if (ingredient == null)
                {
                    problem = $"unknown ingredient '{step.ingredient}'";
                    return null;
                }
                if (!Enum.TryParse(step.prep, true, out Prep prep))
                {
                    problem = $"unknown preparation '{step.prep}' (use Raw, Sliced, Heated or SlicedHeated)";
                    return null;
                }
                recipe.steps.Add(new RecipeStep { ingredient = ingredient, prep = prep });
            }
            return recipe;
        }

        static IngredientData FindIngredient(IReadOnlyList<IngredientData> ingredients, string id)
        {
            foreach (IngredientData ingredient in ingredients)
            {
                if (ingredient != null && ingredient.id == id) return ingredient;
            }
            return null;
        }
    }
}
