using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>MIX: the student stirs the cauldron with a swirl of magic. Each press counts as one stir.</summary>
    public class MixPotion : ButtonAction
    {
        [SerializeField] Cauldron cauldron;
        [SerializeField] StudentAnimator student;
        [SerializeField] EffectSpawner effects;
        [SerializeField] SpellBeam beam;
        [SerializeField] string stirClip = "wizard_stir";
        [SerializeField] float mixSeconds = 1.6f;
        [SerializeField] Color beamColor = new Color(0.55f, 1f, 0.86f);
        [Tooltip("The beam aims this far above the surface.")]
        [SerializeField] Vector3 beamAimOffset = new Vector3(0f, 0.6f, 0f);
        [SerializeField] EffectSpec swirl = new EffectSpec("fx_mix", 44);
        [SerializeField] string emptyMessage = "There's nothing to stir yet - POUR first.";

        public override BrewActionId? Button => BrewActionId.Mix;

        public override IEnumerator Run()
        {
            if (!cauldron.HasLiquid)
            {
                Say(emptyMessage);
                yield break;
            }

            student.Play(stirClip);
            beam.Show(() => student.WandTip, () => cauldron.SurfacePoint + beamAimOffset, beamColor);
            FramePlayer effect = effects.Spawn(swirl, cauldron.SurfacePoint);
            yield return new WaitForSeconds(mixSeconds);
            EffectSpawner.Remove(effect);
            beam.Hide();
            student.Idle();

            cauldron.Mix();
            int times = cauldron.MixCount;
            Log($"MIX -> stirred {times}x");
            Say(times == 1 ? "Stirred 1 time." : $"Stirred {times} times.");
        }
    }
}
