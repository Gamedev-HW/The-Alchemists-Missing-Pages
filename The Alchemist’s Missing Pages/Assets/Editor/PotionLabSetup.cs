using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PotionLab.EditorTools
{
    /// <summary>
    /// The "Potion Lab" menu: one-click asset setup, the game UI builder, an optional shelf shortcut and the
    /// Player Settings for PC + Android. The art can be in any folder under Assets.
    /// </summary>
    public static class PotionLabSetup
    {
        /// <summary>The folder that holds the art pack (found from bg_lab.png), e.g. Assets/Art or Assets/PotionLab/Art.</summary>
        static string Art = "Assets";
        /// <summary>Where the generated data goes: next to the art (Assets/PotionLab/Data, or Assets/PotionLabData when the art is directly in Assets).</summary>
        static string DataFolder = "Assets/PotionLabData";

        static readonly Color Ink = new Color(0.24f, 0.14f, 0.07f);
        static readonly Color Cream = new Color(0.95f, 0.89f, 0.74f);
        static readonly Color Gold = new Color(0.94f, 0.78f, 0.42f);

        // =====================================================================================
        //  1 - Set up assets
        // =====================================================================================
        [MenuItem("Potion Lab/1 - Set Up Assets (import, slice, create data)", false, 1)]
        public static void SetUpAssets()
        {
            if (!FindArt())
            {
                EditorUtility.DisplayDialog("Potion Lab",
                    "Couldn't find the art. Copy the Art folder from the download into your project's Assets folder first.", "OK");
                return;
            }
            try
            {
                EditorUtility.DisplayProgressBar("Potion Lab", "Re-importing the art with the right settings (takes a few minutes the first time)...", 0.05f);
                ReimportArt();

                for (int i = 0; i < PotionLabMeta.Sheets.Length; i++)
                {
                    var sheet = PotionLabMeta.Sheets[i];
                    EditorUtility.DisplayProgressBar("Potion Lab", "Slicing " + sheet.name, 0.2f + 0.5f * i / PotionLabMeta.Sheets.Length);
                    SliceSheet(sheet);
                }

                EditorUtility.DisplayProgressBar("Potion Lab", "Building the animation library...", 0.75f);
                BuildLibrary();

                EditorUtility.DisplayProgressBar("Potion Lab", "Creating ingredients and recipes...", 0.9f);
                var ingredients = BuildIngredients();
                BuildRecipes(ingredients);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            Debug.Log("[Potion Lab] Done. Art: " + Art + ". Sprite sheets sliced; AnimLibrary, Ingredients and Recipes are in " + DataFolder + ".");
        }

        /// <summary>Finds the art folder from bg_lab.png and decides where the data goes. False if the art isn't in the project.</summary>
        static bool FindArt()
        {
            Art = "Assets";
            string bg = FindTexture("bg_lab");
            if (bg == null) return false;
            // .../Art/Background/bg_lab.png -> .../Art   (if bg_lab.png sits right under Assets, all of Assets is searched)
            string folder = ParentFolder(bg);
            string root = ParentFolder(folder);
            Art = root.StartsWith("Assets") ? root : folder;

            string[] existing = AssetDatabase.FindAssets("t:AnimLibrary", new[] { "Assets" });
            if (existing.Length > 0)
            {
                DataFolder = ParentFolder(AssetDatabase.GUIDToAssetPath(existing[0]));   // keep using it
            }
            else
            {
                string parent = Art == "Assets" ? "Assets" : ParentFolder(Art);
                DataFolder = parent == "Assets" ? "Assets/PotionLabData" : parent + "/Data";
            }
            return true;
        }

        /// <summary>Re-imports every Potion Lab PNG so PotionLabImportSettings applies to it (art copied in before the Editor folder needs this).</summary>
        static void ReimportArt()
        {
            var paths = new List<string>();
            foreach (string name in PotionLabMeta.ArtNames)
            {
                string p = FindTexture(name);
                if (p != null) paths.Add(p);
            }
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string p in paths) AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        static void SliceSheet(PotionLabMeta.Sheet sheet)
        {
            string path = FindTexture(sheet.name);
            if (path == null)
            {
                Debug.LogWarning("[Potion Lab] Sprite sheet not found: " + sheet.name);
                return;
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            // keep the IDs of frames that already exist, so scene references survive a re-run
            var oldIds = new Dictionary<string, GUID>();
            foreach (SpriteRect r in provider.GetSpriteRects()) oldIds[r.name] = r.spriteID;

            int textureHeight = sheet.rows * sheet.frameH;
            var rects = new List<SpriteRect>();
            for (int i = 0; i < sheet.frames; i++)
            {
                int col = i % sheet.cols;
                int row = i / sheet.cols;
                string frameName = sheet.name + "_" + i.ToString("00");
                GUID id;
                if (!oldIds.TryGetValue(frameName, out id)) id = GUID.Generate();
                rects.Add(new SpriteRect
                {
                    name = frameName,
                    spriteID = id,
                    // Unity measures sprite rects from the bottom-left corner of the texture
                    rect = new Rect(col * sheet.frameW, textureHeight - (row + 1) * sheet.frameH, sheet.frameW, sheet.frameH),
                    alignment = SpriteAlignment.Custom,
                    pivot = sheet.pivot
                });
            }
            provider.SetSpriteRects(rects.ToArray());
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names != null) names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            provider.Apply();
            importer.SaveAndReimport();
        }

        static void BuildLibrary()
        {
            EnsureFolder(DataFolder);
            var library = FindLibrary();
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AnimLibrary>();
                AssetDatabase.CreateAsset(library, DataFolder + "/AnimLibrary.asset");
            }
            library.clips.Clear();
            foreach (var sheet in PotionLabMeta.Sheets)
            {
                string path = FindTexture(sheet.name);
                if (path == null) continue;
                Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                    .OrderBy(s => s.name, System.StringComparer.Ordinal).ToArray();
                var clip = new AnimClip { name = sheet.name, frames = frames, fps = sheet.fps, loop = sheet.loop };
                if (sheet.wandTipPx != null)
                {
                    float pivotX = sheet.pivot.x * sheet.frameW;
                    float pivotY = sheet.frameH;   // wizard pivot is at the bottom edge
                    clip.wandTip = sheet.wandTipPx.Select(p => new Vector2((p.x - pivotX) / 100f, (pivotY - p.y) / 100f)).ToArray();
                }
                library.clips.Add(clip);
            }
            EditorUtility.SetDirty(library);
        }

        static Dictionary<string, IngredientData> BuildIngredients()
        {
            string folder = DataFolder + "/Ingredients";
            EnsureFolder(folder);
            var result = new Dictionary<string, IngredientData>();
            foreach (var m in PotionLabMeta.Ingredients)
            {
                string path = folder + "/" + m.id + ".asset";
                var d = AssetDatabase.LoadAssetAtPath<IngredientData>(path);
                if (d == null)
                {
                    d = ScriptableObject.CreateInstance<IngredientData>();
                    AssetDatabase.CreateAsset(d, path);
                }
                d.id = m.id;
                d.displayName = m.english;
                d.hebrewName = m.hebrew;
                Color tint;
                if (ColorUtility.TryParseHtmlString(m.tint, out tint)) d.liquidTint = tint;
                d.canSlice = !m.liquid;
                d.canHeat = true;
                d.shelf = FindSprite("ing_" + m.id + "_shelf");
                d.glow = FindSprite("ing_" + m.id + "_glow");
                d.raw = FindSprite("ing_" + m.id);
                d.sliced = FindSprite("ing_" + m.id + "_sliced");
                d.heated = FindSprite("ing_" + m.id + "_heated");
                d.slicedHeated = FindSprite("ing_" + m.id + "_sliced_heated");
                EditorUtility.SetDirty(d);
                result[m.id] = d;
            }
            return result;
        }

        static void BuildRecipes(Dictionary<string, IngredientData> ing)
        {
            string folder = DataFolder + "/Recipes";
            EnsureFolder(folder);

            MakeRecipe(folder + "/1_MoonlightElixir.asset", "Moonlight Elixir",
                "1. POUR the base liquid up to the silver band on the gauge.\n" +
                "2. Take the Moonstone from the top shelf and SLICE it.\n" +
                "3. Send it TO THE POT.\n" +
                "4. MIX it ~~~ times  (an ink blot hides the number).\n" +
                "5. SERVE.",
                2, new Color(0.86f, 0.86f, 1f), true,
                Step(ing, "moonstone", Prep.Sliced));

            MakeRecipe(folder + "/2_EmberTonic.asset", "Ember Tonic",
                "1. POUR to the silver band.\n" +
                "2. Ember Root - HEAT it, then into the pot.\n" +
                "3. Nightbell Leaves - ~~~~~~~  (the ink has run).\n" +
                "4. Sun Honey, just as it is.\n" +
                "5. MIX three times, then SERVE.\n\n" +
                "The order matters!",
                3, new Color(1f, 0.55f, 0.2f), true,
                Step(ing, "ember_root", Prep.Heated),
                Step(ing, "nightbell", Prep.Sliced),
                Step(ing, "sun_honey", Prep.Raw));

            MakeRecipe(folder + "/3_FrostfireBrew.asset", "Frostfire Brew",
                "1. POUR to the silver band.\n" +
                "2. Frost Lily, sliced.\n" +
                "3. Dragon Scale - sliced AND heated.\n" +
                "4. Glowcap Mushrooms, ~~~~~~.\n" +
                "5. A drop of Star Essence to finish.\n" +
                "6. MIX ~~ times and SERVE.\n\n" +
                "The order matters!",
                4, new Color(0.45f, 0.85f, 1f), true,
                Step(ing, "frost_lily", Prep.Sliced),
                Step(ing, "dragon_scale", Prep.SlicedHeated),
                Step(ing, "glowcap", Prep.Heated),
                Step(ing, "star_essence", Prep.Raw));
        }

        static RecipeStep Step(Dictionary<string, IngredientData> ing, string id, Prep prep)
        {
            IngredientData d;
            ing.TryGetValue(id, out d);
            return new RecipeStep { ingredient = d, prep = prep };
        }

        static void MakeRecipe(string path, string title, string text, int mixTimes, Color color, bool orderMatters, params RecipeStep[] steps)
        {
            if (AssetDatabase.LoadAssetAtPath<RecipeData>(path) != null) return;   // keep your own edits
            var r = ScriptableObject.CreateInstance<RecipeData>();
            r.potionName = title;
            r.bookText = text;
            r.mixTimes = mixTimes;
            r.potionColor = color;
            r.orderMatters = orderMatters;
            r.steps = new List<RecipeStep>(steps);
            AssetDatabase.CreateAsset(r, path);
        }

        // =====================================================================================
        //  2 - Game UI + Game object
        // =====================================================================================
        [MenuItem("Potion Lab/2 - Create Game UI + Game object", false, 2)]
        public static void CreateGameUI()
        {
            var library = FindLibrary();
            if (library == null || !FindArt())
            {
                EditorUtility.DisplayDialog("Potion Lab", "Run 'Potion Lab > 1 - Set Up Assets' first.", "OK");
                return;
            }
            var existing = FindAny<GameUI>();
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Potion Lab", "The scene already has a Game UI. Replace it?", "Replace", "Cancel")) return;
                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            var eventSystem = FindAny<EventSystem>();
            if (eventSystem == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                var module = es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                module.AssignDefaultActions();   // mouse, touch, keyboard and gamepad
#else
                es.AddComponent<StandaloneInputModule>();
#endif
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            else if (eventSystem.GetComponent<StandaloneInputModule>() != null)
            {
                // an old-style module throws errors every frame when the project uses the new Input System
                Undo.DestroyObjectImmediate(eventSystem.GetComponent<StandaloneInputModule>());
                var module = Undo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(eventSystem.gameObject);
                module.AssignDefaultActions();
            }
#endif

            // Canvas: 1920x1080 reference, matched on height like the orthographic camera. In Play mode and in builds
            // the ScreenFitter takes over and fits the frame to any screen (PC window, phone, notch).
            var canvasGo = new GameObject("Game UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Game UI");
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            var ui = canvasGo.AddComponent<GameUI>();

            // a centred 1920x1080 frame: every position below is in the same pixels as the art layout
            RectTransform frame = MakeRect("Frame", canvasGo.transform, 0f, 0f, 1920f, 1080f);
            frame.anchorMin = new Vector2(0.5f, 0.5f);
            frame.anchorMax = new Vector2(0.5f, 0.5f);
            frame.anchoredPosition = Vector2.zero;
            var fitter = canvasGo.AddComponent<ScreenFitter>();
            fitter.frame = frame;
            fitter.worldCamera = Camera.main;
            if (Camera.main != null)
            {
                Undo.RecordObject(Camera.main, "Set up camera");
                Camera.main.orthographic = true;
                Camera.main.orthographicSize = 5.4f;
                Camera.main.backgroundColor = Color.black;
            }

            ui.goalText = MakeText("Goal", frame, 340f, 1050f, 640f, 44f, 26, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
            ui.goalText.rectTransform.pivot = new Vector2(0f, 0.5f);   // grows to the right when enlarged on phones
            ui.goalText.rectTransform.anchoredPosition = new Vector2(20f, 1050f);

            // ---- bar with three buttons + plaques
            MakeImage("Bar", frame, FindSprite("ui_bar"), 960f, 100f, 1920f, 200f, true);
            float[] xs = { 250f, 560f, 870f };
            Sprite normal = FindSprite("ui_btn_normal");
            for (int i = 0; i < 3; i++)
            {
                Image img = MakeImage("Button " + (i + 1), frame, normal, xs[i], 72f, 140f, 140f, true);
                var button = img.gameObject.AddComponent<Button>();
                button.targetGraphic = img;
                button.transition = Selectable.Transition.SpriteSwap;
                var state = new SpriteState();
                state.highlightedSprite = FindSprite("ui_btn_hover");
                state.pressedSprite = FindSprite("ui_btn_pressed");
                state.disabledSprite = FindSprite("ui_btn_disabled");
                button.spriteState = state;
                NoNavigation(button);
                ui.buttons[i] = button;
                ui.plaques[i] = MakeImage("Plaque " + (i + 1), frame, null, xs[i], 158f, 300f, 84f, false);
            }

            // ---- name panel
            MakeImage("Name Panel", frame, FindSprite("ui_name_panel"), 1500f, 100f, 640f, 170f, true);
            ui.panelIcon = MakeImage("Panel Icon", frame, FindSprite("ui_icon_cauldron"), 1270f, 100f, 110f, 110f, false);
            ui.panelIcon.preserveAspect = true;
            ui.panelName = MakeText("Panel Name", frame, 1560f, 116f, 470f, 60f, 30, Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            ui.panelName.resizeTextForBestFit = true;
            ui.panelName.resizeTextMinSize = 16;
            ui.panelName.resizeTextMaxSize = 32;
            for (int i = 0; i < 3; i++)
            {
                ui.badges[i] = MakeImage("Badge " + (i + 1), frame, null, 1504f + i * 56f, 58f, 46f, 46f, false);
                ui.badges[i].enabled = false;
            }

            // ---- speech bubble (near the student's head)
            Image bubble = MakeImage("Speech Bubble", frame, FindSprite("ui_speech_bubble"), 1250f, 900f, 460f, 230f, false);
            ui.bubble = bubble.gameObject;
            ui.bubbleText = MakeText("Text", bubble.transform, 230f, 130f, 400f, 150f, 26, Ink, TextAnchor.MiddleCenter, FontStyle.Normal);
            bubble.gameObject.SetActive(false);

            // ---- recipe book + journal: a dark backdrop that closes it when tapped, and the book panel on top
            RectTransform bookRoot = MakeRect("Recipe Book", frame, 960f, 560f, 1240f, 780f);
            ui.book = bookRoot.gameObject;
            Image backdrop = MakeImage("Backdrop", bookRoot, null, 620f, 390f, 8000f, 8000f, true);
            backdrop.color = new Color(0f, 0f, 0f, 0.55f);
            var backdropButton = backdrop.gameObject.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;
            NoNavigation(backdropButton);
            UnityEventTools.AddPersistentListener(backdropButton.onClick, ui.OnBackdropClicked);
            Image book = MakeImage("Panel", bookRoot, FindSprite("ui_intro_panel"), 620f, 390f, 1240f, 780f, true);
            ui.bookTitle = MakeText("Title", book.transform, 620f, 690f, 1000f, 70f, 46, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            MakeImage("Divider", book.transform, FindSprite("ui_divider"), 620f, 632f, 640f, 50f, false);
            MakeText("Recipe Header", book.transform, 330f, 585f, 500f, 40f, 28, Gold, TextAnchor.MiddleLeft, FontStyle.Bold).text = "Recipe";
            ui.bookRecipe = MakeText("Recipe Text", book.transform, 330f, 330f, 500f, 460f, 24, Cream, TextAnchor.UpperLeft, FontStyle.Normal);
            MakeText("Journal Header", book.transform, 910f, 585f, 500f, 40f, 28, Gold, TextAnchor.MiddleLeft, FontStyle.Bold).text = "Journal";
            ui.bookJournal = MakeText("Journal Text", book.transform, 910f, 330f, 500f, 460f, 20, new Color(0.85f, 0.8f, 0.68f), TextAnchor.UpperLeft, FontStyle.Normal);
            ui.bookJournal.verticalOverflow = VerticalWrapMode.Truncate;   // old lines drop off the bottom of the page
            ui.bookHint = MakeText("Hint", book.transform, 620f, 70f, 900f, 36f, 20, new Color(0.7f, 0.65f, 0.55f), TextAnchor.MiddleCenter, FontStyle.Italic);
            ui.bookHint.text = "Click outside the book or press Esc to close it";
            Image close = MakeImage("Close", book.transform, FindSprite("ui_badge_fail"), 1158f, 698f, 76f, 76f, true);
            var closeButton = close.gameObject.AddComponent<Button>();
            closeButton.targetGraphic = close;
            NoNavigation(closeButton);
            UnityEventTools.AddPersistentListener(closeButton.onClick, ui.CloseBook);
            bookRoot.gameObject.SetActive(false);

            // bigger on phones (GameUI does this at start-up when it runs on a phone)
            ui.phoneScaled = new[] { bookRoot, (RectTransform)bubble.transform, ui.goalText.rectTransform };

            // ---- sprites used at runtime
            ui.buttonNormal = normal;
            ui.buttonActive = FindSprite("ui_btn_active");
            ui.cauldronIcon = FindSprite("ui_icon_cauldron");
            ui.badgeRaw = FindSprite("ui_badge_raw");
            ui.badgeSliced = FindSprite("ui_badge_sliced");
            ui.badgeHeated = FindSprite("ui_badge_heated");
            ui.badgeInPot = FindSprite("ui_badge_in_pot");
            ui.badgeOk = FindSprite("ui_badge_ok");
            ui.badgeFail = FindSprite("ui_badge_fail");
            string[] keys = { "slice", "heat", "topot", "pour", "stop", "mix", "serve" };
            for (int i = 0; i < keys.Length; i++)
            {
                ui.plaquesEnglish[i] = FindSprite("ui_plaque_" + keys[i] + "_en");
                ui.plaquesHebrew[i] = FindSprite("ui_plaque_" + keys[i] + "_he");
            }
            int[] potPlaques = { 3, 5, 6 };   // POUR, MIX, SERVE
            for (int i = 0; i < 3; i++) ui.plaques[i].sprite = ui.plaquesEnglish[potPlaques[i]];
            EditorUtility.SetDirty(ui);

            // ---- the Game object with PotionGame
            var game = FindAny<PotionGame>();
            if (game == null)
            {
                var go = new GameObject("Game");
                Undo.RegisterCreatedObjectUndo(go, "Create Game");
                game = go.AddComponent<PotionGame>();
            }
            game.library = library;
            game.ui = ui;
            game.recipes = LoadRecipes();
            string cursorPath = FindTexture("ui_cursor_wand");
            if (cursorPath != null) game.cursor = AssetDatabase.LoadAssetAtPath<Texture2D>(cursorPath);
            EditorUtility.SetDirty(game);

            EditorSceneManager.MarkSceneDirty(canvasGo.scene);
            Selection.activeGameObject = canvasGo;
            Debug.Log("[Potion Lab] Game UI and the Game object are in the scene.");
        }

        // =====================================================================================
        //  3 - Optional: the 12 shelf ingredients
        // =====================================================================================
        [MenuItem("Potion Lab/3 - Place the 12 Shelf Ingredients (optional shortcut)", false, 3)]
        public static void PlaceShelfIngredients()
        {
            var byId = new Dictionary<string, IngredientData>();
            foreach (string guid in AssetDatabase.FindAssets("t:IngredientData", new[] { "Assets" }))
            {
                var d = AssetDatabase.LoadAssetAtPath<IngredientData>(AssetDatabase.GUIDToAssetPath(guid));
                if (d != null && !string.IsNullOrEmpty(d.id) && !byId.ContainsKey(d.id)) byId[d.id] = d;
            }
            if (byId.Count == 0)
            {
                EditorUtility.DisplayDialog("Potion Lab", "Run 'Potion Lab > 1 - Set Up Assets' first.", "OK");
                return;
            }
            float[] xs = { 551f, 694f, 838f, 982f, 1125f, 1269f };
            var root = new GameObject("Shelf Ingredients");
            Undo.RegisterCreatedObjectUndo(root, "Place Shelf Ingredients");
            for (int i = 0; i < PotionLabMeta.Ingredients.Length; i++)
            {
                var m = PotionLabMeta.Ingredients[i];
                IngredientData data;
                if (!byId.TryGetValue(m.id, out data))
                {
                    Debug.LogWarning("[Potion Lab] Ingredient asset missing: " + m.id + " - run '1 - Set Up Assets' again.");
                    continue;
                }
                int row = i < 6 ? 0 : 1;
                float y = row == 0 ? 788f : 598f;
                var go = new GameObject(m.english);
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3((xs[i % 6] - 960f) / 100f, (y - 540f) / 100f, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = data.shelf;
                sr.sortingOrder = 11;

                var glowGo = new GameObject("Glow");
                glowGo.transform.SetParent(go.transform, false);
                var glow = glowGo.AddComponent<SpriteRenderer>();
                glow.sprite = data.glow;
                glow.sortingOrder = 10;

                var item = go.AddComponent<ShelfItem>();
                item.data = data;
                item.glow = glow;
                ColliderUtil.Ensure(go, 0.8f);
            }
            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
        }

        // =====================================================================================
        //  4 - Player Settings for PC + Android
        // =====================================================================================
        [MenuItem("Potion Lab/4 - Configure Player Settings (PC + Android)", false, 20)]
        public static void ConfigurePlayerSettings()
        {
            // phones and tablets: landscape only, turning with the device
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            // PC: full screen by default, 1920x1080 when windowed, window can be resized
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.resizableWindow = true;

            // Android: 64-bit (required by Google Play and by most phones today)
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // the open scene goes into the build
            var scene = EditorSceneManager.GetActiveScene();
            string message = "Done: landscape on phones, full screen on PC, 64-bit Android.";
            if (string.IsNullOrEmpty(scene.path))
            {
                message += "\n\nSave the scene (Ctrl+S / Cmd+S) and run this again to add it to the build.";
            }
            else
            {
                var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                int index = scenes.FindIndex(s => s.path == scene.path);
                if (index < 0) scenes.Insert(0, new EditorBuildSettingsScene(scene.path, true));
                else scenes[index].enabled = true;
                EditorBuildSettings.scenes = scenes.ToArray();
                message += "\n\n" + scene.name + " is in the build's scene list.";
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Potion Lab] " + message.Replace("\n\n", " "));
            EditorUtility.DisplayDialog("Potion Lab", message, "OK");
        }

        // =====================================================================================
        //  helpers
        // =====================================================================================
        static RecipeData[] LoadRecipes()
        {
            var list = new List<RecipeData>();
            foreach (string guid in AssetDatabase.FindAssets("t:RecipeData", new[] { "Assets" }))
            {
                var r = AssetDatabase.LoadAssetAtPath<RecipeData>(AssetDatabase.GUIDToAssetPath(guid));
                if (r != null) list.Add(r);
            }
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return list.ToArray();
        }

        /// <summary>Path of the PNG with exactly this name, looked for in the art folder first and then anywhere in Assets.</summary>
        static string FindTexture(string fileName)
        {
            string p = FindTextureIn(fileName, Art);
            return p ?? (Art != "Assets" ? FindTextureIn(fileName, "Assets") : null);
        }

        static string FindTextureIn(string fileName, string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return null;
            foreach (string guid in AssetDatabase.FindAssets(fileName + " t:Texture2D", new[] { folder }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(p) == fileName) return p;
            }
            return null;
        }

        static AnimLibrary FindLibrary()
        {
            string[] guids = AssetDatabase.FindAssets("t:AnimLibrary", new[] { "Assets" });
            return guids.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<AnimLibrary>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        static string ParentFolder(string path)
        {
            string parent = Path.GetDirectoryName(path);
            return string.IsNullOrEmpty(parent) ? "" : parent.Replace('\\', '/');
        }

        static void NoNavigation(Selectable s)
        {
            var nav = s.navigation;
            nav.mode = Navigation.Mode.None;
            s.navigation = nav;
        }

        static Sprite FindSprite(string fileName)
        {
            string p = FindTexture(fileName);
            return p == null ? null : AssetDatabase.LoadAssetAtPath<Sprite>(p);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = ParentFolder(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static T FindAny<T>() where T : Object
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        static Font UIFont()
        {
#if UNITY_2022_2_OR_NEWER
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        }

        /// <summary>RectTransform anchored to the parent's bottom-left corner, centred on (x, y).</summary>
        static RectTransform MakeRect(string name, Transform parent, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.zero;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(x, y);
            r.sizeDelta = new Vector2(w, h);
            return r;
        }

        static Image MakeImage(string name, Transform parent, Sprite sprite, float x, float y, float w, float h, bool raycast)
        {
            var r = MakeRect(name, parent, x, y, w, h);
            var img = r.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = raycast;
            return img;
        }

        static Text MakeText(string name, Transform parent, float x, float y, float w, float h, int size, Color color,
            TextAnchor alignment, FontStyle style)
        {
            var r = MakeRect(name, parent, x, y, w, h);
            var t = r.gameObject.AddComponent<Text>();
            t.font = UIFont();
            t.fontSize = size;
            t.color = color;
            t.alignment = alignment;
            t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }
    }
}
