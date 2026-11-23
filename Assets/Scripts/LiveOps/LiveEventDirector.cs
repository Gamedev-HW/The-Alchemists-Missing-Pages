using System;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Runs the live event that is on today (if any): adds its recipe to the book, tints the room and lets the student
    /// announce it. Everything about the event comes from the live config, so a new event needs no new code.
    /// </summary>
    public class LiveEventDirector : MonoBehaviour
    {
        [SerializeField] LiveConfigEvent configLoaded;
        [SerializeField] RecipeBook recipeBook;
        [Tooltip("Every ingredient, so recipes in the config can name them by id.")]
        [SerializeField] IngredientData[] ingredients = new IngredientData[0];

        [Header("Events")]
        [SerializeField] TextEvent speech;
        [SerializeField] TextEvent journal;
        [SerializeField] GameEvent potionBrewed;
        [SerializeField] ColorEvent roomMood;

        [Header("Testing")]
        [Tooltip("Pretend today is this date (UTC), e.g. 2026-12-24, to try an event before it starts. Empty = really today.")]
        [SerializeField] string pretendDate = "";

        LiveEvent running;
        RecipeData eventRecipe;

        void OnEnable()
        {
            configLoaded.Raised += OnConfigLoaded;
            potionBrewed.Raised += OnPotionBrewed;
        }

        void OnDisable()
        {
            configLoaded.Raised -= OnConfigLoaded;
            potionBrewed.Raised -= OnPotionBrewed;
        }

        void OnConfigLoaded(LiveConfig config)
        {
            running = FindRunningEvent(config, Today());
            if (running == null) return;

            journal.Raise("LIVE EVENT: " + running.title);
            AddEventRecipe();
            if (!string.IsNullOrEmpty(running.roomTint) && ColorUtility.TryParseHtmlString(running.roomTint, out Color tint)) roomMood.Raise(tint);
            if (!string.IsNullOrEmpty(running.startMessage)) speech.Raise(running.startMessage);
        }

        void AddEventRecipe()
        {
            if (running.recipe == null || running.recipe.steps.Length == 0) return;
            eventRecipe = LiveRecipeFactory.Create(running.recipe, ingredients, out string problem);
            if (eventRecipe != null) recipeBook.Insert(eventRecipe);
            else Debug.LogWarning($"[Potion Lab] The recipe of live event '{running.id}' was skipped: {problem}.");
        }

        void OnPotionBrewed()
        {
            bool brewedEventRecipe = eventRecipe != null && recipeBook.Current == eventRecipe;
            if (brewedEventRecipe && !string.IsNullOrEmpty(running.successMessage)) speech.Raise(running.successMessage);
        }

        DateTime Today()
        {
            return LiveEvent.TryParseUtc(pretendDate, out DateTime pretend) ? pretend : DateTime.UtcNow;
        }

        /// <summary>The first event in the list whose dates include 'now'.</summary>
        static LiveEvent FindRunningEvent(LiveConfig config, DateTime now)
        {
            if (config.events == null) return null;
            foreach (LiveEvent liveEvent in config.events)
            {
                if (liveEvent != null && liveEvent.IsActive(now)) return liveEvent;
            }
            return null;
        }
    }
}
