using UnityEngine;

namespace PotionLab
{
    /// <summary>The gauge next to the cauldron: its fill grows with the liquid level and takes the potion's colour.</summary>
    public class LevelGauge : MonoBehaviour
    {
        [SerializeField] Cauldron cauldron;
        [Tooltip("The gauge_fill sprite, with its pivot at the bottom.")]
        [SerializeField] SpriteRenderer fill;

        void OnEnable()
        {
            cauldron.Changed += Draw;
            Draw();
        }

        void OnDisable()
        {
            cauldron.Changed -= Draw;
        }

        void Draw()
        {
            fill.transform.localScale = new Vector3(1f, cauldron.Level, 1f);
            Color color = cauldron.Color;
            color.a = 1f;
            fill.color = color;
        }
    }
}
