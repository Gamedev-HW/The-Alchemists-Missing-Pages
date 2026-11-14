using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PotionLab
{
    /// <summary>
    /// Finds what the mouse or finger is on in the scene and tells it: OnClick for IClickable, hover for IHoverable.
    /// Clicks on the UI (buttons, the open book) never reach the scene.
    /// </summary>
    public class WorldPointer : MonoBehaviour
    {
        [SerializeField] Camera worldCamera;

        readonly List<RaycastResult> uiHits = new List<RaycastResult>();
        PointerEventData uiPointer;
        EventSystem uiPointerOwner;
        IHoverable hovered;

        void Update()
        {
            bool pressed = GameInput.PressedThisFrame;
            bool canHover = GameInput.HasHover;
            // a mouse is checked every frame for hover; a finger only when it touches the screen
            Collider2D hit = pressed || canHover ? ColliderUnderPointer() : null;

            SetHovered(canHover && hit != null ? hit.GetComponent<IHoverable>() : null);
            if (pressed && hit != null) hit.GetComponent<IClickable>()?.OnClick();
        }

        Collider2D ColliderUnderPointer()
        {
            Vector2 screen = GameInput.Position;
            if (worldCamera == null || IsOverUI(screen)) return null;
            Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
            return Physics2D.OverlapPoint(world);
        }

        /// <summary>Asks the UI directly, so it works the same for mouse and touch, even on the frame a finger lands.</summary>
        bool IsOverUI(Vector2 screen)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            if (uiPointer == null || uiPointerOwner != eventSystem)
            {
                uiPointer = new PointerEventData(eventSystem);
                uiPointerOwner = eventSystem;
            }
            uiPointer.position = screen;
            eventSystem.RaycastAll(uiPointer, uiHits);
            return uiHits.Count > 0;
        }

        void SetHovered(IHoverable next)
        {
            if (next == hovered) return;
            hovered?.OnHoverEnd();
            hovered = next;
            hovered?.OnHoverStart();
        }
    }
}
