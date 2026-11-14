using System;
using UnityEngine;

namespace PotionLab
{
    /// <summary>One sprite animation: the frames of a sliced sprite sheet and how to play them.</summary>
    [Serializable]
    public class AnimClip
    {
        public string name;
        public Sprite[] frames;
        public float fps = 12f;
        public bool loop;
        [Tooltip("Student sheets only: where the wand tip is on every frame, in units from the student's feet.")]
        public Vector2[] wandTip;

        public bool HasFrames => frames != null && frames.Length > 0;

        public float Duration => !HasFrames || fps <= 0f ? 0f : frames.Length / fps;
    }
}
