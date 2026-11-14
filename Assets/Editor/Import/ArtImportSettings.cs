using System.IO;
using UnityEditor;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>
    /// Applies the right import settings to every Potion Lab PNG automatically, in whatever folder you put the art:
    /// Sprite, 100 pixels per unit, full-rect meshes, the custom pivots and 9-slice borders, and texture compression
    /// for both platforms - high quality (BC7) on PC, ASTC on Android so the game fits in a phone's memory.
    /// (Sprite sheets keep Sprite Mode = Multiple; "Potion Lab > 1 - Set Up Assets" cuts them into frames.)
    /// </summary>
    public class ArtImportSettings : AssetPostprocessor
    {
        // raise this when the settings below change, so Unity re-imports the art by itself
        public override uint GetVersion()
        {
            return 2;
        }

        void OnPreprocessTexture()
        {
            string file = Path.GetFileNameWithoutExtension(assetPath);
            if (!ArtCatalog.IsArt(file)) return;

            var importer = (TextureImporter)assetImporter;
            importer.maxTextureSize = 4096;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.crunchedCompression = false;

            if (file == "ui_cursor_wand")
            {
                // hardware cursors must stay uncompressed (and the cursor isn't used on phones)
                importer.textureType = TextureImporterType.Cursor;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                return;
            }

            // PC and the editor: BC7 (or uncompressed for the few small images whose size isn't a multiple of 4)
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            // Android: ASTC works on every phone Unity 6 supports and keeps the full resolution
            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = 4096;
            android.format = ArtCatalog.AndroidBlock(file) == 6 ? TextureImporterFormat.ASTC_6x6 : TextureImporterFormat.ASTC_4x4;
            android.compressionQuality = 50;
            importer.SetPlatformTextureSettings(android);

            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100f;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            if (ArtCatalog.IsSheet(file))
            {
                settings.spriteMode = (int)SpriteImportMode.Multiple;
            }
            else
            {
                settings.spriteMode = (int)SpriteImportMode.Single;
                Vector2 pivot;
                if (ArtCatalog.SinglePivot(file, out pivot))
                {
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    settings.spritePivot = pivot;
                }
                else
                {
                    settings.spriteAlignment = (int)SpriteAlignment.Center;
                }
                Vector4 border;
                if (ArtCatalog.Border(file, out border)) settings.spriteBorder = border;
            }
            importer.SetTextureSettings(settings);
        }
    }
}
