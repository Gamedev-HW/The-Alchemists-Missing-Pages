using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PotionLab
{
    /// <summary>
    /// The recipe book that opens from the lectern: the current recipe on the left (the journal is on the right).
    /// Closes with X, a click outside it, or Back / Esc.
    /// </summary>
    public class RecipeBookView : MonoBehaviour, IBackHandler
    {
        [SerializeField] RecipeBook recipeBook;
        [Tooltip("Raised by the lectern.")]
        [SerializeField] GameEvent toggleRequested;
        [Tooltip("The open book (dark backdrop + pages). This component sits on its parent, which stays active.")]
        [SerializeField] GameObject openBook;
        [SerializeField] Text title;
        [SerializeField] Text recipeText;
        [SerializeField] Text hint;
        [SerializeField] string desktopHint = "Click outside the book or press Esc to close it";
        [SerializeField] string mobileHint = "Tap outside the book or press Back to close it";

        Coroutine opening;

        public bool IsOpen => openBook.activeSelf;

        void Awake()
        {
            openBook.SetActive(false);
            hint.text = Application.isMobilePlatform ? mobileHint : desktopHint;
        }

        void OnEnable()
        {
            recipeBook.Changed += ShowRecipe;
            toggleRequested.Raised += Toggle;
            ShowRecipe();
        }

        void OnDisable()
        {
            recipeBook.Changed -= ShowRecipe;
            toggleRequested.Raised -= Toggle;
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else if (opening == null) opening = StartCoroutine(OpenNextFrame());
        }

        public void Close()
        {
            openBook.SetActive(false);
        }

        public bool TryHandleBack()
        {
            if (!IsOpen) return false;
            Close();
            return true;
        }

        /// <summary>Opens one frame later, so the tap that opened the book can't also land on the backdrop and close it.</summary>
        IEnumerator OpenNextFrame()
        {
            yield return null;
            openBook.SetActive(true);
            opening = null;
        }

        void ShowRecipe()
        {
            RecipeData recipe = recipeBook.Current;
            title.text = recipe != null ? recipe.potionName : "";
            recipeText.text = recipe != null ? recipe.bookText : "";
        }
    }
}
