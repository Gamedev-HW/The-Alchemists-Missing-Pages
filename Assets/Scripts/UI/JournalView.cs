using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PotionLab
{
    /// <summary>The journal page in the recipe book: every attempt, newest on top.</summary>
    public class JournalView : MonoBehaviour
    {
        [SerializeField] TextEvent journal;
        [SerializeField] Text page;
        [SerializeField] int maxLines = 14;
        [SerializeField] string emptyText = "(nothing yet - every attempt is written here)";

        readonly List<string> lines = new List<string>();

        void Awake()
        {
            page.text = emptyText;
        }

        void OnEnable()
        {
            journal.Raised += Add;
        }

        void OnDisable()
        {
            journal.Raised -= Add;
        }

        void Add(string line)
        {
            lines.Insert(0, line);
            if (lines.Count > maxLines) lines.RemoveAt(lines.Count - 1);
            page.text = string.Join("\n", lines);
        }
    }
}
