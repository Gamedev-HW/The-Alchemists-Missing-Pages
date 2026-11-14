using UnityEngine;
using UnityEngine.UI;

namespace PotionLab
{
    /// <summary>The line at the top left that says which recipe is being brewed.</summary>
    public class GoalLabel : MonoBehaviour
    {
        [SerializeField] RecipeBook recipeBook;
        [SerializeField] Text label;
        [Tooltip("{0} = recipe number, {1} = number of recipes, {2} = potion name.")]
        [SerializeField] string format = "Recipe {0}/{1}:  {2}";

        void OnEnable()
        {
            recipeBook.Changed += Show;
            Show();
        }

        void OnDisable()
        {
            recipeBook.Changed -= Show;
        }

        void Show()
        {
            RecipeData recipe = recipeBook.Current;
            label.text = recipe != null ? string.Format(format, recipeBook.Number, recipeBook.Count, recipe.potionName) : "";
        }
    }
}
