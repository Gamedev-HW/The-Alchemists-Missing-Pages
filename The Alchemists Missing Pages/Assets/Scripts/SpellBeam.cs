using UnityEngine;

namespace PotionLab
{
    /// <summary>The ribbon of light from the wand tip to the target. Created from code by PotionGame.</summary>
    public class SpellBeam : MonoBehaviour
    {
        SpriteRenderer beam;
        SpriteRenderer tip;
        FramePlayer beamPlayer;
        FramePlayer tipPlayer;
        System.Func<Vector3> from;
        System.Func<Vector3> to;

        public static SpellBeam Create()
        {
            var root = new GameObject("SpellBeam");
            var sb = root.AddComponent<SpellBeam>();

            var b = new GameObject("Beam");
            b.transform.SetParent(root.transform, false);
            sb.beam = b.AddComponent<SpriteRenderer>();
            sb.beam.sortingOrder = Order.Beam;
            sb.beamPlayer = b.AddComponent<FramePlayer>();

            var t = new GameObject("WandTip");
            t.transform.SetParent(root.transform, false);
            sb.tip = t.AddComponent<SpriteRenderer>();
            sb.tip.sortingOrder = Order.WandTip;
            sb.tipPlayer = t.AddComponent<FramePlayer>();

            root.SetActive(false);
            return sb;
        }

        public void Show(System.Func<Vector3> fromPoint, System.Func<Vector3> toPoint, Color color)
        {
            from = fromPoint;
            to = toPoint;
            gameObject.SetActive(true);
            beam.color = color;
            tip.color = Color.Lerp(color, Color.white, 0.5f);
            if (Fx.Library != null)
            {
                beamPlayer.Play(Fx.Library.Get("fx_spell_beam"));
                tipPlayer.Play(Fx.Library.Get("fx_wand_tip"));
            }
            LateUpdate();
        }

        public void Hide()
        {
            from = null;
            to = null;
            gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (from == null || to == null) return;
            Vector3 a = from();
            Vector3 b = to();
            a.z = 0f;
            b.z = 0f;
            Vector3 d = b - a;
            beam.transform.position = a;
            beam.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            // the beam frames are 512 px = 5.12 units long, pivot at the left end
            beam.transform.localScale = new Vector3(Mathf.Max(0.01f, d.magnitude / 5.12f), 1f, 1f);
            tip.transform.position = a;
        }
    }
}
