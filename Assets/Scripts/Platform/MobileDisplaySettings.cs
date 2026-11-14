using UnityEngine;

namespace PotionLab
{
    /// <summary>Phone and tablet settings: landscape only, a smooth frame rate, and a screen that stays on while playing.</summary>
    public class MobileDisplaySettings : MonoBehaviour
    {
        [SerializeField] bool lockLandscape = true;
        [Tooltip("Android runs games at 30 fps unless they ask for more.")]
        [SerializeField] int frameRate = 60;
        [SerializeField] bool keepScreenOn = true;

        void Awake()
        {
            if (!Application.isMobilePlatform) return;
            if (lockLandscape)
            {
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.orientation = ScreenOrientation.AutoRotation;
            }
            if (frameRate > 0) Application.targetFrameRate = frameRate;
            if (keepScreenOn) Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
