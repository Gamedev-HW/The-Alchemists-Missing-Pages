using System;
using UnityEngine;

namespace PotionLab
{
    /// <summary>How to show one effect: which animation, drawn in front of what, how big, and where relative to its target.</summary>
    [Serializable]
    public class EffectSpec
    {
        public string clip;
        public int sortingOrder;
        public float scale = 1f;
        public Vector2 offset;

        public EffectSpec()
        {
        }

        public EffectSpec(string clip, int sortingOrder, float scale = 1f, float offsetX = 0f, float offsetY = 0f)
        {
            this.clip = clip;
            this.sortingOrder = sortingOrder;
            this.scale = scale;
            offset = new Vector2(offsetX, offsetY);
        }

        public bool IsSet => !string.IsNullOrEmpty(clip);
    }
}
