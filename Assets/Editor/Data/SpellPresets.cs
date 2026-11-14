using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>The starting values of the two spell assets. After that, edit the assets in the Inspector.</summary>
    public static class SpellPresets
    {
        public static void Slice(SpellData spell)
        {
            spell.verb = "SLICE";
            spell.adds = Prep.Sliced;
            spell.castClip = "wizard_cast_spot";
            spell.beamFrame = 4;
            spell.beamColor = new Color(0.75f, 0.88f, 1f);
            spell.effects = new[] { new EffectSpec("fx_slice", 63) };
            spell.changeFrame = 5;
            spell.afterEffect = new EffectSpec();
            spell.successMessage = "Clean cut!";
            spell.impossibleMessage = "You can't slice {0} - it's a liquid!";
            spell.alreadyDoneMessage = "{0} is already sliced.";
        }

        public static void Heat(SpellData spell)
        {
            spell.verb = "HEAT";
            spell.adds = Prep.Heated;
            spell.castClip = "wizard_cast_spot";
            spell.beamFrame = 4;
            spell.beamColor = new Color(1f, 0.7f, 0.43f);
            // fire behind and in front of the ingredient, so it sits inside the flames
            spell.effects = new[] { new EffectSpec("fx_heat_back", 58, 1f, 0f, 0.2f), new EffectSpec("fx_heat_front", 61, 1f, 0f, 0.2f) };
            spell.changeFrame = 6;
            spell.afterEffect = new EffectSpec("fx_steam", 62, 0.6f, 0f, 0.5f);
            spell.afterEffectSeconds = 3f;
            spell.successMessage = "Nice and hot!";
            spell.impossibleMessage = "{0} just crackles and smokes.";
            spell.alreadyDoneMessage = "{0} is already hot.";
        }
    }
}
