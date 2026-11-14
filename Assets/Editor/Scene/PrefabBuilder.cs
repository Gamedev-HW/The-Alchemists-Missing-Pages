using UnityEditor;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>Saves the two prefabs: a shelf item (one per ingredient on the shelf) and the floating held ingredient.</summary>
    public static class PrefabBuilder
    {
        const string Folder = "Assets/Prefabs";

        public static void Build(SceneContext context)
        {
            AssetFiles.EnsureFolder(Folder);
            context.shelfItemPrefab = BuildShelfItem(context.data);
            context.Add(BuildHeldIngredient(context.data));
        }

        static ShelfItem BuildShelfItem(GameData data)
        {
            Transform root = SceneFactory.Group("Shelf Item");
            root.gameObject.AddComponent<SpriteRenderer>().sortingOrder = 11;
            root.gameObject.AddComponent<BoxCollider2D>();
            SpriteRenderer glow = SceneFactory.Sprite("Glow", root, null, 0f, 0f, 10);
            var item = root.gameObject.AddComponent<ShelfItem>();
            Wiring.Set(item, "glow", glow);
            Wiring.Set(item, "clicked", data.shelfItemClicked);
            return Save(root.gameObject, "ShelfItem").GetComponent<ShelfItem>();
        }

        static HeldIngredient BuildHeldIngredient(GameData data)
        {
            Transform root = SceneFactory.Group("Held Ingredient");
            FramePlayer levitation = SceneFactory.Animated("Levitation", root, data.library, "fx_levitate", 0f, -0.1f, 59);
            levitation.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            SpriteRenderer art = SceneFactory.Sprite("Art", root, null, 0f, 0f, 60);
            var held = root.gameObject.AddComponent<HeldIngredient>();
            Wiring.Set(held, "art", art);
            Wiring.Set(held, "levitation", levitation.gameObject);
            return Save(root.gameObject, "HeldIngredient").GetComponent<HeldIngredient>();
        }

        static GameObject Save(GameObject root, string fileName)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, Folder + "/" + fileName + ".prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }
    }
}
