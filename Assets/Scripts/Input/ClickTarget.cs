using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Makes a scene object clickable (it also needs a Collider2D). Clicking only raises an event, e.g. the lectern
    /// raises "Recipe Book Toggled"; whatever listens decides what happens.
    /// </summary>
    public class ClickTarget : MonoBehaviour, IClickable
    {
        [SerializeField] GameEvent clicked;

        public void OnClick()
        {
            if (clicked != null) clicked.Raise();
        }
    }
}
