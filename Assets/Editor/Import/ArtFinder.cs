using System.IO;
using UnityEditor;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>Finds the art pack's images by file name, wherever the Art folder was copied in the project.</summary>
    public static class ArtFinder
    {
        /// <summary>The folder that holds the art (found from bg_lab.png). Searched first, then the whole project.</summary>
        static string artRoot = "Assets";

        /// <summary>Looks for the art. Returns false if bg_lab.png isn't in the project.</summary>
        public static bool Locate()
        {
            artRoot = "Assets";
            string background = FindTexture("bg_lab");
            if (background == null) return false;
            // .../Art/Background/bg_lab.png -> .../Art
            string folder = ParentFolder(background);
            string root = ParentFolder(folder);
            artRoot = root.StartsWith("Assets") ? root : folder;
            return true;
        }

        public static string FindTexture(string fileName)
        {
            return FindTextureIn(fileName, artRoot) ?? (artRoot != "Assets" ? FindTextureIn(fileName, "Assets") : null);
        }

        public static Sprite FindSprite(string fileName)
        {
            string path = FindTexture(fileName);
            return path == null ? null : AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>The first frame of a sliced sprite sheet, e.g. wizard_idle_00.</summary>
        public static Sprite FirstFrame(string sheetName)
        {
            string path = FindTexture(sheetName);
            if (path == null) return null;
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite && sprite.name == sheetName + "_00") return sprite;
            }
            return null;
        }

        public static string ParentFolder(string path)
        {
            string parent = Path.GetDirectoryName(path);
            return string.IsNullOrEmpty(parent) ? "" : parent.Replace('\\', '/');
        }

        static string FindTextureIn(string fileName, string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return null;
            foreach (string guid in AssetDatabase.FindAssets(fileName + " t:Texture2D", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == fileName) return path;
            }
            return null;
        }
    }
}
