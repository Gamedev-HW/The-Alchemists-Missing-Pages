using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>Cuts the animation sheets into frames (sheet_00, sheet_01...) with the right pivot, like the Sprite Editor would.</summary>
    public static class SpriteSheetSlicer
    {
        /// <summary>Re-imports every art PNG so ArtImportSettings applies (art copied in before the Editor folder needs this).</summary>
        public static void ReimportAllArt()
        {
            var paths = new List<string>();
            foreach (string name in ArtCatalog.ArtNames)
            {
                string path = ArtFinder.FindTexture(name);
                if (path != null) paths.Add(path);
            }
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string path in paths) AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        public static void Slice(ArtCatalog.Sheet sheet)
        {
            string path = ArtFinder.FindTexture(sheet.name);
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

            // keep the IDs of frames that already exist, so references to them survive a re-run
            var oldIds = new Dictionary<string, GUID>();
            foreach (SpriteRect rect in provider.GetSpriteRects()) oldIds[rect.name] = rect.spriteID;

            SpriteRect[] frames = FrameRects(sheet, oldIds);
            provider.SetSpriteRects(frames);
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names != null) names.SetNameFileIdPairs(frames.Select(f => new SpriteNameFileIdPair(f.name, f.spriteID)).ToList());
            provider.Apply();
            importer.SaveAndReimport();
        }

        static SpriteRect[] FrameRects(ArtCatalog.Sheet sheet, Dictionary<string, GUID> oldIds)
        {
            int textureHeight = sheet.rows * sheet.frameH;
            var frames = new SpriteRect[sheet.frames];
            for (int i = 0; i < sheet.frames; i++)
            {
                int column = i % sheet.cols;
                int row = i / sheet.cols;
                string frameName = sheet.name + "_" + i.ToString("00");
                if (!oldIds.TryGetValue(frameName, out GUID id)) id = GUID.Generate();
                frames[i] = new SpriteRect
                {
                    name = frameName,
                    spriteID = id,
                    // Unity measures sprite rects from the bottom-left corner of the texture
                    rect = new Rect(column * sheet.frameW, textureHeight - (row + 1) * sheet.frameH, sheet.frameW, sheet.frameH),
                    alignment = SpriteAlignment.Custom,
                    pivot = sheet.pivot
                };
            }
            return frames;
        }
    }
}
