using UnityEngine;

namespace PotionLab
{
    /// <summary>How an ingredient has been prepared.</summary>
    public enum Prep { Raw, Sliced, Heated, SlicedHeated }

    /// <summary>What the three bar buttons control: the cauldron (POUR / MIX / SERVE) or a floating ingredient (SLICE / HEAT / TO THE POT).</summary>
    public enum Mode { Pot, Ingredient }

    /// <summary>Sorting orders for everything that is spawned from code. Scene objects use the table in GUIDE.md.</summary>
    public static class Order
    {
        public const int KettleGlow = 48;
        public const int Stream = 49;
        public const int Splash = 44;
        public const int Mix = 44;
        public const int HeatBack = 58;
        public const int Levitate = 59;
        public const int Floating = 60;
        public const int HeatFront = 61;
        public const int Steam = 62;
        public const int Slice = 63;
        public const int Beam = 64;
        public const int WandTip = 65;
        public const int Burst = 66;
    }

    /// <summary>Converts positions from the 1920x1080 art layout (pixels, origin bottom-left) to Unity world units.</summary>
    public static class Px
    {
        public static Vector3 ToWorld(float x, float y)
        {
            return new Vector3((x - 960f) / 100f, (y - 540f) / 100f, 0f);
        }
    }

    /// <summary>One sprite animation (a sliced sprite sheet).</summary>
    [System.Serializable]
    public class AnimClip
    {
        public string name;
        public Sprite[] frames;
        public float fps = 12f;
        public bool loop;

        [Tooltip("Wizard sheets only: offset of the wand tip from the wizard's pivot, in units, for every frame.")]
        public Vector2[] wandTip;

        public float Duration
        {
            get { return frames == null || frames.Length == 0 || fps <= 0f ? 0f : frames.Length / fps; }
        }
    }

    public static class ColliderUtil
    {
        /// <summary>Adds a BoxCollider2D that fits the sprite (scaled by 'shrink') if the object has no collider yet.</summary>
        public static void Ensure(GameObject go, float shrink = 0.85f)
        {
            if (go.GetComponent<Collider2D>() != null) return;
            var sr = go.GetComponent<SpriteRenderer>();
            var box = go.AddComponent<BoxCollider2D>();
            if (sr != null && sr.sprite != null)
            {
                Bounds b = sr.sprite.bounds;
                box.size = new Vector2(b.size.x * shrink, b.size.y * shrink);
                box.offset = b.center;
            }
        }
    }
}
