using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>Three starter recipes, created only when the project has no recipes at all.</summary>
    public static class SampleRecipes
    {
        public static void Create(string folder, Dictionary<string, IngredientData> ingredients)
        {
            Save(folder + "/1_MoonlightElixir.asset", "Moonlight Elixir",
                "1. POUR the base liquid up to the silver band on the gauge.\n" +
                "2. Take the Moonstone from the top shelf and SLICE it.\n" +
                "3. Send it TO THE POT.\n" +
                "4. MIX it ~~~ times  (an ink blot hides the number).\n" +
                "5. SERVE.",
                2, new Color(0.86f, 0.86f, 1f),
                Step(ingredients, "moonstone", Prep.Sliced));

            Save(folder + "/2_EmberTonic.asset", "Ember Tonic",
                "1. POUR to the silver band.\n" +
                "2. Ember Root - HEAT it, then into the pot.\n" +
                "3. Nightbell Leaves - ~~~~~~~  (the ink has run).\n" +
                "4. Sun Honey, just as it is.\n" +
                "5. MIX three times, then SERVE.\n\n" +
                "The order matters!",
                3, new Color(1f, 0.55f, 0.2f),
                Step(ingredients, "ember_root", Prep.Heated),
                Step(ingredients, "nightbell", Prep.Sliced),
                Step(ingredients, "sun_honey", Prep.Raw));

            Save(folder + "/3_FrostfireBrew.asset", "Frostfire Brew",
                "1. POUR to the silver band.\n" +
                "2. Frost Lily, sliced.\n" +
                "3. Dragon Scale - sliced AND heated.\n" +
                "4. Glowcap Mushrooms, ~~~~~~.\n" +
                "5. A drop of Star Essence to finish.\n" +
                "6. MIX ~~ times and SERVE.\n\n" +
                "The order matters!",
                4, new Color(0.45f, 0.85f, 1f),
                Step(ingredients, "frost_lily", Prep.Sliced),
                Step(ingredients, "dragon_scale", Prep.SlicedHeated),
                Step(ingredients, "glowcap", Prep.Heated),
                Step(ingredients, "star_essence", Prep.Raw));
        }

        static RecipeStep Step(Dictionary<string, IngredientData> ingredients, string id, Prep prep)
        {
            ingredients.TryGetValue(id, out IngredientData ingredient);
            return new RecipeStep { ingredient = ingredient, prep = prep };
        }

        static void Save(string path, string potionName, string bookText, int mixTimes, Color color, params RecipeStep[] steps)
        {
            var recipe = AssetFiles.LoadOrCreate<RecipeData>(path);
            recipe.potionName = potionName;
            recipe.bookText = bookText;
            recipe.mixTimes = mixTimes;
            recipe.potionColor = color;
            recipe.orderMatters = true;
            recipe.steps = new List<RecipeStep>(steps);
            EditorUtility.SetDirty(recipe);
        }
    }
}
