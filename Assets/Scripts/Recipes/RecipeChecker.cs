using System.Collections.Generic;

namespace PotionLab
{
    /// <summary>
    /// Compares a cauldron with a recipe and explains the first thing that is wrong, so every failed attempt teaches
    /// the player something. Static because it is a pure calculation: it keeps no state of its own.
    /// </summary>
    public static class RecipeChecker
    {
        static readonly string[] Ordinals = { "first", "second", "third", "fourth", "fifth", "sixth" };

        public static bool Check(RecipeData recipe, Cauldron cauldron, out string problem)
        {
            problem = FindProblem(recipe, cauldron);
            return problem == null;
        }

        static string FindProblem(RecipeData recipe, Cauldron cauldron)
        {
            if (cauldron.Level < recipe.pourMin) return "Not enough base liquid - fill it up to the silver band.";
            if (cauldron.Level > recipe.pourMax) return "Too much base liquid - it went past the silver band.";
            return CheckIngredients(recipe, cauldron.Ingredients) ?? CheckMixing(recipe, cauldron.MixCount);
        }

        static string CheckIngredients(RecipeData recipe, IReadOnlyList<RecipeStep> added)
        {
            foreach (RecipeStep step in recipe.steps)
            {
                if (step.ingredient == null) continue;
                RecipeStep match = Find(added, step.ingredient);
                if (match == null) return "Something is missing: " + step.ingredient.displayName + ".";
                if (match.prep != step.prep)
                    return $"{step.ingredient.displayName} should be {step.prep.Describe()}, not {match.prep.Describe()}.";
            }
            foreach (RecipeStep step in added)
            {
                if (Find(recipe.steps, step.ingredient) == null)
                    return $"{step.ingredient.displayName} doesn't belong in {recipe.potionName}.";
            }
            if (added.Count != recipe.steps.Count) return "An ingredient went in more than once.";
            if (!recipe.orderMatters) return null;
            for (int i = 0; i < recipe.steps.Count; i++)
            {
                IngredientData expected = recipe.steps[i].ingredient;
                if (expected != null && added[i].ingredient != expected)
                    return $"Right ingredients, wrong order: {expected.displayName} goes in {Ordinal(i)}.";
            }
            return null;
        }

        static string CheckMixing(RecipeData recipe, int mixes)
        {
            if (mixes < recipe.mixTimes) return "It needed more stirring.";
            if (mixes > recipe.mixTimes) return "It was stirred too much.";
            return null;
        }

        static RecipeStep Find(IReadOnlyList<RecipeStep> steps, IngredientData ingredient)
        {
            foreach (RecipeStep step in steps)
            {
                if (step.ingredient == ingredient) return step;
            }
            return null;
        }

        static string Ordinal(int index)
        {
            return index < Ordinals.Length ? Ordinals[index] : "#" + (index + 1);
        }
    }
}
