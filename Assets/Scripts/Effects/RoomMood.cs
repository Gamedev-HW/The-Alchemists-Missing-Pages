using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>Tints the room (e.g. cold moonlight during a live event), fading smoothly to the new colour.</summary>
    public class RoomMood : MonoBehaviour
    {
        [SerializeField] ColorEvent roomMood;
        [SerializeField] SpriteRenderer[] tinted = new SpriteRenderer[0];
        [SerializeField] float fadeSeconds = 2f;

        Coroutine fading;

        void OnEnable()
        {
            roomMood.Raised += FadeTo;
        }

        void OnDisable()
        {
            roomMood.Raised -= FadeTo;
        }

        void FadeTo(Color tint)
        {
            if (fading != null) StopCoroutine(fading);
            fading = StartCoroutine(Fade(tint));
        }

        IEnumerator Fade(Color tint)
        {
            var from = new Color[tinted.Length];
            for (int i = 0; i < tinted.Length; i++) from[i] = tinted[i].color;
            for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
            {
                for (int i = 0; i < tinted.Length; i++) tinted[i].color = Color.Lerp(from[i], tint, t / fadeSeconds);
                yield return null;
            }
            foreach (SpriteRenderer sprite in tinted) sprite.color = tint;
            fading = null;
        }
    }
}
