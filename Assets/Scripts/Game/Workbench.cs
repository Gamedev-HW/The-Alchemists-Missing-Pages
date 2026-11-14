using System;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Which ingredient the player is working on, if any. When nothing is held, the cauldron is the target.
    /// Raises Changed when the held ingredient changes or is prepared, so the bar and the name panel can update.
    /// </summary>
    public class Workbench : MonoBehaviour
    {
        public event Action Changed;

        public HeldIngredient Held { get; private set; }
        public bool IsHolding => Held != null;

        public void Hold(HeldIngredient ingredient)
        {
            Release();
            Held = ingredient;
            Held.Changed += OnHeldChanged;
            Changed?.Invoke();
        }

        /// <summary>Lets go of the held ingredient and returns it; the caller decides what happens to it.</summary>
        public HeldIngredient Release()
        {
            HeldIngredient released = Held;
            if (released == null) return null;
            released.Changed -= OnHeldChanged;
            Held = null;
            Changed?.Invoke();
            return released;
        }

        void OnHeldChanged()
        {
            Changed?.Invoke();
        }
    }
}
