using System;
using System.Globalization;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// The shape of live-config.json. The game downloads this file when it starts, so editing it on GitHub
    /// changes the game for every player without a new build. Field names must match the JSON keys exactly.
    /// </summary>
    [Serializable]
    public class LiveConfig
    {
        public int version;
        public LiveEvent[] events = new LiveEvent[0];
    }

    /// <summary>A time-limited event: when it runs, what the student announces, the room's colour and a special recipe.</summary>
    [Serializable]
    public class LiveEvent
    {
        public string id;
        public string title;
        [Tooltip("UTC, e.g. 2026-12-01T00:00:00Z")]
        public string startUtc;
        public string endUtc;
        public string startMessage;
        public string successMessage;
        [Tooltip("Hex colour that tints the room while the event runs, e.g. #B8C4FF. Empty = no change.")]
        public string roomTint;
        public LiveRecipe recipe;

        public bool IsActive(DateTime nowUtc)
        {
            return TryParseUtc(startUtc, out DateTime start) && TryParseUtc(endUtc, out DateTime end) && nowUtc >= start && nowUtc < end;
        }

        public static bool TryParseUtc(string text, out DateTime utc)
        {
            return DateTime.TryParse(text, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out utc);
        }
    }

    /// <summary>A recipe written in the JSON. Ingredients are named by their id (moonstone, silver_dew...).</summary>
    [Serializable]
    public class LiveRecipe
    {
        public string potionName;
        public string bookText;
        public float pourMin = 0.40f;
        public float pourMax = 0.62f;
        public bool orderMatters = true;
        public int mixTimes = 2;
        public string color = "#FFFFFF";
        public LiveStep[] steps = new LiveStep[0];
    }

    [Serializable]
    public class LiveStep
    {
        public string ingredient;
        [Tooltip("Raw, Sliced, Heated or SlicedHeated")]
        public string prep = "Raw";
    }
}
