using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// A spell that prepares the held ingredient (SLICE, HEAT...): what it does and how it looks.
    /// A new spell of this kind needs a new asset, not new code.
    /// </summary>
    [CreateAssetMenu(menuName = "Potion Lab/Spell")]
    public class SpellData : ScriptableObject
    {
        [Tooltip("Written in the journal, e.g. SLICE.")]
        public string verb = "SLICE";
        [Tooltip("What the spell does to the ingredient.")]
        public Prep adds = Prep.Sliced;

        [Header("Student")]
        public string castClip = "wizard_cast_spot";
        [Tooltip("The frame of the cast animation on which the beam fires.")]
        public int beamFrame = 4;
        public Color beamColor = new Color(0.75f, 0.88f, 1f);

        [Header("Effects, drawn on the ingredient")]
        public EffectSpec[] effects = { new EffectSpec("fx_slice", 63) };
        [Tooltip("The ingredient changes on this frame of the last effect in the list.")]
        public int changeFrame = 5;
        [Tooltip("Optional effect that stays on the ingredient afterwards, e.g. steam.")]
        public EffectSpec afterEffect = new EffectSpec();
        public float afterEffectSeconds = 3f;

        [Header("Messages ({0} = the ingredient)")]
        public string successMessage = "Clean cut!";
        public string impossibleMessage = "You can't slice {0} - it's a liquid!";
        public string alreadyDoneMessage = "{0} is already sliced.";
    }
}
