using UnityEngine;

namespace PotionLab
{
    /// <summary>Makes a scene object clickable: the cauldron (switches the bar to POUR / MIX / SERVE) or the recipe book.</summary>
    public class ClickTarget : MonoBehaviour
    {
        public enum Kind { Cauldron, RecipeBook }
        public Kind kind;

        void Awake()
        {
            ColliderUtil.Ensure(gameObject, 0.85f);
        }
    }
}
