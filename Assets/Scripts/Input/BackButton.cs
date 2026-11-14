using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Back on Android / Esc on PC. Asks its handlers in order and stops at the first one that uses the press:
    /// close the book, else put the ingredient back, else (pressed twice) quit.
    /// </summary>
    public class BackButton : MonoBehaviour
    {
        [Tooltip("Components that implement IBackHandler, in the order they are asked.")]
        [SerializeField] MonoBehaviour[] handlers = new MonoBehaviour[0];

        void Update()
        {
            if (!GameInput.BackPressed) return;
            foreach (MonoBehaviour handler in handlers)
            {
                if (handler is IBackHandler backHandler && backHandler.TryHandleBack()) return;
            }
        }

        void OnValidate()
        {
            foreach (MonoBehaviour handler in handlers)
            {
                if (handler != null && !(handler is IBackHandler))
                    Debug.LogWarning($"[Potion Lab] {handler.GetType().Name} can't handle Back: it isn't an IBackHandler.", this);
            }
        }
    }
}
