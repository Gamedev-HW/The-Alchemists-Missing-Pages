using System.Collections.Generic;
using UnityEngine;

namespace PotionLab
{
    [System.Serializable]
    public class RecipeStep
    {
        public IngredientData ingredient;
        public Prep prep;
    }

    /// <summary>
    /// A potion to brew. SERVE compares the cauldron with this recipe and explains the first thing that is wrong,
    /// so every failed attempt teaches the player something.
    /// </summary>
    [CreateAssetMenu(menuName = "Potion Lab/Recipe")]
    public class RecipeData : ScriptableObject
    {
        public string potionName = "New Potion";
        [TextArea(6, 14)]
        [Tooltip("What the recipe book shows. Leave some parts smudged (~~~) so the player has to experiment.")]
        public string bookText;

        [Header("Base liquid (the silver band on the gauge is 0.40 - 0.62)")]
        [Range(0f, 1f)] public float pourMin = 0.40f;
        [Range(0f, 1f)] public float pourMax = 0.62f;

        [Header("Ingredients")]
        public List<RecipeStep> steps = new List<RecipeStep>();
        public bool orderMatters = true;

        [Header("Stirring")]
        public int mixTimes = 2;

        [Header("Result")]
        public Color potionColor = new Color(0.85f, 0.85f, 1f);

        public bool Check(float level, List<RecipeStep> added, int mixes, out string why)
        {
            if (level < pourMin)
            {
                why = "Not enough base liquid - fill it up to the silver band.";
                return false;
            }
            if (level > pourMax)
            {
                why = "Too much base liquid - it went past the silver band.";
                return false;
            }
            foreach (var step in steps)
            {
                if (step.ingredient == null) continue;
                int index = added.FindIndex(a => a.ingredient == step.ingredient);
                if (index < 0)
                {
                    why = "Something is missing: " + step.ingredient.displayName + ".";
                    return false;
                }
                if (added[index].prep != step.prep)
                {
                    why = step.ingredient.displayName + " should be " + IngredientData.PrepName(step.prep) +
                          ", not " + IngredientData.PrepName(added[index].prep) + ".";
                    return false;
                }
            }
            foreach (var a in added)
            {
                if (a.ingredient == null) continue;
                if (!steps.Exists(s => s.ingredient == a.ingredient))
                {
                    why = a.ingredient.displayName + " doesn't belong in " + potionName + ".";
                    return false;
                }
            }
            if (added.Count != steps.Count)
            {
                why = "An ingredient went in more than once.";
                return false;
            }
            if (orderMatters)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    if (steps[i].ingredient == null) continue;
                    if (added[i].ingredient != steps[i].ingredient)
                    {
                        why = "Right ingredients, wrong order: " + steps[i].ingredient.displayName + " goes in " + Ordinal(i + 1) + ".";
                        return false;
                    }
                }
            }
            if (mixes != mixTimes)
            {
                why = mixes < mixTimes ? "It needed more stirring." : "It was stirred too much.";
                return false;
            }
            why = "";
            return true;
        }

        static string Ordinal(int n)
        {
            switch (n)
            {
                case 1: return "first";
                case 2: return "second";
                case 3: return "third";
                case 4: return "fourth";
                case 5: return "fifth";
                default: return "#" + n;
            }
        }
    }
}
