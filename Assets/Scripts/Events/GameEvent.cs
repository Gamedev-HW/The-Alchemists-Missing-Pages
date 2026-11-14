using System;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// A message with no data, saved as an asset (for example "Cauldron Clicked").
    /// The sender raises it without knowing who listens; listeners subscribe in OnEnable and unsubscribe in OnDisable.
    /// </summary>
    [CreateAssetMenu(menuName = "Potion Lab/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        public event Action Raised;

        public void Raise()
        {
            Raised?.Invoke();
        }
    }
}
