using UnityEngine;

namespace PotionLab
{
    /// <summary>One of the bar buttons was pressed (SLICE, POUR, STOP...).</summary>
    [CreateAssetMenu(menuName = "Potion Lab/Events/Brew Action Event")]
    public class BrewActionEvent : EventChannel<BrewActionId>
    {
    }
}
