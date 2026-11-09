// Generated from the art build metadata. Do not edit by hand.
using System.Collections.Generic;
using UnityEngine;

namespace PotionLab.EditorTools
{
    public static class PotionLabMeta
    {
        public class Sheet
        {
            public readonly string name;
            public readonly int frameW, frameH, cols, rows, frames;
            public readonly float fps;
            public readonly bool loop;
            public readonly Vector2 pivot;
            /// <summary>Wizard sheets: wand tip per frame, in frame pixels from the top-left corner.</summary>
            public readonly Vector2[] wandTipPx;

            public Sheet(string name, int frameW, int frameH, int cols, int rows, int frames, float fps, bool loop, Vector2 pivot, Vector2[] wandTipPx)
            {
                this.name = name; this.frameW = frameW; this.frameH = frameH; this.cols = cols; this.rows = rows;
                this.frames = frames; this.fps = fps; this.loop = loop; this.pivot = pivot; this.wandTipPx = wandTipPx;
            }
        }

        public class Ingredient
        {
            public readonly string id, english, hebrew, tint;
            public readonly bool liquid;

            public Ingredient(string id, string english, string hebrew, string tint, bool liquid)
            {
                this.id = id; this.english = english; this.hebrew = hebrew; this.tint = tint; this.liquid = liquid;
            }
        }

        public static readonly Sheet[] Sheets =
        {
            new Sheet("wizard_idle", 840, 900, 4, 2, 8, 8.0f, true, new Vector2(0.619f, 0f), new Vector2[] { new Vector2(244.0f, 294.0f), new Vector2(253.5f, 292.0f), new Vector2(257.5f, 291.1f), new Vector2(253.5f, 292.0f), new Vector2(244.0f, 294.0f), new Vector2(234.6f, 296.3f), new Vector2(230.8f, 297.4f), new Vector2(234.6f, 296.3f) }),
            new Sheet("wizard_cast_spot", 840, 900, 4, 2, 8, 12.0f, false, new Vector2(0.619f, 0f), new Vector2[] { new Vector2(244.0f, 294.0f), new Vector2(299.3f, 274.2f), new Vector2(344.2f, 265.4f), new Vector2(263.0f, 284.9f), new Vector2(105.9f, 405.5f), new Vector2(59.3f, 500.0f), new Vector2(62.9f, 488.9f), new Vector2(67.0f, 477.8f) }),
            new Sheet("wizard_cast_pot", 840, 900, 4, 2, 8, 12.0f, false, new Vector2(0.619f, 0f), new Vector2[] { new Vector2(244.0f, 294.0f), new Vector2(299.3f, 274.2f), new Vector2(344.2f, 265.4f), new Vector2(263.0f, 284.9f), new Vector2(93.0f, 425.2f), new Vector2(48.5f, 545.8f), new Vector2(50.6f, 534.2f), new Vector2(53.1f, 522.7f) }),
            new Sheet("wizard_stir", 840, 900, 4, 2, 8, 10.0f, true, new Vector2(0.619f, 0f), new Vector2[] { new Vector2(64.0f, 511.3f), new Vector2(70.5f, 487.1f), new Vector2(69.2f, 477.4f), new Vector2(59.1f, 487.1f), new Vector2(48.0f, 511.3f), new Vector2(43.8f, 536.2f), new Vector2(47.6f, 546.6f), new Vector2(55.2f, 536.2f) }),
            new Sheet("wizard_happy", 840, 900, 4, 2, 6, 10.0f, false, new Vector2(0.619f, 0f), new Vector2[] { new Vector2(244.0f, 294.0f), new Vector2(299.3f, 262.2f), new Vector2(345.5f, 243.4f), new Vector2(368.9f, 247.5f), new Vector2(322.2f, 263.0f), new Vector2(265.7f, 284.9f) }),
            new Sheet("wizard_oops", 840, 900, 4, 2, 6, 10.0f, false, new Vector2(0.619f, 0f), new Vector2[] { new Vector2(244.0f, 294.0f), new Vector2(295.0f, 277.4f), new Vector2(332.2f, 269.0f), new Vector2(318.7f, 271.4f), new Vector2(292.0f, 277.4f), new Vector2(265.7f, 284.9f) }),
            new Sheet("fx_wand_tip", 128, 128, 4, 2, 8, 16.0f, true, new Vector2(0.5f, 0.5f), null),
            new Sheet("fx_spell_beam", 512, 128, 2, 4, 8, 16.0f, true, new Vector2(0.0f, 0.5f), null),
            new Sheet("fx_slice", 384, 384, 4, 3, 12, 20.0f, false, new Vector2(0.5f, 0.5f), null),
            new Sheet("fx_heat_back", 384, 384, 4, 3, 12, 16.0f, false, new Vector2(0.5f, 0.5f), null),
            new Sheet("fx_heat_front", 384, 384, 4, 3, 12, 16.0f, false, new Vector2(0.5f, 0.5f), null),
            new Sheet("fx_heat", 384, 384, 4, 3, 12, 16.0f, false, new Vector2(0.5f, 0.5f), null),
            new Sheet("fx_levitate", 256, 256, 4, 2, 8, 12.0f, true, new Vector2(0.5f, 0.5f), null),
            new Sheet("fx_splash", 384, 256, 5, 2, 10, 20.0f, false, new Vector2(0.5f, 0.2188f), null),
            new Sheet("fx_pour_stream", 128, 384, 8, 1, 8, 16.0f, true, new Vector2(0.5f, 1.0f), null),
            new Sheet("fx_mix", 384, 384, 4, 4, 16, 16.0f, true, new Vector2(0.5f, 0.2448f), null),
            new Sheet("fx_serve", 384, 384, 7, 2, 14, 18.0f, false, new Vector2(0.5f, 0.5f), null),
            new Sheet("fx_bubbles", 320, 96, 4, 3, 12, 12.0f, true, new Vector2(0.5f, 0.5f), null),
            new Sheet("fx_smoke_fail", 384, 384, 4, 3, 12, 14.0f, false, new Vector2(0.5f, 0.5f), null),
            new Sheet("fx_steam", 256, 320, 6, 2, 12, 12.0f, true, new Vector2(0.5f, 0.0f), null),
            new Sheet("fx_candle_flame", 48, 96, 8, 1, 8, 10.0f, true, new Vector2(0.5f, 0.0833f), null),
            new Sheet("fx_cauldron_fire", 340, 150, 4, 2, 8, 12.0f, true, new Vector2(0.5f, 0.1467f), null),
            new Sheet("fx_poof", 256, 256, 4, 2, 8, 16.0f, false, new Vector2(0.5f, 0.5f), null),
        };

        public static readonly Ingredient[] Ingredients =
        {
            new Ingredient("star_essence", "Star Essence", "תמצית כוכב", "#5C7DFF", true),
            new Ingredient("silver_dew", "Silver Dew", "טל כסף", "#D4DBE8", true),
            new Ingredient("frost_lily", "Frost Lily", "שושנת כפור", "#9FD2F5", false),
            new Ingredient("griffin_feather", "Griffin Feather", "נוצת גריפון", "#E0A84A", false),
            new Ingredient("nightbell", "Nightbell Leaves", "עלי פעמון-לילה", "#8A5AD6", false),
            new Ingredient("moonstone", "Moonstone", "אבן ירח", "#C8BFFF", false),
            new Ingredient("amber", "Amber Resin", "שרף ענבר", "#F39A2A", false),
            new Ingredient("ember_root", "Ember Root", "שורש גחלת", "#FF6A2A", false),
            new Ingredient("glowcap", "Glowcap Mushrooms", "פטריות זוהר", "#2FD8C0", false),
            new Ingredient("dragon_scale", "Dragon Scale", "קשקש דרקון", "#2FAE82", false),
            new Ingredient("bog_berries", "Bog Berries", "גרגרי ביצה", "#8A2AB8", false),
            new Ingredient("sun_honey", "Sun Honey", "דבש שמש", "#FFBF2A", true),
        };

        /// <summary>Every PNG in the art pack (163 files). The import settings are applied to these names wherever they are in Assets.</summary>
        public static readonly HashSet<string> ArtNames = new HashSet<string>
        {
            "bg_lab", "cauldron_back", "cauldron_front", "cauldron_mask",
            "fg_counter", "fg_vignette", "fx_bubbles", "fx_candle_flame",
            "fx_cauldron_fire", "fx_heat", "fx_heat_back", "fx_heat_front",
            "fx_levitate", "fx_mix", "fx_poof", "fx_pour_stream",
            "fx_serve", "fx_slice", "fx_smoke_fail", "fx_spell_beam",
            "fx_splash", "fx_steam", "fx_wand_tip", "gauge_back",
            "gauge_fill", "gauge_front", "ing_amber", "ing_amber_glow",
            "ing_amber_heated", "ing_amber_shelf", "ing_amber_sliced", "ing_amber_sliced_heated",
            "ing_bog_berries", "ing_bog_berries_glow", "ing_bog_berries_heated", "ing_bog_berries_shelf",
            "ing_bog_berries_sliced", "ing_bog_berries_sliced_heated", "ing_dragon_scale", "ing_dragon_scale_glow",
            "ing_dragon_scale_heated", "ing_dragon_scale_shelf", "ing_dragon_scale_sliced", "ing_dragon_scale_sliced_heated",
            "ing_ember_root", "ing_ember_root_glow", "ing_ember_root_heated", "ing_ember_root_shelf",
            "ing_ember_root_sliced", "ing_ember_root_sliced_heated", "ing_frost_lily", "ing_frost_lily_glow",
            "ing_frost_lily_heated", "ing_frost_lily_shelf", "ing_frost_lily_sliced", "ing_frost_lily_sliced_heated",
            "ing_glowcap", "ing_glowcap_glow", "ing_glowcap_heated", "ing_glowcap_shelf",
            "ing_glowcap_sliced", "ing_glowcap_sliced_heated", "ing_griffin_feather", "ing_griffin_feather_glow",
            "ing_griffin_feather_heated", "ing_griffin_feather_shelf", "ing_griffin_feather_sliced", "ing_griffin_feather_sliced_heated",
            "ing_moonstone", "ing_moonstone_glow", "ing_moonstone_heated", "ing_moonstone_shelf",
            "ing_moonstone_sliced", "ing_moonstone_sliced_heated", "ing_nightbell", "ing_nightbell_glow",
            "ing_nightbell_heated", "ing_nightbell_shelf", "ing_nightbell_sliced", "ing_nightbell_sliced_heated",
            "ing_silver_dew", "ing_silver_dew_glow", "ing_silver_dew_heated", "ing_silver_dew_shelf",
            "ing_silver_dew_sliced", "ing_silver_dew_sliced_heated", "ing_star_essence", "ing_star_essence_glow",
            "ing_star_essence_heated", "ing_star_essence_shelf", "ing_star_essence_sliced", "ing_star_essence_sliced_heated",
            "ing_sun_honey", "ing_sun_honey_glow", "ing_sun_honey_heated", "ing_sun_honey_shelf",
            "ing_sun_honey_sliced", "ing_sun_honey_sliced_heated", "kettle", "ladle",
            "liquid", "p_ember", "p_glow", "p_smoke",
            "p_spark", "p_star", "p_star5", "prop_lectern_book",
            "ui_badge_fail", "ui_badge_heated", "ui_badge_in_pot", "ui_badge_ok",
            "ui_badge_raw", "ui_badge_sliced", "ui_bar", "ui_btn_active",
            "ui_btn_disabled", "ui_btn_hover", "ui_btn_normal", "ui_btn_pressed",
            "ui_cursor_wand", "ui_divider", "ui_icon_cauldron", "ui_intro_panel",
            "ui_name_panel", "ui_plaque_blank", "ui_plaque_heat_en", "ui_plaque_heat_he",
            "ui_plaque_mix_en", "ui_plaque_mix_he", "ui_plaque_pour_en", "ui_plaque_pour_he",
            "ui_plaque_serve_en", "ui_plaque_serve_he", "ui_plaque_slice_en", "ui_plaque_slice_he",
            "ui_plaque_start_en", "ui_plaque_start_he", "ui_plaque_stop_en", "ui_plaque_stop_he",
            "ui_plaque_topot_en", "ui_plaque_topot_he", "ui_speech_bubble", "ui_title_en",
            "ui_title_he", "vial_fill", "vial_glass", "wiz_arm",
            "wiz_blink", "wiz_body", "wiz_eyes_happy", "wiz_head",
            "wiz_mouth_grin", "wiz_mouth_open", "wiz_mouth_smile", "wiz_mouth_worried",
            "wiz_pupils", "wizard_cast_pot", "wizard_cast_spot", "wizard_happy",
            "wizard_idle", "wizard_oops", "wizard_stir",
        };

        public static bool IsArt(string name)
        {
            return ArtNames.Contains(name);
        }

        /// <summary>ASTC block size on Android: 6x6 for the six big wizard sheets (a quarter of the memory of 4x4), 4x4 for the rest.</summary>
        public static int AndroidBlock(string name)
        {
            return name.StartsWith("wizard_") ? 6 : 4;
        }

        public static Sheet FindSheet(string name)
        {
            foreach (var s in Sheets) if (s.name == name) return s;
            return null;
        }

        public static bool IsSheet(string name)
        {
            return FindSheet(name) != null;
        }

        /// <summary>Custom pivots for single sprites (everything else is centred).</summary>
        public static bool SinglePivot(string name, out Vector2 pivot)
        {
            if (name.StartsWith("ing_") && (name.EndsWith("_shelf") || name.EndsWith("_glow")))
            {
                pivot = new Vector2(0.5f, 0.07f);   // bottom of the item, 14 px above the canvas edge
                return true;
            }
            if (name == "gauge_fill")
            {
                pivot = new Vector2(0.5f, 0f);
                return true;
            }
            pivot = new Vector2(0.5f, 0.5f);
            return false;
        }

        /// <summary>9-slice borders (left, bottom, right, top) for UI panels.</summary>
        public static bool Border(string name, out Vector4 border)
        {
            switch (name)
            {
                case "ui_name_panel": border = new Vector4(44, 44, 44, 44); return true;
                case "ui_intro_panel": border = new Vector4(100, 100, 100, 100); return true;
                case "ui_plaque_blank": border = new Vector4(30, 0, 30, 0); return true;
                default: border = Vector4.zero; return false;
            }
        }
    }
}
