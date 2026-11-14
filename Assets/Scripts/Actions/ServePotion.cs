using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// SERVE: checks the cauldron against the current recipe. A good potion fills the vial and moves on to the next
    /// recipe; a bad one fizzles and the student explains what went wrong. Either way the cauldron is emptied.
    /// </summary>
    public class ServePotion : ButtonAction
    {
        [SerializeField] Cauldron cauldron;
        [SerializeField] PotionVial vial;
        [SerializeField] RecipeBook recipeBook;
        [SerializeField] StudentAnimator student;
        [SerializeField] EffectSpawner effects;
        [SerializeField] SpellBeam beam;
        [SerializeField] MishapReaction mishap;
        [Tooltip("Raised for anything that wants to celebrate or react, e.g. the name panel's badge.")]
        [SerializeField] GameEvent potionBrewed;
        [SerializeField] GameEvent potionFailed;

        [Header("Success")]
        [SerializeField] string castClip = "wizard_cast_pot";
        [SerializeField] int castFrame = 3;
        [Tooltip("Time for the liquid to turn into the potion's colour.")]
        [SerializeField] float colorChangeSeconds = 0.5f;
        [SerializeField] Color beamColor = new Color(1f, 0.86f, 0.55f);
        [SerializeField] EffectSpec sparkle = new EffectSpec("fx_serve", 66);
        [SerializeField] string happyClip = "wizard_happy";
        [SerializeField] float celebrateSeconds = 2.2f;

        [Header("Failure")]
        [SerializeField] Vector3 smokeOffset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] float pauseAfterFailSeconds = 0.5f;

        [Header("Messages")]
        [SerializeField] string emptyMessage = "The cauldron is empty.";
        [SerializeField] string allDoneMessage = "You brewed every recipe in the book! Starting over.";

        public override BrewActionId? Button => BrewActionId.Serve;

        public override IEnumerator Run()
        {
            RecipeData recipe = recipeBook.Current;
            if (!cauldron.HasLiquid || recipe == null)
            {
                Say(emptyMessage);
                yield break;
            }

            if (RecipeChecker.Check(recipe, cauldron, out string problem)) yield return Succeed(recipe);
            else yield return Fail(problem);
            cauldron.Empty();
            student.Idle();
        }

        IEnumerator Succeed(RecipeData recipe)
        {
            student.Play(castClip);
            yield return student.WaitForFrame(castFrame);
            cauldron.SetColor(recipe.potionColor);
            yield return new WaitForSeconds(colorChangeSeconds);

            beam.Show(() => cauldron.SurfacePoint, () => vial.Point, beamColor);
            yield return vial.Fill(recipe.potionColor);
            beam.Hide();
            effects.Spawn(sparkle, vial.Point);
            student.Play(happyClip);
            potionBrewed.Raise();
            Say($"Perfect! {recipe.potionName} is ready!");
            Log($"SUCCESS: {recipe.potionName} brewed!");
            yield return new WaitForSeconds(celebrateSeconds);

            vial.Clear();
            if (recipeBook.Advance()) Say(allDoneMessage);
        }

        IEnumerator Fail(string problem)
        {
            potionFailed.Raise();
            yield return mishap.Play(cauldron.SurfacePoint + smokeOffset, "The potion fizzles... " + problem, "FAILED: " + problem);
            yield return new WaitForSeconds(pauseAfterFailSeconds);
        }
    }
}
