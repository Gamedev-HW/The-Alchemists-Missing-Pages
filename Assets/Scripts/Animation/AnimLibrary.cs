using System.Collections.Generic;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Every sprite animation in the game (student and effects), looked up by sheet name, e.g. Get("fx_slice").
    /// Filled by "Potion Lab > 1 - Set Up Assets".
    /// </summary>
    [CreateAssetMenu(menuName = "Potion Lab/Animation Library")]
    public class AnimLibrary : ScriptableObject
    {
        public List<AnimClip> clips = new List<AnimClip>();

        Dictionary<string, AnimClip> byName;

        public AnimClip Get(string clipName)
        {
            if (byName == null || byName.Count != clips.Count) BuildLookup();
            if (byName.TryGetValue(clipName, out AnimClip clip)) return clip;
            Debug.LogWarning($"[Potion Lab] {name} has no animation called '{clipName}'.");
            return null;
        }

        void BuildLookup()
        {
            byName = new Dictionary<string, AnimClip>();
            foreach (AnimClip clip in clips)
            {
                if (clip != null && !string.IsNullOrEmpty(clip.name)) byName[clip.name] = clip;
            }
        }
    }
}
