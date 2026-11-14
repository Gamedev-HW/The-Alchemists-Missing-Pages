using UnityEngine;

namespace PotionLab
{
    /// <summary>The wand mouse cursor. Phones have no cursor, so it's skipped there.</summary>
    public class DesktopCursor : MonoBehaviour
    {
        [SerializeField] Texture2D cursor;
        [Tooltip("The pixel of the image that does the clicking (the wand's tip).")]
        [SerializeField] Vector2 hotspot = new Vector2(6f, 6f);

        void Start()
        {
            if (cursor != null && !Application.isMobilePlatform) Cursor.SetCursor(cursor, hotspot, CursorMode.Auto);
        }
    }
}
