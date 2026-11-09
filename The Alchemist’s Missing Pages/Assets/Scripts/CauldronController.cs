using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// The cauldron station: liquid level and colour, what went in, how often it was stirred,
    /// plus the pour gauge, the kettle and the vial that belong to it.
    /// </summary>
    public class CauldronController : MonoBehaviour
    {
        [Header("Parts (drag the scene objects here)")]
        [Tooltip("SpriteRenderer with the white 'liquid' sprite. It is shown only inside the cauldron mask.")]
        public SpriteRenderer liquid;
        [Tooltip("SpriteRenderer + FramePlayer (fx_bubbles) on the liquid surface.")]
        public SpriteRenderer bubbles;
        [Tooltip("The gauge_fill object (pivot at the bottom). Its Y scale shows the level.")]
        public Transform gaugeFill;
        [Tooltip("The kettle. It is hidden until POUR and flies in from the top right.")]
        public Transform kettle;
        [Tooltip("SpriteRenderer with the white 'vial_fill' sprite inside the vial.")]
        public SpriteRenderer vialFill;

        [Header("Kettle")]
        [Tooltip("Spout tip in the kettle's local units (from its centre).")]
        public Vector2 spoutLocal = new Vector2(-1.04f, 0.42f);
        public float pourTilt = 28f;

        [Header("Colours")]
        public Color startColor = new Color(0.55f, 0.8f, 1f);

        public float Level { get; private set; }
        public int Mixes { get; private set; }
        public readonly List<RecipeStep> Contents = new List<RecipeStep>();
        public Color CurrentColor { get; private set; }
        public Vector3 KettleHome { get; private set; }

        public bool HasLiquid { get { return Level > 0.01f; } }

        public Vector3 SurfacePoint
        {
            get { return liquid != null ? liquid.transform.position : transform.position + new Vector3(0f, 0.74f, 0f); }
        }

        public Vector3 VialPoint
        {
            get { return vialFill != null ? vialFill.transform.position : transform.position + new Vector3(3.3f, -0.66f, 0f); }
        }

        public Vector3 Spout
        {
            get { return kettle != null ? kettle.TransformPoint(spoutLocal) : SurfacePoint + Vector3.up * 1.8f; }
        }

        float liquidAlpha = 1f;

        void Awake()
        {
            CurrentColor = startColor;
            if (kettle != null)
            {
                KettleHome = kettle.position;
                kettle.gameObject.SetActive(false);
            }
            if (liquid != null) liquid.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            if (bubbles != null) bubbles.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            ClearVial();
            Refresh();
        }

        public void Pour(float amount, Color baseColor)
        {
            if (!HasLiquid) CurrentColor = baseColor;
            Level = Mathf.Clamp01(Level + amount);
            liquidAlpha = 1f;
            Refresh();
        }

        public void AddIngredient(IngredientData data, Prep prep)
        {
            Contents.Add(new RecipeStep { ingredient = data, prep = prep });
            if (data != null && HasLiquid) CurrentColor = Color.Lerp(CurrentColor, data.liquidTint, 0.45f);
            Refresh();
        }

        public void Mix()
        {
            Mixes++;
        }

        public void Refresh()
        {
            bool on = HasLiquid;
            Color c = CurrentColor;
            c.a = liquidAlpha;
            if (liquid != null)
            {
                liquid.enabled = on;
                liquid.color = c;
            }
            if (bubbles != null)
            {
                bubbles.enabled = on;
                bubbles.color = c;
            }
            if (gaugeFill != null)
            {
                gaugeFill.localScale = new Vector3(1f, Level, 1f);
                var fillRenderer = gaugeFill.GetComponent<SpriteRenderer>();
                if (fillRenderer != null)
                {
                    Color g = CurrentColor;
                    g.a = 1f;
                    fillRenderer.color = g;
                }
            }
        }

        public IEnumerator TintTo(Color target, float seconds)
        {
            Color from = CurrentColor;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                CurrentColor = Color.Lerp(from, target, t / seconds);
                Refresh();
                yield return null;
            }
            CurrentColor = target;
            Refresh();
        }

        public IEnumerator FillVial(Color color, float seconds)
        {
            if (vialFill == null) yield break;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                vialFill.color = new Color(color.r, color.g, color.b, t / seconds);
                yield return null;
            }
            vialFill.color = new Color(color.r, color.g, color.b, 1f);
        }

        public void ClearVial()
        {
            if (vialFill != null) vialFill.color = new Color(1f, 1f, 1f, 0f);
        }

        /// <summary>Fades the liquid out and forgets everything that went in.</summary>
        public IEnumerator EmptyPot(float seconds = 0.6f)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                liquidAlpha = 1f - t / seconds;
                Refresh();
                yield return null;
            }
            Level = 0f;
            Mixes = 0;
            Contents.Clear();
            liquidAlpha = 1f;
            CurrentColor = startColor;
            Refresh();
        }
    }
}
