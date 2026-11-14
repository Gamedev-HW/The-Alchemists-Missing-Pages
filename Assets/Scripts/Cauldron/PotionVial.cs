using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>The vial on the counter that fills with a finished potion.</summary>
    public class PotionVial : MonoBehaviour
    {
        [Tooltip("The white vial_fill sprite inside the glass.")]
        [SerializeField] SpriteRenderer fill;
        [SerializeField] float fillSeconds = 0.8f;

        public Vector3 Point => fill.transform.position;

        void Awake()
        {
            Clear();
        }

        public IEnumerator Fill(Color potion)
        {
            for (float t = 0f; t < fillSeconds; t += Time.deltaTime)
            {
                fill.color = new Color(potion.r, potion.g, potion.b, t / fillSeconds);
                yield return null;
            }
            fill.color = new Color(potion.r, potion.g, potion.b, 1f);
        }

        public void Clear()
        {
            fill.color = Color.clear;
        }
    }
}
