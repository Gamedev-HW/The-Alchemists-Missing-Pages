using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PotionLab.EditorTools
{
    /// <summary>Builds the whole level in a new scene (Assets/Scenes/PotionLab.unity) and wires every component.</summary>
    public static class LevelSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/PotionLab.unity";
        /// <summary>Unity's URP 2D template scene: a camera and the Global Light 2D that lit sprites need.</summary>
        const string TemplatePath = "Assets/Settings/Scenes/URP2DSceneTemplate.unity";

        public static void Build()
        {
            if (!ArtFinder.Locate())
            {
                EditorUtility.DisplayDialog("Potion Lab", "Couldn't find the art. Copy the Art folder into Assets first.", "OK");
                return;
            }
            if (!GameDataBuilder.LoadOrCreate().HasAnimations)
            {
                EditorUtility.DisplayDialog("Potion Lab", "Run 'Potion Lab > 1 - Set Up Assets' first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Potion Lab", ScenePath + " already exists. Build it again from scratch?", "Rebuild", "Cancel")) return;

            Scene scene = CreateScene();
            // load the data only now: opening a scene unloads assets that were loaded before it
            GameData data = GameDataBuilder.LoadOrCreate();
            var context = new SceneContext(data) { camera = SetUpCamera() };
            context.Add(context.camera);
            PrefabBuilder.Build(context);
            WorldBuilder.Build(context);
            LogicBuilder.Build(context);
            UIBuilder.Build(context);
            WireEverything(context);

            EditorSceneManager.SaveScene(scene);
            PlayerSettingsSetup.AddSceneToBuild(ScenePath);
            Debug.Log("[Potion Lab] The level is ready in " + ScenePath + ". Press Play.");
        }

        static Scene CreateScene()
        {
            AssetFiles.EnsureFolder(ArtFinder.ParentFolder(ScenePath));
            // start from an empty scene, so the old level isn't open while its file is replaced
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (File.Exists(TemplatePath))
            {
                AssetDatabase.DeleteAsset(ScenePath);
                AssetDatabase.CopyAsset(TemplatePath, ScenePath);
                return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            AddGlobalLight();
            EditorSceneManager.SaveScene(scene, ScenePath);
            return scene;
        }

        static Camera SetUpCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = new GameObject("Main Camera").AddComponent<Camera>();
                camera.tag = "MainCamera";
            }
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = ScreenFitter.DesignHeight / ScreenFitter.PixelsPerUnit / 2f;   // half the scene height: 5.4
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            return camera;
        }

        /// <summary>Lit sprites need a 2D light. Added by name so this file doesn't depend on the URP package.</summary>
        static void AddGlobalLight()
        {
            System.Type lightType = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
            if (lightType == null) return;
            var light = (Component)new GameObject("Global Light 2D").AddComponent(lightType);
            var serialized = new SerializedObject(light);
            SerializedProperty type = serialized.FindProperty("m_LightType");
            if (type != null) type.intValue = 4;   // Global
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Fills in the references that were left empty: event assets by field name, scene objects by type.</summary>
        static void WireEverything(SceneContext context)
        {
            Wiring.SetArray(context.backButton, "handlers", context.backHandlers);

            var byName = context.AssetsByFieldName();
            foreach (var entry in byName)
            {
                if (entry.Value == null) Debug.LogError("[Potion Lab] Missing data asset for '" + entry.Key + "'. Run 'Potion Lab > 1 - Set Up Assets' again.");
            }
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (component != null && component.GetType().Namespace == typeof(Cauldron).Namespace)
                        Wiring.AutoWire(component, byName, context.services);
                }
            }
        }
    }
}