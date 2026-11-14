using UnityEditor;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>The "Potion Lab" menu. Run the items in order the first time; each one can be run again later.</summary>
    public static class PotionLabMenu
    {
        [MenuItem("Potion Lab/1 - Set Up Assets", false, 1)]
        public static void SetUpAssets()
        {
            if (!ArtFinder.Locate())
            {
                EditorUtility.DisplayDialog("Potion Lab", "Couldn't find the art. Copy the Art folder into Assets first.", "OK");
                return;
            }
            try
            {
                EditorUtility.DisplayProgressBar("Potion Lab", "Importing the art (a few minutes the first time)...", 0.05f);
                SpriteSheetSlicer.ReimportAllArt();
                for (int i = 0; i < ArtCatalog.Sheets.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Potion Lab", "Slicing " + ArtCatalog.Sheets[i].name, 0.2f + 0.5f * i / ArtCatalog.Sheets.Length);
                    SpriteSheetSlicer.Slice(ArtCatalog.Sheets[i]);
                }
                EditorUtility.DisplayProgressBar("Potion Lab", "Creating the data assets...", 0.8f);
                GameDataBuilder.BuildAnimationLibrary();
                GameDataBuilder.UpdateIngredients();
                GameDataBuilder.LoadOrCreate();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            Debug.Log("[Potion Lab] Assets are ready in " + AssetFiles.DataFolder + ". Next: Potion Lab > 2 - Build the Level Scene.");
        }

        [MenuItem("Potion Lab/2 - Build the Level Scene", false, 2)]
        public static void BuildLevelScene()
        {
            LevelSceneBuilder.Build();
        }

        [MenuItem("Potion Lab/3 - Configure Player Settings (PC + Android)", false, 20)]
        public static void ConfigurePlayerSettings()
        {
            PlayerSettingsSetup.Configure();
            EditorUtility.DisplayDialog("Potion Lab", "Done: landscape on phones, full screen on PC, 64-bit Android.", "OK");
        }
    }
}
