using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>Creates scene objects. Positions are in units, relative to the parent (or the world when there is none).</summary>
    public static class SceneFactory
    {
        public static Transform Group(string name, Transform parent = null, float x = 0f, float y = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(x, y, 0f);
            return go.transform;
        }

        public static SpriteRenderer Sprite(string name, Transform parent, Sprite sprite, float x, float y, int sortingOrder)
        {
            var spriteRenderer = Group(name, parent, x, y).gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = sortingOrder;
            return spriteRenderer;
        }

        /// <summary>A sprite that plays one of the library's animations from the start of the game (candles, fire, bubbles).</summary>
        public static FramePlayer Animated(string name, Transform parent, AnimLibrary library, string clip, float x, float y,
            int sortingOrder, bool randomStart = false)
        {
            SpriteRenderer spriteRenderer = Sprite(name, parent, ArtFinder.FirstFrame(clip), x, y, sortingOrder);
            var player = spriteRenderer.gameObject.AddComponent<FramePlayer>();
            Wiring.Set(player, "library", library);
            Wiring.SetString(player, "clipName", clip);
            Wiring.SetBool(player, "playOnStart", true);
            Wiring.SetBool(player, "randomStart", randomStart);
            return player;
        }

        /// <summary>A FramePlayer that code starts later (the spell beam, the student).</summary>
        public static FramePlayer Player(string name, Transform parent, AnimLibrary library, int sortingOrder)
        {
            SpriteRenderer spriteRenderer = Sprite(name, parent, null, 0f, 0f, sortingOrder);
            var player = spriteRenderer.gameObject.AddComponent<FramePlayer>();
            Wiring.Set(player, "library", library);
            return player;
        }

        /// <summary>Adds (or resizes) a box collider that covers the sprite, scaled by 'scale', so the object can be clicked.</summary>
        public static BoxCollider2D FitClickArea(GameObject go, float scale)
        {
            var area = go.GetComponent<BoxCollider2D>();
            if (area == null) area = go.AddComponent<BoxCollider2D>();
            Sprite sprite = go.GetComponent<SpriteRenderer>().sprite;
            if (sprite != null)
            {
                area.size = sprite.bounds.size * scale;
                area.offset = sprite.bounds.center;
            }
            return area;
        }
    }
}
