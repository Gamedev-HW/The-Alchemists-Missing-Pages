using UnityEngine;
using UnityEngine.EventSystems;

namespace PotionLab.EditorTools
{
    /// <summary>
    /// Builds the "Game" object: input, the brewing state, one child per action, effects and platform settings.
    /// Each action is its own component, so the Hierarchy shows what the game can do.
    /// </summary>
    public static class LogicBuilder
    {
        public static void Build(SceneContext context)
        {
            AddEventSystem();
            Transform game = SceneFactory.Group("Game");

            Transform input = SceneFactory.Group("Input", game);
            input.gameObject.AddComponent<WorldPointer>();
            context.backButton = input.gameObject.AddComponent<BackButton>();

            Transform brewing = SceneFactory.Group("Brewing", game);
            context.Add(brewing.gameObject.AddComponent<Workbench>());
            context.Add(brewing.gameObject.AddComponent<ActionRunner>());
            var recipeBook = context.Add(brewing.gameObject.AddComponent<RecipeBook>());
            Wiring.SetArray(recipeBook, "recipes", context.data.recipes);
            brewing.gameObject.AddComponent<WelcomeMessage>();

            context.Add(SceneFactory.Group("Effects", game).gameObject.AddComponent<EffectSpawner>());
            BuildActions(context, SceneFactory.Group("Actions", game));
            BuildPlatform(context, SceneFactory.Group("Platform", game));
        }

        static void BuildActions(SceneContext context, Transform actions)
        {
            context.Add(Add<MishapReaction>(actions, "Mishap"));
            var pickUp = Add<PickUpIngredient>(actions, "Pick Up");
            Wiring.Set(pickUp, "workSpot", context.workSpot);
            context.backHandlers.Add(context.Add(Add<PutIngredientBack>(actions, "Put Back")));
            AddSpell(actions, "Slice", BrewActionId.Slice, context.data.slice);
            AddSpell(actions, "Heat", BrewActionId.Heat, context.data.heat);
            Add<DropIntoCauldron>(actions, "To The Pot");
            Add<PourBaseLiquid>(actions, "Pour");
            Add<MixPotion>(actions, "Mix");
            Add<ServePotion>(actions, "Serve");
        }

        static void AddSpell(Transform actions, string name, BrewActionId button, SpellData spell)
        {
            var action = Add<PrepSpellAction>(actions, name);
            Wiring.SetInt(action, "button", (int)button);
            Wiring.Set(action, "spell", spell);
        }

        static void BuildPlatform(SceneContext context, Transform platform)
        {
            platform.gameObject.AddComponent<MobileDisplaySettings>();
            var cursor = platform.gameObject.AddComponent<DesktopCursor>();
            string cursorPath = ArtFinder.FindTexture("ui_cursor_wand");
            if (cursorPath != null) Wiring.Set(cursor, "cursor", UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(cursorPath));
            // Back / Esc: book first (added by the UI builder), then the held ingredient, then quitting
            context.backHandlers.Add(platform.gameObject.AddComponent<QuitOnDoubleBack>());
        }

        static T Add<T>(Transform parent, string name) where T : Component
        {
            return SceneFactory.Group(name, parent).gameObject.AddComponent<T>();
        }

        /// <summary>The UI needs an EventSystem with the input module that matches the project's input handling.</summary>
        static void AddEventSystem()
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
