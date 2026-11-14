using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace PotionLab
{
    /// <summary>
    /// The player's input: one pointer for mouse and touch, plus the Back / Esc key. Works with the new Input System
    /// or the old Input Manager (whichever "Active Input Handling" the project uses).
    /// Static because it only reads Unity's input, which is global anyway; it keeps no state of its own.
    /// </summary>
    public static class GameInput
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>Screen position of the mouse, or of the finger on a touch screen.</summary>
        public static Vector2 Position
        {
            get
            {
                var p = Pointer.current;   // the mouse, pen or touch screen that was used last
                return p != null ? p.position.ReadValue() : Vector2.zero;
            }
        }

        /// <summary>Left click or finger down, this frame only.</summary>
        public static bool PressedThisFrame
        {
            get
            {
                var p = Pointer.current;
                return p != null && p.press.wasPressedThisFrame;
            }
        }

        /// <summary>True for a mouse or pen, which can hover. A finger can't, so there's no hover glow on phones.</summary>
        public static bool HasHover
        {
            get
            {
                var p = Pointer.current;
                return p is Mouse || p is Pen;
            }
        }

        /// <summary>Esc on a keyboard, or the Back button / gesture on Android.</summary>
        public static bool BackPressed
        {
            get
            {
                var k = Keyboard.current;
                return k != null && k.escapeKey.wasPressedThisFrame;
            }
        }
#else
        public static Vector2 Position
        {
            get { return Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition; }
        }

        public static bool PressedThisFrame
        {
            get
            {
                if (Input.touchCount > 0) return Input.GetTouch(0).phase == TouchPhase.Began;
                return Input.GetMouseButtonDown(0);
            }
        }

        public static bool HasHover
        {
            get { return Input.mousePresent && Input.touchCount == 0 && !Application.isMobilePlatform; }
        }

        public static bool BackPressed
        {
            get { return Input.GetKeyDown(KeyCode.Escape); }
        }
#endif
    }
}
