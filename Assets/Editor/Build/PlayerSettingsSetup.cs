using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>Player Settings for both platforms: landscape on phones, full screen on PC, 64-bit Android.</summary>
    public static class PlayerSettingsSetup
    {
        public static void Configure()
        {
            // phones and tablets: landscape only, turning over with the device
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            // PC: full screen by default, 1920x1080 when windowed, and the window can be resized
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.resizableWindow = true;

            // Android: 64-bit, which Google Play and today's phones need
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            AssetDatabase.SaveAssets();
            Debug.Log("[Potion Lab] Player Settings: landscape on phones, full screen on PC, 64-bit Android.");
        }

        /// <summary>Puts the scene first in the build's scene list.</summary>
        public static void AddSceneToBuild(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == scenePath || !System.IO.File.Exists(s.path));
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
