using System;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Base for event assets that carry a value. Unity can't save generic assets, so each value type has a small
    /// subclass (TextEvent, BrewActionEvent, ShelfItemEvent).
    /// </summary>
    public abstract class EventChannel<T> : ScriptableObject
    {
        public event Action<T> Raised;

        public void Raise(T value)
        {
            Raised?.Invoke(value);
        }
    }
}
