using UnityEngine;

namespace PotionLab
{
    /// <summary>A colour, e.g. a new tint for the room during a live event.</summary>
    [CreateAssetMenu(menuName = "Potion Lab/Events/Color Event")]
    public class ColorEvent : EventChannel<Color>
    {
    }
}
