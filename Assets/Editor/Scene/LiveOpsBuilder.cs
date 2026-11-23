using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PotionLab.EditorTools
{
    /// <summary>Adds the live ops objects: the config loader and event director under Game, and the room's tint.</summary>
    public static class LiveOpsBuilder
    {
        /// <summary>Creates the components. Their event and RecipeBook fields are filled in by Wiring.AutoWire.</summary>
        public static List<MonoBehaviour> Build(Transform game, Transform room, GameData data)
        {
            Transform liveOps = SceneFactory.Group("Live Ops", game);
            var loader = liveOps.gameObject.AddComponent<LiveConfigLoader>();
            var director = liveOps.gameObject.AddComponent<LiveEventDirector>();
            Wiring.SetArray(director, "ingredients", data.ingredients.Values.OrderBy(i => i.id).ToList());

            var mood = room.gameObject.AddComponent<RoomMood>();
            Transform background = room.Find("Background");
            if (background != null) Wiring.SetArray(mood, "tinted", new[] { background.GetComponent<SpriteRenderer>() });
            return new List<MonoBehaviour> { loader, director, mood };
        }

        /// <summary>For a level that was built before live ops existed: adds them to the open scene without rebuilding it.</summary>
        public static void AddToOpenScene()
        {
            GameData data = GameDataBuilder.LoadOrCreate();
            var recipeBook = Object.FindAnyObjectByType<RecipeBook>();
            GameObject game = GameObject.Find("Game");
            GameObject room = GameObject.Find("Room");
            if (recipeBook == null || game == null || room == null)
            {
                EditorUtility.DisplayDialog("Potion Lab", "Open the PotionLab scene first.", "OK");
                return;
            }
            if (Object.FindAnyObjectByType<LiveConfigLoader>() != null)
            {
                EditorUtility.DisplayDialog("Potion Lab", "This scene already has Live Ops.", "OK");
                return;
            }

            Dictionary<string, Object> byName = new SceneContext(data).AssetsByFieldName();
            var services = new List<Object> { recipeBook };
            foreach (MonoBehaviour component in Build(game.transform, room.transform, data)) Wiring.AutoWire(component, byName, services);

            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Potion Lab] Live Ops added to " + scene.name + ". Press Play: the live config is loaded at start.");
        }
    }
}
