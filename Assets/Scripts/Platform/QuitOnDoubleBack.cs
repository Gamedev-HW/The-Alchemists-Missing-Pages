using UnityEngine;

namespace PotionLab
{
    /// <summary>The last Back / Esc handler: the first press asks "press again to quit", a second quick press quits.</summary>
    public class QuitOnDoubleBack : MonoBehaviour, IBackHandler
    {
        [SerializeField] TextEvent speech;
        [Tooltip("The second press must come within this many seconds.")]
        [SerializeField] float secondPressWindow = 2f;
        [SerializeField] string desktopHint = "Press Esc again to quit.";
        [SerializeField] string mobileHint = "Press Back again to quit.";

        float lastPress = float.NegativeInfinity;

        public bool TryHandleBack()
        {
            if (Application.isEditor) return false;
            if (Time.unscaledTime - lastPress < secondPressWindow)
            {
                Application.Quit();
                return true;
            }
            lastPress = Time.unscaledTime;
            speech.Raise(Application.isMobilePlatform ? mobileHint : desktopHint);
            return true;
        }
    }
}
