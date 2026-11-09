using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// The student. Plays the wizard_* sheets through a FramePlayer and knows where the wand tip is on every frame,
    /// so spell beams always start at the wand.
    /// </summary>
    [RequireComponent(typeof(FramePlayer))]
    public class Wizard : MonoBehaviour
    {
        public string idleClip = "wizard_idle";

        FramePlayer player;
        AnimClip current;

        FramePlayer Player
        {
            get
            {
                if (player == null) player = GetComponent<FramePlayer>();
                return player;
            }
        }

        void Start()
        {
            Idle();
        }

        public void Idle()
        {
            Play(idleClip);
        }

        public void Play(string clipName)
        {
            if (Fx.Library == null) return;
            AnimClip c = Fx.Library.Get(clipName);
            if (c == null) return;
            current = c;
            Player.Play(c);
        }

        /// <summary>True while a one-shot animation (cast, happy, oops) is running. Loops never count as "playing".</summary>
        public bool IsPlaying
        {
            get { return current != null && !current.loop && Player.IsPlaying; }
        }

        public int Frame { get { return Player.Frame; } }

        public Vector3 WandTip
        {
            get
            {
                if (current == null || current.wandTip == null || current.wandTip.Length == 0)
                    return transform.position + new Vector3(-2.8f, 6f, 0f);
                int f = Mathf.Clamp(Player.Frame, 0, current.wandTip.Length - 1);
                Vector2 offset = current.wandTip[f];
                return transform.TransformPoint(new Vector3(offset.x, offset.y, 0f));
            }
        }
    }
}
