using System.Collections.Generic;
using UnityEngine;

namespace PotionLab
{
    /// <summary>A potion to brew: what the recipe book shows, and the right answer that SERVE compares against.</summary>
    [CreateAssetMenu(menuName = "Potion Lab/Recipe")]
    public class RecipeData : ScriptableObject
    {
        public string potionName = "New Potion";
        [TextArea(6, 14)]
        [Tooltip("What the recipe book shows. Smudge some parts (~~~) so the player has to experiment.")]
        public string bookText;

        [Header("Base liquid (the silver band on the gauge is 0.40 - 0.62)")]
        [Range(0f, 1f)] public float pourMin = 0.40f;
        [Range(0f, 1f)] public float pourMax = 0.62f;

        [Header("Ingredients")]
        public List<RecipeStep> steps = new List<RecipeStep>();
        public bool orderMatters = true;

        [Header("Mixing")]
        public int mixTimes = 2;

        [Header("Result")]
        public Color potionColor = new Color(0.85f, 0.85f, 1f);
    }
}
