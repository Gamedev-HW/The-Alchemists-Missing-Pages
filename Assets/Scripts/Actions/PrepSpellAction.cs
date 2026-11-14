using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Casts a preparation spell (SLICE or HEAT) on the held ingredient. One component per spell; the SpellData asset
    /// holds what makes each spell different.
    /// </summary>
    public class PrepSpellAction : ButtonAction
    {
        [SerializeField] BrewActionId button = BrewActionId.Slice;
        [SerializeField] SpellData spell;
        [SerializeField] Workbench workbench;
        [SerializeField] StudentAnimator student;
        [SerializeField] EffectSpawner effects;
        [SerializeField] SpellBeam beam;
        [SerializeField] MishapReaction mishap;

        public override BrewActionId? Button => button;

        public override IEnumerator Run()
        {
            HeldIngredient held = workbench.Held;
            if (held == null) yield break;
            string ingredientName = held.Data.displayName;

            if (!held.Data.Allows(spell.adds))
            {
                string message = string.Format(spell.impossibleMessage, ingredientName);
                yield return mishap.Play(held.transform.position, message, "FAILED: " + message);
                yield break;
            }
            if (held.Prep.Includes(spell.adds))
            {
                Say(string.Format(spell.alreadyDoneMessage, ingredientName));
                yield break;
            }

            student.Play(spell.castClip);
            yield return student.WaitForFrame(spell.beamFrame);
            beam.Show(() => student.WandTip, () => held.transform.position, spell.beamColor);
            FramePlayer[] spawned = SpawnEffects(held.transform.position);
            FramePlayer timing = spawned.Length > 0 ? spawned[spawned.Length - 1] : null;
            yield return EffectSpawner.WaitForFrame(timing, spell.changeFrame);

            held.AddPrep(spell.adds);
            FramePlayer afterEffect = effects.Spawn(spell.afterEffect, held.transform.position, null, held.transform);
            if (afterEffect != null) Destroy(afterEffect.gameObject, spell.afterEffectSeconds);
            Log($"{ingredientName}: {spell.verb} -> {held.Prep.Describe()}");
            Say(spell.successMessage);

            foreach (FramePlayer effect in spawned) yield return EffectSpawner.WaitUntilDone(effect);
            beam.Hide();
            yield return student.WaitUntilDone();
            student.Idle();
        }

        FramePlayer[] SpawnEffects(Vector3 at)
        {
            var spawned = new FramePlayer[spell.effects.Length];
            for (int i = 0; i < spawned.Length; i++) spawned[i] = effects.Spawn(spell.effects[i], at);
            return spawned;
        }
    }
}
