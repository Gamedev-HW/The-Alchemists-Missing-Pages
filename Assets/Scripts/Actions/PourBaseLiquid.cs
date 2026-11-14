using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>POUR: the kettle flies in and pours base liquid until the player presses STOP or the cauldron is full.</summary>
    public class PourBaseLiquid : ButtonAction
    {
        [SerializeField] Cauldron cauldron;
        [SerializeField] Kettle kettle;
        [SerializeField] RecipeBook recipeBook;
        [SerializeField] StudentAnimator student;
        [SerializeField] EffectSpawner effects;
        [SerializeField] SpellBeam beam;
        [SerializeField] string castClip = "wizard_cast_pot";
        [SerializeField] Color beamColor = new Color(0.75f, 0.88f, 1f);

        [Header("Liquid")]
        [Tooltip("How much of the gauge fills per second.")]
        [SerializeField] float pourSpeed = 0.3f;
        [SerializeField] Color liquidColor = new Color(0.55f, 0.8f, 1f);
        [SerializeField] EffectSpec stream = new EffectSpec("fx_pour_stream", 49);
        [SerializeField] float streamWidth = 0.5f;
        [Tooltip("Length of one stream frame in units; the stream is stretched to reach the surface.")]
        [SerializeField] float streamFrameLength = 3.84f;
        [SerializeField] float shortestStream = 0.2f;
        [SerializeField] EffectSpec splash = new EffectSpec("fx_splash", 44, 0.6f, 0.2f);
        [SerializeField] float secondsBetweenSplashes = 0.5f;

        [Header("Messages")]
        [SerializeField] string tooMuchMessage = "Whoa, that's a lot of liquid...";
        [SerializeField] string tooLittleMessage = "Is that enough liquid?";
        [SerializeField] string filledMessage = "The cauldron is filled.";
        [Tooltip("{0} = how full the cauldron is, in percent.")]
        [SerializeField] string journalLine = "POUR -> base liquid at {0}%";

        bool stopRequested;

        public override BrewActionId? Button => BrewActionId.Pour;
        public override BrewActionId? StopButton => BrewActionId.StopPour;

        protected override void OnButtonPressed(BrewActionId id)
        {
            if (id == BrewActionId.StopPour) stopRequested = true;
            else base.OnButtonPressed(id);
        }

        public override IEnumerator Run()
        {
            stopRequested = false;
            student.Play(castClip);
            yield return kettle.FlyIn();
            beam.Show(() => student.WandTip, () => kettle.transform.position, beamColor);
            yield return kettle.TiltToPour();

            FramePlayer pouring = SpawnStream();
            yield return PourUntilStopped();
            EffectSpawner.Remove(pouring);

            beam.Hide();
            yield return kettle.TiltBack();
            yield return kettle.FlyOut();
            student.Idle();
            Report();
        }

        FramePlayer SpawnStream()
        {
            FramePlayer effect = effects.Spawn(stream, kettle.SpoutTip, liquidColor);
            if (effect != null)
            {
                float drop = Mathf.Max(shortestStream, kettle.SpoutTip.y - cauldron.SurfacePoint.y);
                effect.transform.localScale = new Vector3(streamWidth, drop / streamFrameLength, 1f);
            }
            return effect;
        }

        IEnumerator PourUntilStopped()
        {
            float untilSplash = 0f;
            while (!stopRequested && !cauldron.IsFull)
            {
                cauldron.AddLiquid(pourSpeed * Time.deltaTime, liquidColor);
                untilSplash -= Time.deltaTime;
                if (untilSplash <= 0f)
                {
                    effects.Spawn(splash, cauldron.SurfacePoint, cauldron.Color);
                    untilSplash = secondsBetweenSplashes;
                }
                yield return null;
            }
        }

        void Report()
        {
            Log(string.Format(journalLine, Mathf.RoundToInt(cauldron.Level * 100f)));
            RecipeData recipe = recipeBook.Current;
            if (recipe != null && cauldron.Level > recipe.pourMax) Say(tooMuchMessage);
            else if (recipe != null && cauldron.Level < recipe.pourMin) Say(tooLittleMessage);
            else Say(filledMessage);
        }
    }
}
