using UnityEngine;
using UnityEngine.UI;

namespace PotionLab
{
    /// <summary>
    /// Makes the game fit any screen: PC windows of any size and shape, phones from 4:3 to 21:9, and phones with a
    /// notch or punch-hole camera (the "safe area"). The 1920x1080 scene is shown as large as possible inside the
    /// safe area with black bars around it, and the UI frame is moved and scaled to sit exactly on top of it.
    /// On phones it also locks the game to landscape, runs it at 60 fps and keeps the screen awake.
    /// Lives on the "Game UI" canvas (menu "Potion Lab ▸ 2" adds it). Works only in Play mode and in builds.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Canvas))]
    public class ScreenFitter : MonoBehaviour
    {
        public const float DesignWidth = 1920f;
        public const float DesignHeight = 1080f;

        [Tooltip("The camera that shows the scene. Empty = the Main Camera.")]
        public Camera worldCamera;
        [Tooltip("The 1920x1080 'Frame' object under this canvas. Every UI element is inside it.")]
        public RectTransform frame;
        [Tooltip("Keep the game clear of notches, punch-hole cameras and rounded corners.")]
        public bool useSafeArea = true;
        [Tooltip("Colour of the bars when the screen isn't 16:9.")]
        public Color barColor = Color.black;

        [Header("Phones and tablets")]
        public bool lockLandscape = true;
        [Tooltip("Android runs at 30 fps unless a game asks for more.")]
        public int frameRate = 60;
        public bool keepScreenOn = true;

        /// <summary>The part of the screen (in pixels) that shows the 16:9 game.</summary>
        public static Rect GameRect { get; private set; }

        Canvas canvas;
        CanvasScaler scaler;
        Camera bars;
        int lastWidth, lastHeight;
        Rect lastSafeArea;

        void Awake()
        {
            if (Application.isMobilePlatform)
            {
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
            canvas = GetComponent<Canvas>();
            scaler = GetComponent<CanvasScaler>();
            Apply();
        }

        void Update()
        {
            // re-fit when the window is resized, the phone turns round or the safe area changes
            if (Screen.width != lastWidth || Screen.height != lastHeight || Screen.safeArea != lastSafeArea) Apply();
        }

        public void Apply()
        {
            int sw = Screen.width;
            int sh = Screen.height;
            if (sw <= 0 || sh <= 0) return;
            lastWidth = sw;
            lastHeight = sh;
            lastSafeArea = Screen.safeArea;

            Rect area = useSafeArea ? Screen.safeArea : new Rect(0f, 0f, sw, sh);
            if (area.width < 2f || area.height < 2f) area = new Rect(0f, 0f, sw, sh);

            // the largest 16:9 rectangle inside the area, centred, in whole pixels
            float aspect = DesignWidth / DesignHeight;
            float w = area.width;
            float h = area.height;
            if (w / h > aspect) w = h * aspect;
            else h = w / aspect;
            w = Mathf.Floor(w);
            h = Mathf.Floor(h);
            var game = new Rect(Mathf.Round(area.x + (area.width - w) * 0.5f), Mathf.Round(area.y + (area.height - h) * 0.5f), w, h);
            GameRect = game;
            float scale = game.height / DesignHeight;

            // ---- the world camera draws only inside the 16:9 rectangle
            Camera cam = worldCamera != null ? worldCamera : Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = DesignHeight / 200f;   // 5.4 = half the scene height at 100 pixels per unit
                cam.rect = new Rect(game.x / sw, game.y / sh, game.width / sw, game.height / sh);
                bool fullScreen = game.width >= sw && game.height >= sh;
                SetBars(cam, !fullScreen);
            }

            // ---- the UI: 1 canvas unit = 1 pixel of the 1920x1080 design, wherever the game sits
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.scaleFactor = scale;
            }
            if (canvas != null) canvas.scaleFactor = scale;   // right away, without waiting for the scaler
            if (frame != null)
            {
                frame.anchorMin = Vector2.zero;
                frame.anchorMax = Vector2.zero;
                frame.pivot = Vector2.zero;
                frame.localScale = Vector3.one;
                frame.sizeDelta = new Vector2(DesignWidth, DesignHeight);
                frame.anchoredPosition = new Vector2(game.x, game.y) / scale;
            }
        }

        /// <summary>A second camera that clears the whole screen to black behind the game (phones show garbage otherwise).</summary>
        void SetBars(Camera cam, bool on)
        {
            if (!on)
            {
                if (bars != null) bars.enabled = false;
                return;
            }
            if (bars == null)
            {
                var go = new GameObject("Letterbox Camera");
                bars = go.AddComponent<Camera>();
                bars.cullingMask = 0;
                bars.clearFlags = CameraClearFlags.SolidColor;
                bars.orthographic = true;
                bars.useOcclusionCulling = false;
                bars.allowHDR = false;
                bars.allowMSAA = false;
            }
            bars.backgroundColor = barColor;
            bars.depth = cam.depth - 1f;
            bars.rect = new Rect(0f, 0f, 1f, 1f);
            bars.enabled = true;
        }

        void OnDestroy()
        {
            if (bars != null) Destroy(bars.gameObject);
        }
    }
}
