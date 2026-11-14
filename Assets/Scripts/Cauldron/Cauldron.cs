using System;
using System.Collections.Generic;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// What is in the cauldron: how full it is, its colour, the ingredients that went in and how often it was stirred.
    /// It only stores this state and raises Changed; the liquid, the gauge and the UI listen and draw it.
    /// </summary>
    public class Cauldron : MonoBehaviour
    {
        [Tooltip("The liquid's surface. Ingredients drop here and effects appear here.")]
        [SerializeField] Transform surface;
        [SerializeField] Color emptyColor = new Color(0.55f, 0.8f, 1f);
        [Tooltip("How much of an ingredient's colour the potion takes on.")]
        [SerializeField, Range(0f, 1f)] float ingredientTint = 0.45f;
        [Tooltip("Below this level the cauldron counts as empty.")]
        [SerializeField] float emptyLevel = 0.01f;

        readonly List<RecipeStep> ingredients = new List<RecipeStep>();

        public event Action Changed;

        public float Level { get; private set; }
        public int MixCount { get; private set; }
        public Color Color { get; private set; }
        public IReadOnlyList<RecipeStep> Ingredients => ingredients;
        public bool HasLiquid => Level > emptyLevel;
        public bool IsFull => Level >= 1f;
        public Vector3 SurfacePoint => surface != null ? surface.position : transform.position;

        void Awake()
        {
            Color = emptyColor;
        }

        public void AddLiquid(float amount, Color liquidColor)
        {
            if (!HasLiquid) Color = liquidColor;
            Level = Mathf.Clamp01(Level + amount);
            Changed?.Invoke();
        }

        public void AddIngredient(IngredientData ingredient, Prep prep)
        {
            ingredients.Add(new RecipeStep { ingredient = ingredient, prep = prep });
            if (HasLiquid) Color = Color.Lerp(Color, ingredient.liquidTint, ingredientTint);
            Changed?.Invoke();
        }

        public void Mix()
        {
            MixCount++;
            Changed?.Invoke();
        }

        public void SetColor(Color color)
        {
            Color = color;
            Changed?.Invoke();
        }

        public void Empty()
        {
            Level = 0f;
            MixCount = 0;
            ingredients.Clear();
            Color = emptyColor;
            Changed?.Invoke();
        }
    }
}
