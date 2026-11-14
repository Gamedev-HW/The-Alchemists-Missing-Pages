using System;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// The ingredient floating over the cauldron while you work on it: what it is, how it has been prepared,
    /// and its gentle bobbing. Spawned from the HeldIngredient prefab by PickUpIngredient.
    /// </summary>
    public class HeldIngredient : MonoBehaviour
    {
        [SerializeField] SpriteRenderer art;
        [Tooltip("The magic glow under the ingredient (fx_levitate).")]
        [SerializeField] GameObject levitation;
        [SerializeField] float bobHeight = 0.06f;
        [SerializeField] float bobSpeed = 2f;

        Vector3 home;
        bool bobbing;

        /// <summary>Raised when the preparation changes (after SLICE or HEAT).</summary>
        public event Action Changed;

        public IngredientData Data { get; private set; }
        public Prep Prep { get; private set; }
        public Transform Art => art.transform;

        public void Setup(IngredientData ingredient)
        {
            Data = ingredient;
            Prep = Prep.Raw;
            art.sprite = ingredient.SpriteFor(Prep);
            name = "Held " + ingredient.displayName;
        }

        public void AddPrep(Prep spell)
        {
            Prep |= spell;
            art.sprite = Data.SpriteFor(Prep);
            Changed?.Invoke();
        }

        /// <summary>Stay here and bob up and down.</summary>
        public void FloatAt(Vector3 position)
        {
            home = position;
            transform.position = position;
            bobbing = true;
        }

        /// <summary>Stop bobbing so an action can move it (into the cauldron, for example).</summary>
        public void StopFloating()
        {
            bobbing = false;
            if (levitation != null) levitation.SetActive(false);
        }

        void Update()
        {
            if (bobbing) transform.position = home + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        }
    }
}
