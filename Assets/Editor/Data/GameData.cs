using System.Collections.Generic;

namespace PotionLab.EditorTools
{
    /// <summary>Every data asset the scene needs, gathered in one place while the scene is built.</summary>
    public class GameData
    {
        public AnimLibrary library;
        public readonly Dictionary<string, IngredientData> ingredients = new Dictionary<string, IngredientData>();
        public RecipeData[] recipes;
        public SpellData slice;
        public SpellData heat;

        // events
        public BrewActionEvent buttonPressed;
        public ShelfItemEvent shelfItemClicked;
        public GameEvent cauldronClicked;
        public GameEvent recipeBookToggled;
        public GameEvent potionBrewed;
        public GameEvent potionFailed;
        public TextEvent speech;
        public TextEvent journal;

        public bool HasAnimations => library != null && library.clips.Count > 0;
    }
}
