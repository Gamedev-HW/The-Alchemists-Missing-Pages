using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Plays an AnimClip on a SpriteRenderer, frame by frame (no Animator needed).
    /// Used by the student, the candles, the cauldron fire and bubbles, and every spell effect.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FramePlayer : MonoBehaviour
    {
        [SerializeField] AnimLibrary library;
        [Tooltip("Clip to play when the scene starts, e.g. fx_candle_flame. Leave empty for objects driven by code.")]
        [SerializeField] string clipName;
        [SerializeField] bool playOnStart;
        [Tooltip("Start looping clips on a random frame, so several candles don't flicker in sync.")]
        [SerializeField] bool randomStart;

        SpriteRenderer spriteRenderer;
        AnimClip clip;
        float time;
        bool destroyWhenDone;

        public AnimClip Clip => clip;
        public int Frame { get; private set; } = -1;
        public bool IsPlaying { get; private set; }

        SpriteRenderer Renderer
        {
            get
            {
                if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
                return spriteRenderer;
            }
        }

        public AnimLibrary Library
        {
            get => library;
            set => library = value;
        }

        void Start()
        {
            if (!playOnStart || IsPlaying || string.IsNullOrEmpty(clipName)) return;
            Play(clipName);
            if (randomStart && clip != null && clip.loop) time = Random.value * clip.Duration;
        }

        public void Play(string animationName)
        {
            if (library != null) Play(library.Get(animationName));
        }

        /// <summary>Plays a clip from the first frame. A one-shot clip can delete its object when it ends (used for effects).</summary>
        public void Play(AnimClip newClip, bool destroyAtEnd = false)
        {
            if (newClip == null || !newClip.HasFrames) return;
            clip = newClip;
            destroyWhenDone = destroyAtEnd && !newClip.loop;
            time = 0f;
            IsPlaying = true;
            ShowFrame(0);
        }

        void Update()
        {
            if (!IsPlaying) return;
            time += Time.deltaTime;
            int frameCount = clip.frames.Length;
            int next = Mathf.FloorToInt(time * clip.fps);
            if (next >= frameCount)
            {
                if (!clip.loop)
                {
                    Finish(frameCount - 1);
                    return;
                }
                time %= clip.Duration;
                next = Mathf.Clamp(Mathf.FloorToInt(time * clip.fps), 0, frameCount - 1);
            }
            if (next != Frame) ShowFrame(next);
        }

        void Finish(int lastFrame)
        {
            ShowFrame(lastFrame);
            IsPlaying = false;
            if (destroyWhenDone) Destroy(gameObject);
        }

        void ShowFrame(int frame)
        {
            Frame = frame;
            Renderer.sprite = clip.frames[frame];
        }
    }
}
