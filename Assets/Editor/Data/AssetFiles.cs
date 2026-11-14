using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>Small helpers for creating and finding the game's data assets.</summary>
    public static class AssetFiles
    {
        const string DefaultDataFolder = "Assets/Data";

        /// <summary>Where the data assets live: next to the existing AnimLibrary, or Assets/Data.</summary>
        public static string DataFolder
        {
            get
            {
                AnimLibrary library = FindFirst<AnimLibrary>();
                return library != null ? ArtFinder.ParentFolder(AssetDatabase.GetAssetPath(library)) : DefaultDataFolder;
            }
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = ArtFinder.ParentFolder(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        /// <summary>Loads the asset at 'path', or creates it there. 'created' tells which.</summary>
        public static T LoadOrCreate<T>(string path, out bool created) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            created = asset == null;
            if (created)
            {
                EnsureFolder(ArtFinder.ParentFolder(path));
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            return LoadOrCreate<T>(path, out bool _);
        }

        /// <summary>Every asset of this type in the project, sorted by name.</summary>
        public static List<T> FindAll<T>() where T : Object
        {
            var found = new List<T>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { "Assets" }))
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) found.Add(asset);
            }
            found.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return found;
        }

        public static T FindFirst<T>() where T : Object
        {
            List<T> all = FindAll<T>();
            return all.Count > 0 ? all[0] : null;
        }
    }
}
