using UnityEngine;

namespace PotionLab
{
    /// <summary>The student's first hint when the level starts.</summary>
    public class WelcomeMessage : MonoBehaviour
    {
        [SerializeField] TextEvent speech;
        [SerializeField] string desktopText = "Click the recipe book on the lectern to read the recipe, then start with POUR.";
        [SerializeField] string mobileText = "Tap the recipe book on the lectern to read the recipe, then start with POUR.";

        void Start()
        {
            speech.Raise(Application.isMobilePlatform ? mobileText : desktopText);
        }
    }
}
