using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// Sends the held ingredient back to the shelf (with a puff), so the bar shows the cauldron's actions again.
    /// Happens when the cauldron is clicked or Back / Esc is pressed.
    /// </summary>
    public class PutIngredientBack : BrewAction, IBackHandler
    {
        [SerializeField] GameEvent cauldronClicked;
        [SerializeField] Workbench workbench;
        [SerializeField] EffectSpawner effects;
        [SerializeField] EffectSpec poof = new EffectSpec("fx_poof", 66);
        [SerializeField] float pauseAfterSeconds = 0.15f;

        void OnEnable()
        {
            cauldronClicked.Raised += OnCauldronClicked;
        }

        void OnDisable()
        {
            cauldronClicked.Raised -= OnCauldronClicked;
        }

        void OnCauldronClicked()
        {
            if (workbench.IsHolding) Request();
        }

        public bool TryHandleBack()
        {
            return workbench.IsHolding && Request();
        }

        public override IEnumerator Run()
        {
            yield return ReturnHeld();
        }

        /// <summary>Also used by PickUpIngredient when another ingredient is picked while one is held.</summary>
        public IEnumerator ReturnHeld()
        {
            HeldIngredient held = workbench.Release();
            if (held == null) yield break;
            effects.Spawn(poof, held.transform.position);
            Destroy(held.gameObject);
            yield return new WaitForSeconds(pauseAfterSeconds);
        }
    }
}
