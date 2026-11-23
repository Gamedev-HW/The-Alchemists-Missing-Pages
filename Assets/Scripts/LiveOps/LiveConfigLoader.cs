using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace PotionLab
{
    /// <summary>
    /// Gets the live config when the game starts: online first, then the copy saved last time, then the one built into
    /// the game. Announces it on the "Live Config Loaded" event; it doesn't decide what the config changes.
    /// </summary>
    public class LiveConfigLoader : MonoBehaviour
    {
        const string SavedCopyKey = "PotionLab.LiveConfig";

        [Tooltip("Where the live config is published. Editing this file changes every player's game, without a new build.")]
        [SerializeField] string url = "https://raw.githubusercontent.com/Gamedev-HW/The-Alchemists-Missing-Pages/main/LiveOps/live-config.json";
        [Tooltip("Used when the player is offline and nothing was saved yet.")]
        [SerializeField] TextAsset builtInConfig;
        [SerializeField] float timeoutSeconds = 5f;
        [SerializeField] LiveConfigEvent configLoaded;

        IEnumerator Start()
        {
            string downloaded = null;
            yield return Download(text => downloaded = text);

            LiveConfig config = Parse(downloaded, "online");
            if (config != null)
            {
                PlayerPrefs.SetString(SavedCopyKey, downloaded);
                PlayerPrefs.Save();
            }
            else
            {
                config = Parse(PlayerPrefs.GetString(SavedCopyKey, ""), "the copy saved last time")
                         ?? Parse(builtInConfig != null ? builtInConfig.text : "", "the built-in file")
                         ?? new LiveConfig();
            }
            configLoaded.Raise(config);
        }

        IEnumerator Download(Action<string> done)
        {
            if (string.IsNullOrEmpty(url)) yield break;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = Mathf.CeilToInt(timeoutSeconds);
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success) done(request.downloadHandler.text);
                else Debug.Log($"[Potion Lab] Live config not downloaded ({request.error}).");
            }
        }

        static LiveConfig Parse(string json, string source)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                LiveConfig config = JsonUtility.FromJson<LiveConfig>(json);
                Debug.Log($"[Potion Lab] Live config version {config.version} loaded from {source}.");
                return config;
            }
            catch (ArgumentException error)
            {
                Debug.LogWarning($"[Potion Lab] The live config from {source} isn't valid JSON: {error.Message}");
                return null;
            }
        }
    }
}
