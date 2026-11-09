using System.Collections.Generic;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Every sprite animation of the pack (wizard + effects), filled automatically by
    /// "Potion Lab ▸ 1 - Set Up Assets". Look clips up by sheet name, e.g. Get("fx_slice").
    /// </summary>
    [CreateAssetMenu(menuName = "Potion Lab/Animation Library")]
    public class AnimLibrary : ScriptableObject
    {
        public List<AnimClip> clips = new List<AnimClip>();

        Dictionary<string, AnimClip> lookup;

        public AnimClip Get(string clipName)
        {
            if (lookup == null || lookup.Count != clips.Count)
            {
                lookup = new Dictionary<string, AnimClip>();
                foreach (var c in clips)
                    if (c != null && !string.IsNullOrEmpty(c.name)) lookup[c.name] = c;
            }
            AnimClip clip;
            if (!lookup.TryGetValue(clipName, out clip))
                Debug.LogWarning("[Potion Lab] There is no animation called '" + clipName + "' in " + name +
                                 ". Run the menu 'Potion Lab > 1 - Set Up Assets' again.");
            return clip;
        }
    }
}
