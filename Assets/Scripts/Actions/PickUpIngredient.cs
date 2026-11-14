using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>When a shelf item is clicked, the ingredient flies off the shelf and floats over the cauldron.</summary>
    public class PickUpIngredient : BrewAction
    {
        [SerializeField] ShelfItemEvent shelfItemClicked;
        [SerializeField] Workbench workbench;
        [SerializeField] PutIngredientBack putBack;
        [SerializeField] EffectSpawner effects;
        [SerializeField] HeldIngredient heldPrefab;
        [Tooltip("Where the ingredient floats while you work on it.")]
        [SerializeField] Transform workSpot;

        [Header("Flight")]
        [Tooltip("Where on the shelf item it takes off, from the item's feet.")]
        [SerializeField] Vector3 takeOffOffset = new Vector3(0f, 0.85f, 0f);
        [SerializeField] float flightSeconds = 0.6f;
        [SerializeField] float arcHeight = 1.2f;
        [SerializeField] float scaleOnShelf = 0.55f;
        [SerializeField] float scaleWhenHeld = 0.8f;
        [SerializeField] EffectSpec poof = new EffectSpec("fx_poof", 66);

        ShelfItem chosen;

        void OnEnable()
        {
            shelfItemClicked.Raised += OnShelfItemClicked;
        }

        void OnDisable()
        {
            shelfItemClicked.Raised -= OnShelfItemClicked;
        }

        void OnShelfItemClicked(ShelfItem item)
        {
            if (runner.IsBusy) return;
            chosen = item;
            Request();
        }

        public override IEnumerator Run()
        {
            ShelfItem item = chosen;
            if (workbench.IsHolding) yield return putBack.ReturnHeld();

            Vector3 takeOff = item.transform.position + takeOffOffset;
            effects.Spawn(poof, takeOff);
            HeldIngredient held = Instantiate(heldPrefab, takeOff, Quaternion.identity);
            held.Setup(item.Data);
            workbench.Hold(held);
            yield return Fly(held, takeOff, workSpot.position);
            held.FloatAt(workSpot.position);
        }

        IEnumerator Fly(HeldIngredient held, Vector3 from, Vector3 to)
        {
            for (float t = 0f; t < flightSeconds; t += Time.deltaTime)
            {
                float progress = Mathf.SmoothStep(0f, 1f, t / flightSeconds);
                held.transform.position = Vector3.Lerp(from, to, progress) + Vector3.up * (Mathf.Sin(progress * Mathf.PI) * arcHeight);
                held.Art.localScale = Vector3.one * Mathf.Lerp(scaleOnShelf, scaleWhenHeld, progress);
                yield return null;
            }
            held.Art.localScale = Vector3.one * scaleWhenHeld;
        }
    }
}
