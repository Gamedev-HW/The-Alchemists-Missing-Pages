using UnityEngine;

namespace PotionLab
{
    /// <summary>Spawns effects from the Animation Library. One-shot effects delete themselves when they finish.</summary>
    public static class Fx
    {
        public static AnimLibrary Library;

        public static FramePlayer Spawn(string clipName, Vector3 position, int order, Color? tint = null,
            float scale = 1f, Transform parent = null)
        {
            if (Library == null)
            {
                Debug.LogWarning("[Potion Lab] No Animation Library: assign it on the PotionGame object.");
                return null;
            }
            AnimClip clip = Library.Get(clipName);
            if (clip == null) return null;

            var go = new GameObject(clipName);
            go.transform.position = new Vector3(position.x, position.y, 0f);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            if (parent != null) go.transform.SetParent(parent, true);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            if (tint.HasValue) sr.color = tint.Value;

            var player = go.AddComponent<FramePlayer>();
            player.destroyWhenDone = !clip.loop;
            player.Play(clip);
            return player;
        }
    }
}
