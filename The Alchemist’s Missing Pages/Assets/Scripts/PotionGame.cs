using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PotionLab
{
    /// <summary>
    /// The game itself. Handles clicks on the shelf / cauldron / book, the three bar buttons, every spell
    /// sequence (wizard animation + beam + effect + sprite swap), the recipe check and the progression.
    /// Put it on an empty "Game" object (the menu "Potion Lab ▸ 2 - Create Game UI" does this for you).
    /// </summary>
    public class PotionGame : MonoBehaviour
    {
        [Header("Data")]
        public AnimLibrary library;
        public RecipeData[] recipes;

        [Header("Scene (found automatically when left empty)")]
        public Wizard wizard;
        public CauldronController cauldron;
        public GameUI ui;
        [Tooltip("Where a picked ingredient floats. Empty = above the cauldron, scene pixel (820, 700).")]
        public Transform workSpot;

        [Header("Feel")]
        [Tooltip("Gauge filled per second while pouring.")]
        public float pourSpeed = 0.3f;
        public float mixSeconds = 1.6f;
        public Color baseLiquid = new Color(0.55f, 0.8f, 1f);
        public Color sliceBeam = new Color(0.75f, 0.88f, 1f);
        public Color heatBeam = new Color(1f, 0.7f, 0.43f);
        public Color pourBeam = new Color(0.75f, 0.88f, 1f);
        public Color mixBeam = new Color(0.55f, 1f, 0.86f);
        public Color serveBeam = new Color(1f, 0.86f, 0.55f);
        public Texture2D cursor;

        public Mode CurrentMode { get; private set; }

        class Floating
        {
            public GameObject root;
            public Transform art;
            public SpriteRenderer sr;
            public GameObject levitate;
            public IngredientData data;
            public Prep prep;
            public Vector3 home;
        }

        bool busy;
        bool pouring;
        bool stopPour;
        int recipeIndex;
        ShelfItem hovered;
        Floating floating;
        SpellBeam beam;
        Camera cam;

        RecipeData CurrentRecipe
        {
            get { return recipes != null && recipes.Length > 0 ? recipes[recipeIndex % recipes.Length] : null; }
        }

        Vector3 WorkSpot
        {
            get { return workSpot != null ? workSpot.position : Px.ToWorld(820f, 700f); }
        }

        // ------------------------------------------------------------------ setup
        void Awake()
        {
            Fx.Library = library;
            if (library == null) Debug.LogError("[Potion Lab] Assign the AnimLibrary asset (made by 'Potion Lab > 1 - Set Up Assets') on " + name + ".");
            if (wizard == null) wizard = FindAny<Wizard>();
            if (cauldron == null) cauldron = FindAny<CauldronController>();
            if (ui == null) ui = FindAny<GameUI>();
            cam = Camera.main;

            // a UI made with an older version of the menu has no ScreenFitter yet: add one
            if (ui != null && ui.GetComponent<Canvas>() != null && FindAny<ScreenFitter>() == null)
            {
                var fitter = ui.gameObject.AddComponent<ScreenFitter>();
                fitter.frame = ui.transform.Find("Frame") as RectTransform;
                fitter.Apply();
            }
            beam = SpellBeam.Create();
        }

        void Start()
        {
            // a custom cursor only makes sense with a mouse
            if (cursor != null && !Application.isMobilePlatform) Cursor.SetCursor(cursor, new Vector2(6f, 6f), CursorMode.Auto);
            CurrentMode = Mode.Pot;
            if (ui != null)
            {
                ui.Bind(this);
                ui.SetMode(Mode.Pot);
                ui.SetRecipe(CurrentRecipe, recipeIndex, recipes == null ? 0 : recipes.Length);
                ui.ShowCauldron(cauldron);
            }
            Say(Click + " the recipe book on the lectern to read the recipe, then start with POUR.", 5f);
        }

        /// <summary>"Tap" on phones, "Click" on a computer.</summary>
        static string Click
        {
            get { return Application.isMobilePlatform ? "Tap" : "Click"; }
        }

        static T FindAny<T>() where T : Object
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        // ------------------------------------------------------------------ input
        void Update()
        {
            if (cam == null) cam = Camera.main;
            if (PointerInput.BackPressed) OnBack();

            // A mouse is checked every frame (for the hover glow); a finger only when it touches the screen.
            bool pressed = PointerInput.PressedThisFrame;
            bool canHover = PointerInput.HasHover;
            Collider2D hit = null;
            if ((pressed || canHover) && cam != null)
            {
                Vector2 screen = PointerInput.Position;
                if (!PointerOverUI(screen))
                {
                    Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
                    hit = Physics2D.OverlapPoint(new Vector2(world.x, world.y));
                }
            }

            ShelfItem shelf = hit != null ? hit.GetComponent<ShelfItem>() : null;
            ShelfItem glow = canHover && !busy && !pouring ? shelf : null;
            if (glow != hovered)
            {
                if (hovered != null) hovered.SetHover(false);
                hovered = glow;
                if (hovered != null) hovered.SetHover(true);
            }

            if (floating != null && !busy)
                floating.root.transform.position = floating.home + Vector3.up * (Mathf.Sin(Time.time * 2f) * 0.06f);

            if (!pressed || hit == null || busy || pouring) return;

            if (shelf != null)
            {
                StartCoroutine(Pick(shelf));
                return;
            }
            var target = hit.GetComponent<ClickTarget>();
            if (target == null) return;
            if (target.kind == ClickTarget.Kind.RecipeBook)
            {
                if (ui != null) ui.ToggleBook();
            }
            else
            {
                StartCoroutine(SelectPot());
            }
        }

        readonly List<RaycastResult> uiHits = new List<RaycastResult>();
        PointerEventData uiPointer;
        EventSystem uiPointerOwner;

        /// <summary>
        /// Is the pointer over a button or panel? Asks the UI directly, so it works the same for mouse and touch,
        /// even on the very frame a finger lands.
        /// </summary>
        bool PointerOverUI(Vector2 screen)
        {
            EventSystem es = EventSystem.current;
            if (es == null) return false;
            if (uiPointer == null || uiPointerOwner != es)
            {
                uiPointer = new PointerEventData(es);
                uiPointerOwner = es;
            }
            uiPointer.position = screen;
            es.RaycastAll(uiPointer, uiHits);
            return uiHits.Count > 0;
        }

        float lastBack = -10f;

        /// <summary>Esc on a computer, Back on Android: close the book, put the ingredient back, or (pressed twice) quit.</summary>
        void OnBack()
        {
            if (ui != null && ui.BookOpen)
            {
                ui.CloseBook();
                return;
            }
            if (floating != null && !busy && !pouring)
            {
                StartCoroutine(SelectPot());
                return;
            }
            if (Application.isEditor) return;
            if (Time.unscaledTime - lastBack < 2f)
            {
                Application.Quit();
                return;
            }
            lastBack = Time.unscaledTime;
            Say(Application.isMobilePlatform ? "Press Back again to quit." : "Press Esc again to quit.", 2f);
        }

        /// <summary>Called by the three round buttons (index 0, 1, 2).</summary>
        public void OnButton(int index)
        {
            if (pouring)
            {
                if (index == 0) stopPour = true;
                return;
            }
            if (busy) return;

            if (CurrentMode == Mode.Ingredient)
            {
                if (floating == null) return;
                if (index == 0) StartCoroutine(Slice());
                else if (index == 1) StartCoroutine(Heat());
                else StartCoroutine(ToPot());
            }
            else
            {
                if (index == 0) StartCoroutine(Pour());
                else if (index == 1) StartCoroutine(Mix());
                else StartCoroutine(Serve());
            }
        }

        // ------------------------------------------------------------------ picking an ingredient
        IEnumerator Pick(ShelfItem s)
        {
            if (s.data == null) yield break;
            Begin();
            if (floating != null) yield return ReturnFloating();

            IngredientData d = s.data;
            Vector3 from = s.transform.position + new Vector3(0f, 0.85f, 0f);
            Fx.Spawn("fx_poof", from, Order.Burst);

            var f = new Floating { data = d, prep = Prep.Raw };
            f.root = new GameObject("Floating " + d.displayName);
            f.root.transform.position = from;
            FramePlayer lev = Fx.Spawn("fx_levitate", from + new Vector3(0f, -0.1f, 0f), Order.Levitate, null, 1.1f, f.root.transform);
            f.levitate = lev != null ? lev.gameObject : null;
            var art = new GameObject("Art");
            art.transform.SetParent(f.root.transform, false);
            f.art = art.transform;
            f.sr = art.AddComponent<SpriteRenderer>();
            f.sr.sprite = d.raw;
            f.sr.sortingOrder = Order.Floating;
            floating = f;

            CurrentMode = Mode.Ingredient;
            if (ui != null)
            {
                ui.SetMode(Mode.Ingredient);
                ui.ShowIngredient(d, Prep.Raw);
            }

            Vector3 to = WorkSpot;
            const float duration = 0.6f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                f.root.transform.position = Vector3.Lerp(from, to, k) + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 1.2f);
                f.art.localScale = Vector3.one * Mathf.Lerp(0.55f, 0.8f, k);
                yield return null;
            }
            f.root.transform.position = to;
            f.art.localScale = Vector3.one * 0.8f;
            f.home = to;
            End();
        }

        IEnumerator ReturnFloating()
        {
            if (floating == null) yield break;
            Fx.Spawn("fx_poof", floating.root.transform.position, Order.Burst);
            Destroy(floating.root);
            floating = null;
            yield return new WaitForSeconds(0.15f);
        }

        IEnumerator SelectPot()
        {
            Begin();
            if (floating != null) yield return ReturnFloating();
            CurrentMode = Mode.Pot;
            if (ui != null) ui.SetMode(Mode.Pot);
            ShowCauldronInfo();
            End();
        }

        // ------------------------------------------------------------------ ingredient spells
        IEnumerator Slice()
        {
            Floating f = floating;
            IngredientData d = f.data;
            if (!d.canSlice)
            {
                yield return Fail("You can't slice " + d.displayName + " - it's a liquid!", FloatPos());
                yield break;
            }
            if (f.prep == Prep.Sliced || f.prep == Prep.SlicedHeated)
            {
                Say(d.displayName + " is already sliced.");
                yield break;
            }

            Begin();
            WizardPlay("wizard_cast_spot");
            yield return WaitWizardFrame(4);
            beam.Show(WandTip, FloatPos, sliceBeam);
            FramePlayer fx = Fx.Spawn("fx_slice", FloatPos(), Order.Slice);
            yield return WaitFrame(fx, 5);

            f.prep = f.prep == Prep.Heated ? Prep.SlicedHeated : Prep.Sliced;
            f.sr.sprite = d.SpriteFor(f.prep);
            if (ui != null) ui.ShowIngredient(d, f.prep);
            Log(d.displayName + ": SLICE -> " + IngredientData.PrepName(f.prep));
            Say("Clean cut!");

            yield return WaitDone(fx);
            beam.Hide();
            yield return WaitWizard();
            WizardIdle();
            End();
        }

        IEnumerator Heat()
        {
            Floating f = floating;
            IngredientData d = f.data;
            if (!d.canHeat)
            {
                yield return Fail(d.displayName + " just crackles and smokes.", FloatPos());
                yield break;
            }
            if (f.prep == Prep.Heated || f.prep == Prep.SlicedHeated)
            {
                Say(d.displayName + " is already hot.");
                yield break;
            }

            Begin();
            WizardPlay("wizard_cast_spot");
            yield return WaitWizardFrame(4);
            beam.Show(WandTip, FloatPos, heatBeam);
            Vector3 at = FloatPos() + new Vector3(0f, 0.2f, 0f);
            FramePlayer back = Fx.Spawn("fx_heat_back", at, Order.HeatBack);
            FramePlayer front = Fx.Spawn("fx_heat_front", at, Order.HeatFront);
            yield return WaitFrame(front, 6);

            f.prep = f.prep == Prep.Sliced ? Prep.SlicedHeated : Prep.Heated;
            f.sr.sprite = d.SpriteFor(f.prep);
            if (ui != null) ui.ShowIngredient(d, f.prep);
            FramePlayer steam = Fx.Spawn("fx_steam", FloatPos() + new Vector3(0f, 0.5f, 0f), Order.Steam, null, 0.6f, f.root.transform);
            if (steam != null) Destroy(steam.gameObject, 3f);
            Log(d.displayName + ": HEAT -> " + IngredientData.PrepName(f.prep));
            Say("Nice and hot!");

            yield return WaitDone(front);
            yield return WaitDone(back);
            beam.Hide();
            yield return WaitWizard();
            WizardIdle();
            End();
        }

        IEnumerator ToPot()
        {
            Floating f = floating;
            IngredientData d = f.data;
            Begin();
            WizardPlay("wizard_cast_pot");
            yield return WaitWizardFrame(3);

            if (f.levitate != null) Destroy(f.levitate);
            Vector3 from = f.root.transform.position;
            Vector3 to = cauldron != null ? cauldron.SurfacePoint : from + Vector3.down * 2f;
            float startScale = f.art.localScale.x;
            const float duration = 0.45f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                k *= k;
                f.root.transform.position = Vector3.Lerp(from, to, k);
                f.art.localScale = Vector3.one * Mathf.Lerp(startScale, 0.45f, k);
                yield return null;
            }
            Destroy(f.root);
            floating = null;

            bool wet = cauldron != null && cauldron.HasLiquid;
            if (cauldron != null) cauldron.AddIngredient(d, f.prep);
            if (wet) Fx.Spawn("fx_splash", to, Order.Splash, cauldron.CurrentColor);
            else Fx.Spawn("fx_poof", to, Order.Burst, null, 0.6f);
            Log(d.displayName + " (" + IngredientData.PrepName(f.prep) + ") -> into the cauldron");
            Say(wet ? d.displayName + " sinks into the potion." : d.displayName + " lands in the empty cauldron...");

            CurrentMode = Mode.Pot;
            if (ui != null) ui.SetMode(Mode.Pot);
            ShowCauldronInfo();
            yield return WaitWizard();
            WizardIdle();
            End();
        }

        // ------------------------------------------------------------------ cauldron actions
        IEnumerator Pour()
        {
            if (cauldron == null || cauldron.kettle == null)
            {
                Say("Drag the kettle into the 'Kettle' field of the Cauldron object.");
                yield break;
            }
            busy = true;
            pouring = true;
            stopPour = false;
            if (ui != null)
            {
                ui.SetPouring(true);
                ui.SetInteractable(false, 0);
            }
            WizardPlay("wizard_cast_pot");

            Transform k = cauldron.kettle;
            Vector3 home = cauldron.KettleHome;
            Vector3 away = home + new Vector3(3f, 2.4f, 0f);
            k.gameObject.SetActive(true);
            k.position = away;
            k.rotation = Quaternion.identity;
            FramePlayer glow = Fx.Spawn("fx_levitate", away + new Vector3(0f, -0.2f, 0f), Order.KettleGlow, null, 1.2f, k);

            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                k.position = Vector3.Lerp(away, home, Mathf.SmoothStep(0f, 1f, t / 0.35f));
                yield return null;
            }
            k.position = home;
            beam.Show(WandTip, () => k.position, pourBeam);
            for (float t = 0f; t < 0.25f; t += Time.deltaTime)
            {
                k.rotation = Quaternion.Euler(0f, 0f, cauldron.pourTilt * t / 0.25f);
                yield return null;
            }
            k.rotation = Quaternion.Euler(0f, 0f, cauldron.pourTilt);

            FramePlayer stream = Fx.Spawn("fx_pour_stream", cauldron.Spout, Order.Stream, baseLiquid);
            if (stream != null)
            {
                float drop = Mathf.Max(0.2f, cauldron.Spout.y - cauldron.SurfacePoint.y);
                stream.transform.localScale = new Vector3(0.5f, drop / 3.84f, 1f);
            }
            float splashTimer = 0f;
            while (!stopPour && cauldron.Level < 1f)
            {
                cauldron.Pour(pourSpeed * Time.deltaTime, baseLiquid);
                splashTimer -= Time.deltaTime;
                if (splashTimer <= 0f)
                {
                    Fx.Spawn("fx_splash", cauldron.SurfacePoint + new Vector3(0.2f, 0f, 0f), Order.Splash, cauldron.CurrentColor, 0.6f);
                    splashTimer = 0.5f;
                }
                yield return null;
            }
            if (stream != null) Destroy(stream.gameObject);
            beam.Hide();

            for (float t = 0f; t < 0.25f; t += Time.deltaTime)
            {
                k.rotation = Quaternion.Euler(0f, 0f, cauldron.pourTilt * (1f - t / 0.25f));
                yield return null;
            }
            k.rotation = Quaternion.identity;
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                k.position = Vector3.Lerp(home, away, t / 0.3f);
                yield return null;
            }
            if (glow != null) Destroy(glow.gameObject);
            k.position = home;
            k.gameObject.SetActive(false);

            pouring = false;
            if (ui != null) ui.SetPouring(false);
            int percent = Mathf.RoundToInt(cauldron.Level * 100f);
            Log("POUR -> base liquid at " + percent + "%");
            RecipeData r = CurrentRecipe;
            if (r != null && cauldron.Level > r.pourMax) Say("Whoa, that's a lot of liquid...");
            else if (r != null && cauldron.Level < r.pourMin) Say("Is that enough liquid?");
            else Say("The cauldron is filled.");
            WizardIdle();
            ShowCauldronInfo();
            End();
        }

        IEnumerator Mix()
        {
            if (cauldron == null || !cauldron.HasLiquid)
            {
                Say("There's nothing to stir yet - POUR first.");
                yield break;
            }
            Begin();
            WizardPlay("wizard_stir");
            beam.Show(WandTip, () => cauldron.SurfacePoint + new Vector3(0f, 0.6f, 0f), mixBeam);
            FramePlayer vortex = Fx.Spawn("fx_mix", cauldron.SurfacePoint, Order.Mix);
            yield return new WaitForSeconds(mixSeconds);
            if (vortex != null) Destroy(vortex.gameObject);
            beam.Hide();
            WizardIdle();
            cauldron.Mix();
            Log("MIX -> stirred " + cauldron.Mixes + "x");
            Say("Stirred " + cauldron.Mixes + (cauldron.Mixes == 1 ? " time." : " times."));
            ShowCauldronInfo();
            End();
        }

        IEnumerator Serve()
        {
            if (cauldron == null || !cauldron.HasLiquid)
            {
                Say("The cauldron is empty.");
                yield break;
            }
            Begin();
            RecipeData r = CurrentRecipe;
            string why = "";
            bool ok = r == null || r.Check(cauldron.Level, cauldron.Contents, cauldron.Mixes, out why);

            if (ok)
            {
                Color result = r != null ? r.potionColor : cauldron.CurrentColor;
                WizardPlay("wizard_cast_pot");
                yield return WaitWizardFrame(3);
                yield return cauldron.TintTo(result, 0.5f);
                beam.Show(() => cauldron.SurfacePoint, () => cauldron.VialPoint, serveBeam);
                yield return cauldron.FillVial(result, 0.8f);
                beam.Hide();
                Fx.Spawn("fx_serve", cauldron.VialPoint, Order.Burst);
                WizardPlay("wizard_happy");
                string potion = r != null ? r.potionName : "The potion";
                Say("Perfect! " + potion + " is ready!", 3f);
                Log("SUCCESS: " + potion + " brewed!");
                yield return new WaitForSeconds(2.2f);
                yield return cauldron.EmptyPot();
                cauldron.ClearVial();

                if (recipes != null && recipes.Length > 0)
                {
                    recipeIndex++;
                    if (recipeIndex >= recipes.Length)
                    {
                        recipeIndex = 0;
                        Say("You brewed every recipe in the book! Starting over.", 4f);
                    }
                    if (ui != null) ui.SetRecipe(CurrentRecipe, recipeIndex, recipes.Length);
                }
            }
            else
            {
                Fx.Spawn("fx_smoke_fail", cauldron.SurfacePoint + new Vector3(0f, 0.5f, 0f), Order.Burst);
                WizardPlay("wizard_oops");
                Say("The potion fizzles... " + why, 4.5f);
                Log("FAILED: " + why);
                if (ui != null) ui.SetBadges(ui.badgeFail);
                yield return new WaitForSeconds(1.2f);
                yield return cauldron.EmptyPot();
            }
            WizardIdle();
            ShowCauldronInfo();
            End();
        }

        IEnumerator Fail(string message, Vector3 at)
        {
            Begin();
            Fx.Spawn("fx_smoke_fail", at, Order.Burst);
            WizardPlay("wizard_oops");
            Say(message);
            Log("FAILED: " + message);
            yield return WaitWizard();
            WizardIdle();
            End();
        }

        // ------------------------------------------------------------------ helpers
        void Begin()
        {
            busy = true;
            if (ui != null) ui.SetInteractable(false);
        }

        void End()
        {
            busy = false;
            if (ui != null) ui.SetInteractable(true);
        }

        void Say(string message, float seconds = 3.5f)
        {
            if (ui != null) ui.Say(message, seconds);
            else Debug.Log("[Potion Lab] " + message);
        }

        void Log(string line)
        {
            if (ui != null) ui.Log(line);
        }

        void ShowCauldronInfo()
        {
            if (ui != null) ui.ShowCauldron(cauldron);
        }

        Vector3 FloatPos()
        {
            return floating != null ? floating.root.transform.position : WorkSpot;
        }

        Vector3 WandTip()
        {
            return wizard != null ? wizard.WandTip : Px.ToWorld(1360f, 740f);
        }

        void WizardPlay(string clipName)
        {
            if (wizard != null) wizard.Play(clipName);
        }

        void WizardIdle()
        {
            if (wizard != null) wizard.Idle();
        }

        IEnumerator WaitWizardFrame(int frame)
        {
            if (wizard == null) yield break;
            while (wizard.IsPlaying && wizard.Frame < frame) yield return null;
        }

        IEnumerator WaitWizard()
        {
            if (wizard == null) yield break;
            while (wizard.IsPlaying) yield return null;
        }

        static IEnumerator WaitFrame(FramePlayer fx, int frame)
        {
            while (fx != null && fx.IsPlaying && fx.Frame < frame) yield return null;
        }

        static IEnumerator WaitDone(FramePlayer fx)
        {
            while (fx != null && fx.IsPlaying) yield return null;
        }

#if UNITY_EDITOR
        // Fills the data fields automatically when the component is added in the editor.
        void Reset()
        {
            string[] lib = UnityEditor.AssetDatabase.FindAssets("t:AnimLibrary");
            if (lib.Length > 0) library = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimLibrary>(UnityEditor.AssetDatabase.GUIDToAssetPath(lib[0]));
            var found = new List<RecipeData>();
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:RecipeData"))
            {
                var r = UnityEditor.AssetDatabase.LoadAssetAtPath<RecipeData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                if (r != null) found.Add(r);
            }
            found.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            recipes = found.ToArray();
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("ui_cursor_wand t:Texture2D"))
                cursor = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
        }
#endif
    }
}
