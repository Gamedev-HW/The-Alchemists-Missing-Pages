using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>Creates effects (sparks, splashes, smoke...) from the animation library. One-shot effects delete themselves.</summary>
    public class EffectSpawner : MonoBehaviour
    {
        [SerializeField] AnimLibrary library;

        /// <summary>Spawns the effect at 'target' plus its offset. Returns null if the effect isn't set or isn't in the library.</summary>
        public FramePlayer Spawn(EffectSpec effect, Vector3 target, Color? tint = null, Transform parent = null)
        {
            if (effect == null || !effect.IsSet || library == null) return null;
            AnimClip clip = library.Get(effect.clip);
            if (clip == null) return null;

            var go = new GameObject(effect.clip);
            go.transform.position = new Vector3(target.x + effect.offset.x, target.y + effect.offset.y, 0f);
            go.transform.localScale = new Vector3(effect.scale, effect.scale, 1f);
            if (parent != null) go.transform.SetParent(parent, true);

            var spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = effect.sortingOrder;
            if (tint.HasValue) spriteRenderer.color = tint.Value;

            var player = go.AddComponent<FramePlayer>();
            player.Library = library;
            player.Play(clip, true);
            return player;
        }

        public static void Remove(FramePlayer effect)
        {
            if (effect != null) Destroy(effect.gameObject);
        }

        public static IEnumerator WaitForFrame(FramePlayer effect, int frame)
        {
            while (effect != null && effect.IsPlaying && effect.Frame < frame) yield return null;
        }

        public static IEnumerator WaitUntilDone(FramePlayer effect)
        {
            while (effect != null && effect.IsPlaying) yield return null;
        }
    }
}
