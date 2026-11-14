using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>TO THE POT: the held ingredient falls into the cauldron and becomes part of the potion.</summary>
    public class DropIntoCauldron : ButtonAction
    {
        [SerializeField] Workbench workbench;
        [SerializeField] Cauldron cauldron;
        [SerializeField] StudentAnimator student;
        [SerializeField] EffectSpawner effects;
        [SerializeField] string castClip = "wizard_cast_pot";
        [Tooltip("The frame of the cast animation on which the ingredient starts to fall.")]
        [SerializeField] int dropFrame = 3;
        [SerializeField] float fallSeconds = 0.45f;
        [SerializeField] float scaleAtSurface = 0.45f;
        [SerializeField] EffectSpec splash = new EffectSpec("fx_splash", 44);
        [Tooltip("Shown instead of the splash when the cauldron is still empty.")]
        [SerializeField] EffectSpec dryLanding = new EffectSpec("fx_poof", 66, 0.6f);

        [Header("Messages ({0} = the ingredient)")]
        [SerializeField] string sinksMessage = "{0} sinks into the potion.";
        [SerializeField] string landsMessage = "{0} lands in the empty cauldron...";

        public override BrewActionId? Button => BrewActionId.DropInPot;

        public override IEnumerator Run()
        {
            HeldIngredient held = workbench.Held;
            if (held == null) yield break;

            student.Play(castClip);
            yield return student.WaitForFrame(dropFrame);
            held.StopFloating();
            yield return Fall(held);

            IngredientData ingredient = held.Data;
            Prep prep = held.Prep;
            workbench.Release();
            Destroy(held.gameObject);

            bool intoLiquid = cauldron.HasLiquid;
            cauldron.AddIngredient(ingredient, prep);
            if (intoLiquid) effects.Spawn(splash, cauldron.SurfacePoint, cauldron.Color);
            else effects.Spawn(dryLanding, cauldron.SurfacePoint);
            Log($"{ingredient.displayName} ({prep.Describe()}) -> into the cauldron");
            Say(string.Format(intoLiquid ? sinksMessage : landsMessage, ingredient.displayName));

            yield return student.WaitUntilDone();
            student.Idle();
        }

        IEnumerator Fall(HeldIngredient held)
        {
            Vector3 from = held.transform.position;
            Vector3 to = cauldron.SurfacePoint;
            float startScale = held.Art.localScale.x;
            for (float t = 0f; t < fallSeconds; t += Time.deltaTime)
            {
                float progress = t / fallSeconds;
                progress *= progress;   // falls faster and faster
                held.transform.position = Vector3.Lerp(from, to, progress);
                held.Art.localScale = Vector3.one * Mathf.Lerp(startScale, scaleAtSurface, progress);
                yield return null;
            }
        }
    }
}
