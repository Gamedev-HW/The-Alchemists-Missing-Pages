using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Plays an AnimClip on a SpriteRenderer, frame by frame. Used for the wizard, every effect,
    /// the candle flames, the cauldron fire and the bubbles (no Animator needed).
    /// For scene objects: type the sheet name in "Clip Name" and tick "Play On Start".
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FramePlayer : MonoBehaviour
    {
        [Tooltip("Name of a clip in the Animation Library, e.g. fx_candle_flame.")]
        public string clipName;
        public bool playOnStart;
        [Tooltip("Start looping clips on a random frame, so several candles don't flicker in sync.")]
        public bool randomStart;

        [HideInInspector] public bool destroyWhenDone;

        public System.Action<int> FrameChanged;
        public System.Action Finished;

        AnimClip clip;
        SpriteRenderer sr;
        float time;
        int frame = -1;
        bool playing;

        public AnimClip Clip { get { return clip; } }
        public int Frame { get { return frame; } }
        public bool IsPlaying { get { return playing; } }

        public SpriteRenderer Renderer
        {
            get
            {
                if (sr == null) sr = GetComponent<SpriteRenderer>();
                return sr;
            }
        }

        void Start()
        {
            if (!playOnStart || playing) return;
            AnimClip c = clip;
            if (c == null && Fx.Library != null && !string.IsNullOrEmpty(clipName)) c = Fx.Library.Get(clipName);
            if (c == null) return;
            Play(c);
            if (randomStart && c.loop) time = Random.value * c.Duration;
        }

        public void Play(AnimClip c)
        {
            if (c == null || c.frames == null || c.frames.Length == 0) return;
            clip = c;
            time = 0f;
            frame = -1;
            playing = true;
            Show(0);
        }

        public void Stop()
        {
            playing = false;
        }

        void Update()
        {
            if (!playing || clip == null) return;
            time += Time.deltaTime;
            int count = clip.frames.Length;
            int f = Mathf.FloorToInt(time * clip.fps);
            if (f >= count)
            {
                if (clip.loop)
                {
                    time %= clip.Duration;
                    f = Mathf.Clamp(Mathf.FloorToInt(time * clip.fps), 0, count - 1);
                }
                else
                {
                    Show(count - 1);
                    playing = false;
                    if (Finished != null) Finished();
                    if (destroyWhenDone) Destroy(gameObject);
                    return;
                }
            }
            if (f != frame) Show(f);
        }

        void Show(int f)
        {
            frame = f;
            Renderer.sprite = clip.frames[f];
            if (FrameChanged != null) FrameChanged(f);
        }
    }
}
