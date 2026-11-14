using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>
    /// Creates the data assets: the animation library, the 12 ingredients, sample recipes, the two spells and the event
    /// assets. Existing recipes and spells are never overwritten, so your own edits are kept.
    /// </summary>
    public static class GameDataBuilder
    {
        public static void BuildAnimationLibrary()
        {
            var library = AssetFiles.FindFirst<AnimLibrary>() ?? AssetFiles.LoadOrCreate<AnimLibrary>(AssetFiles.DataFolder + "/AnimLibrary.asset");
            library.clips.Clear();
            foreach (ArtCatalog.Sheet sheet in ArtCatalog.Sheets)
            {
                string path = ArtFinder.FindTexture(sheet.name);
                if (path != null) library.clips.Add(MakeClip(sheet, path));
            }
            EditorUtility.SetDirty(library);
        }

        static AnimClip MakeClip(ArtCatalog.Sheet sheet, string path)
        {
            Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                .OrderBy(s => s.name, System.StringComparer.Ordinal).ToArray();
            var clip = new AnimClip { name = sheet.name, frames = frames, fps = sheet.fps, loop = sheet.loop };
            if (sheet.wandTipPx != null)
            {
                // from frame pixels (top-left origin) to units from the student's feet
                float pivotX = sheet.pivot.x * sheet.frameW;
                float pivotY = sheet.frameH;
                clip.wandTip = sheet.wandTipPx.Select(p => new Vector2((p.x - pivotX) / 100f, (pivotY - p.y) / 100f)).ToArray();
            }
            return clip;
        }

        /// <summary>Fills the ingredient assets from the art pack (names, colours, sprites, which spells work on them).</summary>
        public static void UpdateIngredients()
        {
            string folder = AssetFiles.DataFolder + "/Ingredients";
            foreach (ArtCatalog.Ingredient source in ArtCatalog.Ingredients)
            {
                var ingredient = AssetFiles.LoadOrCreate<IngredientData>(folder + "/" + source.id + ".asset");
                ingredient.id = source.id;
                ingredient.displayName = source.english;
                ingredient.hebrewName = source.hebrew;
                if (ColorUtility.TryParseHtmlString(source.tint, out Color tint)) ingredient.liquidTint = tint;
                ingredient.canBe = source.liquid ? Prep.Heated : Prep.Sliced | Prep.Heated;
                ingredient.shelf = ArtFinder.FindSprite("ing_" + source.id + "_shelf");
                ingredient.glow = ArtFinder.FindSprite("ing_" + source.id + "_glow");
                ingredient.raw = ArtFinder.FindSprite("ing_" + source.id);
                ingredient.sliced = ArtFinder.FindSprite("ing_" + source.id + "_sliced");
                ingredient.heated = ArtFinder.FindSprite("ing_" + source.id + "_heated");
                ingredient.slicedHeated = ArtFinder.FindSprite("ing_" + source.id + "_sliced_heated");
                EditorUtility.SetDirty(ingredient);
            }
        }

        /// <summary>Loads every data asset, creating spells, events and sample recipes that don't exist yet.</summary>
        public static GameData LoadOrCreate()
        {
            string folder = AssetFiles.DataFolder;
            var data = new GameData { library = AssetFiles.FindFirst<AnimLibrary>() };
            foreach (IngredientData ingredient in AssetFiles.FindAll<IngredientData>())
            {
                if (!string.IsNullOrEmpty(ingredient.id) && !data.ingredients.ContainsKey(ingredient.id)) data.ingredients[ingredient.id] = ingredient;
            }

            if (AssetFiles.FindAll<RecipeData>().Count == 0) SampleRecipes.Create(folder + "/Recipes", data.ingredients);
            data.recipes = AssetFiles.FindAll<RecipeData>().ToArray();

            data.slice = Spell(folder + "/Spells/Slice.asset", SpellPresets.Slice);
            data.heat = Spell(folder + "/Spells/Heat.asset", SpellPresets.Heat);

            string events = folder + "/Events/";
            data.buttonPressed = AssetFiles.LoadOrCreate<BrewActionEvent>(events + "Button Pressed.asset");
            data.shelfItemClicked = AssetFiles.LoadOrCreate<ShelfItemEvent>(events + "Shelf Item Clicked.asset");
            data.cauldronClicked = AssetFiles.LoadOrCreate<GameEvent>(events + "Cauldron Clicked.asset");
            data.recipeBookToggled = AssetFiles.LoadOrCreate<GameEvent>(events + "Recipe Book Toggled.asset");
            data.potionBrewed = AssetFiles.LoadOrCreate<GameEvent>(events + "Potion Brewed.asset");
            data.potionFailed = AssetFiles.LoadOrCreate<GameEvent>(events + "Potion Failed.asset");
            data.speech = AssetFiles.LoadOrCreate<TextEvent>(events + "Student Says.asset");
            data.journal = AssetFiles.LoadOrCreate<TextEvent>(events + "Journal Entry.asset");
            AssetDatabase.SaveAssets();
            return data;
        }

        static SpellData Spell(string path, System.Action<SpellData> preset)
        {
            var spell = AssetFiles.LoadOrCreate<SpellData>(path, out bool created);
            if (created)
            {
                preset(spell);
                EditorUtility.SetDirty(spell);
            }
            return spell;
        }
    }
}
