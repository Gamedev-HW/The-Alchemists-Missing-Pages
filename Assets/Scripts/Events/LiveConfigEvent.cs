using UnityEngine;

namespace PotionLab
{
    /// <summary>The live config has been loaded (online, from the saved copy, or the built-in one).</summary>
    [CreateAssetMenu(menuName = "Potion Lab/Events/Live Config Event")]
    public class LiveConfigEvent : EventChannel<LiveConfig>
    {
    }
}
