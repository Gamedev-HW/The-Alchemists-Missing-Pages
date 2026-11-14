using UnityEngine;

namespace PotionLab
{
    /// <summary>A line of text, e.g. what the student says or a new journal entry.</summary>
    [CreateAssetMenu(menuName = "Potion Lab/Events/Text Event")]
    public class TextEvent : EventChannel<string>
    {
    }
}
