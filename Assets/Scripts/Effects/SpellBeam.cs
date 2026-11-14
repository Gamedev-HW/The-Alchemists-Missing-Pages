using System;
using UnityEngine;

namespace PotionLab
{
    /// <summary>The ribbon of light from the wand tip to a target. It follows both ends every frame while it's shown.</summary>
    public class SpellBeam : MonoBehaviour
    {
        [SerializeField] FramePlayer beam;
        [SerializeField] FramePlayer wandGlow;
        [SerializeField] string beamClip = "fx_spell_beam";
        [SerializeField] string wandGlowClip = "fx_wand_tip";
        [Tooltip("Length of one beam frame in units (512 px at 100 px per unit). The beam is stretched to reach the target.")]
        [SerializeField] float frameLength = 5.12f;
        [Tooltip("How much lighter than the beam the glow at the wand is.")]
        [SerializeField, Range(0f, 1f)] float wandGlowWhiteness = 0.5f;

        Func<Vector3> from;
        Func<Vector3> to;

        void Awake()
        {
            Hide();
        }

        public void Show(Func<Vector3> fromPoint, Func<Vector3> toPoint, Color color)
        {
            from = fromPoint;
            to = toPoint;
            SetVisible(true);
            beam.GetComponent<SpriteRenderer>().color = color;
            wandGlow.GetComponent<SpriteRenderer>().color = Color.Lerp(color, Color.white, wandGlowWhiteness);
            beam.Play(beamClip);
            wandGlow.Play(wandGlowClip);
            FollowEnds();
        }

        public void Hide()
        {
            from = null;
            to = null;
            SetVisible(false);
        }

        void LateUpdate()
        {
            FollowEnds();
        }

        void FollowEnds()
        {
            if (from == null || to == null) return;
            Vector3 start = from();
            Vector3 end = to();
            Vector3 direction = end - start;
            start.z = 0f;
            beam.transform.position = start;
            beam.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            beam.transform.localScale = new Vector3(Mathf.Max(0.01f, direction.magnitude / frameLength), 1f, 1f);
            wandGlow.transform.position = start;
        }

        void SetVisible(bool visible)
        {
            beam.gameObject.SetActive(visible);
            wandGlow.gameObject.SetActive(visible);
        }
    }
}
