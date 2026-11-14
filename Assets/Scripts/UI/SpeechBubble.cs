using UnityEngine;
using UnityEngine.UI;

namespace PotionLab
{
    /// <summary>The student's speech bubble. Shows whatever is raised on the speech event; longer lines stay up longer.</summary>
    public class SpeechBubble : MonoBehaviour
    {
        [SerializeField] TextEvent speech;
        [Tooltip("The bubble image. This component sits on its parent, which stays active.")]
        [SerializeField] GameObject bubble;
        [SerializeField] Text text;
        [SerializeField] float minimumSeconds = 2.5f;
        [SerializeField] float secondsPerLetter = 0.04f;

        float hideAt;

        void Awake()
        {
            bubble.SetActive(false);
        }

        void OnEnable()
        {
            speech.Raised += Show;
        }

        void OnDisable()
        {
            speech.Raised -= Show;
        }

        void Show(string message)
        {
            text.text = message;
            bubble.SetActive(true);
            hideAt = Time.time + minimumSeconds + message.Length * secondsPerLetter;
        }

        void Update()
        {
            if (bubble.activeSelf && Time.time >= hideAt) bubble.SetActive(false);
        }
    }
}
