using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace PotionLab.EditorTools
{
    /// <summary>
    /// Builds the canvas: the goal line, the bar with three buttons, the name panel, the speech bubble and the recipe book.
    /// Every widget gets the small view component that keeps it up to date.
    /// </summary>
    public static class UIBuilder
    {
        static readonly Color Ink = new Color(0.24f, 0.14f, 0.07f);
        static readonly Color Cream = new Color(0.95f, 0.89f, 0.74f);
        static readonly Color Gold = new Color(0.94f, 0.78f, 0.42f);
        static readonly Color Faded = new Color(0.85f, 0.8f, 0.68f);
        static readonly Color HintColor = new Color(0.7f, 0.65f, 0.55f);
        static readonly string[] PlaqueNames = { "slice", "heat", "topot", "pour", "stop", "mix", "serve" };

        public static void Build(SceneContext context)
        {
            GameObject canvas = BuildCanvas();
            RectTransform frame = BuildFrame(canvas.transform);
            Wiring.Set(canvas.AddComponent<ScreenFitter>(), "frame", frame);

            Text goal = BuildGoalLine(frame);
            BuildActionBar(frame);
            BuildSelectionPanel(frame);
            RectTransform speech = BuildSpeechBubble(frame);
            RectTransform book = BuildRecipeBook(context, frame);

            var phoneScaler = canvas.AddComponent<PhoneTextScaler>();
            Wiring.SetArray(phoneScaler, "panels", new[] { book, speech, goal.rectTransform });
        }

        static GameObject BuildCanvas()
        {
            var go = new GameObject("Game UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            // what the editor shows; in Play mode the ScreenFitter takes over and fits the frame to the screen
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ScreenFitter.DesignWidth, ScreenFitter.DesignHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            return go;
        }

        /// <summary>A centred 1920x1080 frame: every position below is in the pixels of the art layout.</summary>
        static RectTransform BuildFrame(Transform canvas)
        {
            RectTransform frame = UIFactory.NewRect("Frame", canvas, 0f, 0f, ScreenFitter.DesignWidth, ScreenFitter.DesignHeight);
            frame.anchorMin = new Vector2(0.5f, 0.5f);
            frame.anchorMax = new Vector2(0.5f, 0.5f);
            frame.anchoredPosition = Vector2.zero;
            return frame;
        }

        static Text BuildGoalLine(RectTransform frame)
        {
            Text goal = UIFactory.NewText("Goal", frame, 340f, 1050f, 640f, 44f, 26, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
            goal.rectTransform.pivot = new Vector2(0f, 0.5f);   // grows to the right when enlarged on phones
            goal.rectTransform.anchoredPosition = new Vector2(20f, 1050f);
            Wiring.Set(goal.gameObject.AddComponent<GoalLabel>(), "label", goal);
            return goal;
        }

        static void BuildActionBar(RectTransform frame)
        {
            Image bar = UIFactory.NewImage("Bar", frame, ArtFinder.FindSprite("ui_bar"), 960f, 100f, 1920f, 200f, true);
            float[] columns = { 250f, 560f, 870f };
            var buttons = new Button[columns.Length];
            var plaques = new Image[columns.Length];
            Sprite normal = ArtFinder.FindSprite("ui_btn_normal");
            Sprite[] english = Plaques("_en");
            for (int i = 0; i < columns.Length; i++)
            {
                Image image = UIFactory.NewImage("Button " + (i + 1), frame, normal, columns[i], 72f, 140f, 140f, true);
                buttons[i] = UIFactory.NewButton(image, Selectable.Transition.SpriteSwap);
                buttons[i].spriteState = new SpriteState
                {
                    highlightedSprite = ArtFinder.FindSprite("ui_btn_hover"),
                    pressedSprite = ArtFinder.FindSprite("ui_btn_pressed"),
                    disabledSprite = ArtFinder.FindSprite("ui_btn_disabled")
                };
                plaques[i] = UIFactory.NewImage("Plaque " + (i + 1), frame, null, columns[i], 158f, 300f, 84f, false);
            }
            // what the editor shows before Play: POUR, MIX, SERVE
            plaques[0].sprite = english[(int)BrewActionId.Pour];
            plaques[1].sprite = english[(int)BrewActionId.Mix];
            plaques[2].sprite = english[(int)BrewActionId.Serve];

            var view = bar.gameObject.AddComponent<ActionBarView>();
            Wiring.SetArray(view, "buttons", buttons);
            Wiring.SetArray(view, "plaques", plaques);
            Wiring.SetArray(view, "englishPlaqueSprites", english);
            Wiring.SetArray(view, "hebrewPlaqueSprites", Plaques("_he"));
            Wiring.Set(view, "normalButton", normal);
            Wiring.Set(view, "stopButton", ArtFinder.FindSprite("ui_btn_active"));
        }

        static Sprite[] Plaques(string language)
        {
            var sprites = new Sprite[PlaqueNames.Length];
            for (int i = 0; i < PlaqueNames.Length; i++) sprites[i] = ArtFinder.FindSprite("ui_plaque_" + PlaqueNames[i] + language);
            return sprites;
        }

        static void BuildSelectionPanel(RectTransform frame)
        {
            Image panel = UIFactory.NewImage("Name Panel", frame, ArtFinder.FindSprite("ui_name_panel"), 1500f, 100f, 640f, 170f, true);
            Image icon = UIFactory.NewImage("Panel Icon", frame, ArtFinder.FindSprite("ui_icon_cauldron"), 1270f, 100f, 110f, 110f, false);
            icon.preserveAspect = true;
            Text title = UIFactory.NewText("Panel Name", frame, 1560f, 116f, 470f, 60f, 30, Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 16;
            title.resizeTextMaxSize = 32;
            var badges = new Image[3];
            for (int i = 0; i < badges.Length; i++)
            {
                badges[i] = UIFactory.NewImage("Badge " + (i + 1), frame, null, 1504f + i * 56f, 58f, 46f, 46f, false);
                badges[i].enabled = false;
            }

            var view = panel.gameObject.AddComponent<SelectionPanelView>();
            Wiring.Set(view, "icon", icon);
            Wiring.Set(view, "title", title);
            Wiring.SetArray(view, "badges", badges);
            Wiring.Set(view, "cauldronIcon", ArtFinder.FindSprite("ui_icon_cauldron"));
            Wiring.Set(view, "rawBadge", ArtFinder.FindSprite("ui_badge_raw"));
            Wiring.Set(view, "slicedBadge", ArtFinder.FindSprite("ui_badge_sliced"));
            Wiring.Set(view, "heatedBadge", ArtFinder.FindSprite("ui_badge_heated"));
            Wiring.Set(view, "inPotBadge", ArtFinder.FindSprite("ui_badge_in_pot"));
            Wiring.Set(view, "brewedBadge", ArtFinder.FindSprite("ui_badge_ok"));
            Wiring.Set(view, "failedBadge", ArtFinder.FindSprite("ui_badge_fail"));
        }

        /// <summary>The bubble by the student's head. Its holder stays active so it can listen while the bubble is hidden.</summary>
        static RectTransform BuildSpeechBubble(RectTransform frame)
        {
            RectTransform holder = UIFactory.NewRect("Speech", frame, 1250f, 900f, 460f, 230f);
            Image bubble = UIFactory.NewImage("Bubble", holder, ArtFinder.FindSprite("ui_speech_bubble"), 230f, 115f, 460f, 230f, false);
            Text text = UIFactory.NewText("Text", bubble.transform, 230f, 130f, 400f, 150f, 26, Ink, TextAnchor.MiddleCenter, FontStyle.Normal);
            bubble.gameObject.SetActive(false);

            var view = holder.gameObject.AddComponent<SpeechBubble>();
            Wiring.Set(view, "bubble", bubble.gameObject);
            Wiring.Set(view, "text", text);
            return holder;
        }

        /// <summary>The recipe book with the journal. Its holder stays active; the open book underneath is shown and hidden.</summary>
        static RectTransform BuildRecipeBook(SceneContext context, RectTransform frame)
        {
            RectTransform holder = UIFactory.NewRect("Recipe Book", frame, 960f, 560f, 1240f, 780f);
            RectTransform openBook = UIFactory.NewRect("Open Book", holder, 620f, 390f, 1240f, 780f);
            var view = holder.gameObject.AddComponent<RecipeBookView>();

            Image backdrop = UIFactory.NewImage("Backdrop", openBook, null, 620f, 390f, 8000f, 8000f, true);
            backdrop.color = new Color(0f, 0f, 0f, 0.55f);
            UnityEventTools.AddPersistentListener(UIFactory.NewButton(backdrop, Selectable.Transition.None).onClick, view.Close);

            Image pages = UIFactory.NewImage("Pages", openBook, ArtFinder.FindSprite("ui_intro_panel"), 620f, 390f, 1240f, 780f, true);
            Text title = UIFactory.NewText("Title", pages.transform, 620f, 690f, 1000f, 70f, 46, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.NewImage("Divider", pages.transform, ArtFinder.FindSprite("ui_divider"), 620f, 632f, 640f, 50f, false);
            UIFactory.NewText("Recipe Header", pages.transform, 330f, 585f, 500f, 40f, 28, Gold, TextAnchor.MiddleLeft, FontStyle.Bold).text = "Recipe";
            Text recipe = UIFactory.NewText("Recipe Text", pages.transform, 330f, 330f, 500f, 460f, 24, Cream, TextAnchor.UpperLeft, FontStyle.Normal);
            UIFactory.NewText("Journal Header", pages.transform, 910f, 585f, 500f, 40f, 28, Gold, TextAnchor.MiddleLeft, FontStyle.Bold).text = "Journal";
            Text journal = UIFactory.NewText("Journal Text", pages.transform, 910f, 330f, 500f, 460f, 20, Faded, TextAnchor.UpperLeft, FontStyle.Normal);
            journal.verticalOverflow = VerticalWrapMode.Truncate;   // old lines drop off the bottom of the page
            Text hint = UIFactory.NewText("Hint", pages.transform, 620f, 70f, 900f, 36f, 20, HintColor, TextAnchor.MiddleCenter, FontStyle.Italic);
            Image close = UIFactory.NewImage("Close", pages.transform, ArtFinder.FindSprite("ui_badge_fail"), 1158f, 698f, 76f, 76f, true);
            UnityEventTools.AddPersistentListener(UIFactory.NewButton(close, Selectable.Transition.ColorTint).onClick, view.Close);
            openBook.gameObject.SetActive(false);

            Wiring.Set(view, "openBook", openBook.gameObject);
            Wiring.Set(view, "title", title);
            Wiring.Set(view, "recipeText", recipe);
            Wiring.Set(view, "hint", hint);
            Wiring.Set(holder.gameObject.AddComponent<JournalView>(), "page", journal);
            context.backHandlers.Insert(0, view);
            return holder;
        }
    }
}
