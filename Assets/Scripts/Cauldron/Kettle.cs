using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// The kettle that pours the base liquid. It stays hidden, flies in when asked, tilts to pour and flies away again.
    /// It doesn't decide when to pour; PourBaseLiquid does.
    /// </summary>
    public class Kettle : MonoBehaviour
    {
        [SerializeField] SpriteRenderer body;
        [Tooltip("The magic glow that carries the kettle (fx_levitate).")]
        [SerializeField] GameObject levitation;
        [Tooltip("Tip of the spout, in the kettle's own units.")]
        [SerializeField] Vector2 spout = new Vector2(-1.04f, 0.42f);
        [SerializeField] float pourAngle = 28f;
        [Tooltip("Where it flies in from, relative to its place in the scene.")]
        [SerializeField] Vector3 offstageOffset = new Vector3(3f, 2.4f, 0f);
        [SerializeField] float flyInSeconds = 0.35f;
        [SerializeField] float tiltSeconds = 0.25f;
        [SerializeField] float flyOutSeconds = 0.3f;

        Vector3 home;

        public Vector3 SpoutTip => transform.TransformPoint(spout);

        void Awake()
        {
            home = transform.position;
            SetVisible(false);
        }

        public IEnumerator FlyIn()
        {
            transform.rotation = Quaternion.identity;
            SetVisible(true);
            yield return Move(home + offstageOffset, home, flyInSeconds);
        }

        public IEnumerator TiltToPour()
        {
            yield return Tilt(0f, pourAngle);
        }

        public IEnumerator TiltBack()
        {
            yield return Tilt(pourAngle, 0f);
        }

        public IEnumerator FlyOut()
        {
            yield return Move(home, home + offstageOffset, flyOutSeconds);
            SetVisible(false);
            transform.position = home;
        }

        IEnumerator Move(Vector3 from, Vector3 to, float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / seconds));
                yield return null;
            }
            transform.position = to;
        }

        IEnumerator Tilt(float fromAngle, float toAngle)
        {
            for (float t = 0f; t < tiltSeconds; t += Time.deltaTime)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(fromAngle, toAngle, t / tiltSeconds));
                yield return null;
            }
            transform.rotation = Quaternion.Euler(0f, 0f, toAngle);
        }

        void SetVisible(bool visible)
        {
            body.enabled = visible;
            if (levitation != null) levitation.SetActive(visible);
        }
    }
}
