using UnityEditor;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>
    /// Builds the room: background, lectern, candles, the shelf of ingredients, the student, the cauldron station
    /// and the spell beam. Positions are in units: (pixel - 960) / 100 across, (pixel - 540) / 100 up.
    /// </summary>
    public static class WorldBuilder
    {
        static readonly float[] ShelfColumns = { -4.09f, -2.66f, -1.22f, 0.22f, 1.65f, 3.09f };
        static readonly float[] ShelfRows = { 2.48f, 0.58f };

        public static void Build(SceneContext context)
        {
            BuildRoom(context);
            BuildCandles(context.data);
            BuildShelf(context);
            BuildStudent(context);
            BuildCauldron(context);
            BuildGauge(context);
            BuildVialAndKettle(context);
            BuildSpellBeam(context);
            context.workSpot = SceneFactory.Group("Work Spot", null, -1.40f, 1.60f);
        }

        static void BuildRoom(SceneContext context)
        {
            Transform room = SceneFactory.Group("Room");
            SceneFactory.Sprite("Background", room, ArtFinder.FindSprite("bg_lab"), 0f, 0f, 0);
            SceneFactory.Sprite("Vignette", room, ArtFinder.FindSprite("fg_vignette"), 0f, 0f, 90);
            SceneFactory.Sprite("Counter", room, ArtFinder.FindSprite("fg_counter"), 2.35f, -3.075f, 30);
            SpriteRenderer lectern = SceneFactory.Sprite("Lectern", room, ArtFinder.FindSprite("prop_lectern_book"), -7.30f, -1.29f, 5);
            MakeClickable(lectern.gameObject, context.data.recipeBookToggled);
        }

        static void BuildCandles(GameData data)
        {
            Transform candles = SceneFactory.Group("Candles");
            Vector2[] flames = { new Vector2(-4.94f, -1.10f), new Vector2(-4.66f, -1.48f), new Vector2(-4.42f, -1.72f), new Vector2(-6.14f, 3.75f) };
            for (int i = 0; i < flames.Length; i++)
                SceneFactory.Animated("Flame " + (i + 1), candles, data.library, "fx_candle_flame", flames[i].x, flames[i].y, 6, true);
        }

        static void BuildShelf(SceneContext context)
        {
            Transform shelf = SceneFactory.Group("Shelf");
            for (int i = 0; i < ArtCatalog.Ingredients.Length; i++)
            {
                if (!context.data.ingredients.TryGetValue(ArtCatalog.Ingredients[i].id, out IngredientData ingredient)) continue;
                int column = i % ShelfColumns.Length;
                int row = i / ShelfColumns.Length;
                PlaceShelfItem(context.shelfItemPrefab, shelf, ingredient, ShelfColumns[column], ShelfRows[row]);
            }
        }

        static void PlaceShelfItem(ShelfItem prefab, Transform shelf, IngredientData ingredient, float x, float y)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, shelf);
            go.name = ingredient.displayName;
            go.transform.localPosition = new Vector3(x, y, 0f);
            var body = go.GetComponent<SpriteRenderer>();
            body.sprite = ingredient.shelf;
            var glow = go.transform.Find("Glow").GetComponent<SpriteRenderer>();
            glow.sprite = ingredient.glow;
            BoxCollider2D area = SceneFactory.FitClickArea(go, 0.8f);
            Wiring.Set(go.GetComponent<ShelfItem>(), "data", ingredient);
            // changes made from code to a prefab instance must be recorded, or they are lost when the scene reloads
            PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(body);
            PrefabUtility.RecordPrefabInstancePropertyModifications(glow);
            PrefabUtility.RecordPrefabInstancePropertyModifications(area);
        }

        static void BuildStudent(SceneContext context)
        {
            SpriteRenderer body = SceneFactory.Sprite("Student", null, ArtFinder.FirstFrame("wizard_idle"), 6.80f, -4.10f, 20);
            var player = body.gameObject.AddComponent<FramePlayer>();
            Wiring.Set(player, "library", context.data.library);
            context.Add(body.gameObject.AddComponent<StudentAnimator>());
        }

        static void BuildCauldron(SceneContext context)
        {
            Transform station = SceneFactory.Group("Cauldron", null, -1.40f, -0.98f);
            SceneFactory.Sprite("Back", station, ArtFinder.FindSprite("cauldron_back"), 0f, 0f, 40);
            var mask = SceneFactory.Group("Mask", station).gameObject.AddComponent<SpriteMask>();
            mask.sprite = ArtFinder.FindSprite("cauldron_mask");
            SpriteRenderer liquid = SceneFactory.Sprite("Liquid", station, ArtFinder.FindSprite("liquid"), 0f, 0.74f, 41);
            FramePlayer bubbles = SceneFactory.Animated("Bubbles", station, context.data.library, "fx_bubbles", 0f, 0.84f, 42);
            var bubblesRenderer = bubbles.GetComponent<SpriteRenderer>();
            liquid.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            bubblesRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            SpriteRenderer front = SceneFactory.Sprite("Front", station, ArtFinder.FindSprite("cauldron_front"), 0f, 0f, 43);
            MakeClickable(front.gameObject, context.data.cauldronClicked);

            var cauldron = context.Add(station.gameObject.AddComponent<Cauldron>());
            Wiring.Set(cauldron, "surface", liquid.transform);
            var view = station.gameObject.AddComponent<CauldronLiquid>();
            Wiring.SetArray(view, "parts", new[] { liquid, bubblesRenderer });

            SceneFactory.Animated("Cauldron Fire", null, context.data.library, "fx_cauldron_fire", -1.40f, -2.30f, 39);
        }

        static void BuildGauge(SceneContext context)
        {
            Transform gauge = SceneFactory.Group("Gauge", null, -3.98f, -1.20f);
            SceneFactory.Sprite("Back", gauge, ArtFinder.FindSprite("gauge_back"), 0f, 0f, 35);
            SpriteRenderer fill = SceneFactory.Sprite("Fill", gauge, ArtFinder.FindSprite("gauge_fill"), 0f, -0.85f, 36);
            SceneFactory.Sprite("Front", gauge, ArtFinder.FindSprite("gauge_front"), 0f, 0f, 37);
            Wiring.Set(gauge.gameObject.AddComponent<LevelGauge>(), "fill", fill);
        }

        static void BuildVialAndKettle(SceneContext context)
        {
            SpriteRenderer vial = SceneFactory.Sprite("Vial", null, ArtFinder.FindSprite("vial_glass"), 1.90f, -1.64f, 46);
            SpriteRenderer vialFill = SceneFactory.Sprite("Fill", vial.transform, ArtFinder.FindSprite("vial_fill"), 0f, 0f, 45);
            Wiring.Set(context.Add(vial.gameObject.AddComponent<PotionVial>()), "fill", vialFill);

            SpriteRenderer kettle = SceneFactory.Sprite("Kettle", null, ArtFinder.FindSprite("kettle"), 0.40f, 1.72f, 50);
            FramePlayer levitation = SceneFactory.Animated("Levitation", kettle.transform, context.data.library, "fx_levitate", 0f, -0.2f, 48);
            levitation.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            var kettleComponent = context.Add(kettle.gameObject.AddComponent<Kettle>());
            Wiring.Set(kettleComponent, "body", kettle);
            Wiring.Set(kettleComponent, "levitation", levitation.gameObject);
        }

        static void BuildSpellBeam(SceneContext context)
        {
            Transform root = SceneFactory.Group("Spell Beam");
            FramePlayer beam = SceneFactory.Player("Beam", root, context.data.library, 64);
            FramePlayer wandGlow = SceneFactory.Player("Wand Glow", root, context.data.library, 65);
            var spellBeam = context.Add(root.gameObject.AddComponent<SpellBeam>());
            Wiring.Set(spellBeam, "beam", beam);
            Wiring.Set(spellBeam, "wandGlow", wandGlow);
        }

        static void MakeClickable(GameObject go, GameEvent clicked)
        {
            SceneFactory.FitClickArea(go, 0.85f);
            Wiring.Set(go.AddComponent<ClickTarget>(), "clicked", clicked);
        }
    }
}
