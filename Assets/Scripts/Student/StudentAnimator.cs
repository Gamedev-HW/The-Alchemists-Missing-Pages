using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Plays the student's animations and knows where the wand tip is on every frame, so spell beams start at the wand.
    /// </summary>
    [RequireComponent(typeof(FramePlayer))]
    public class StudentAnimator : MonoBehaviour
    {
        [SerializeField] string idleClip = "wizard_idle";
        [Tooltip("Wand position used for clips without wand data, in units from the student's feet.")]
        [SerializeField] Vector2 defaultWandTip = new Vector2(-2.8f, 6f);

        FramePlayer player;

        FramePlayer Player
        {
            get
            {
                if (player == null) player = GetComponent<FramePlayer>();
                return player;
            }
        }

        /// <summary>True while a one-shot animation (cast, happy, oops) runs. Looping clips never count.</summary>
        public bool IsBusy => Player.Clip != null && !Player.Clip.loop && Player.IsPlaying;

        public Vector3 WandTip
        {
            get
            {
                AnimClip clip = Player.Clip;
                Vector2 offset = defaultWandTip;
                if (clip != null && clip.wandTip != null && clip.wandTip.Length > 0)
                    offset = clip.wandTip[Mathf.Clamp(Player.Frame, 0, clip.wandTip.Length - 1)];
                return transform.TransformPoint(offset);
            }
        }

        void Start()
        {
            Idle();
        }

        public void Idle()
        {
            Player.Play(idleClip);
        }

        public void Play(string clipName)
        {
            Player.Play(clipName);
        }

        public IEnumerator WaitForFrame(int frame)
        {
            while (IsBusy && Player.Frame < frame) yield return null;
        }

        public IEnumerator WaitUntilDone()
        {
            while (IsBusy) yield return null;
        }
    }
}
