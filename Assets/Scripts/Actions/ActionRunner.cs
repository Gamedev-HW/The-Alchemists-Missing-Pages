using System;
using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>Runs one BrewAction at a time and tells the UI what is running, so buttons can be disabled meanwhile.</summary>
    public class ActionRunner : MonoBehaviour
    {
        public event Action RunningChanged;

        public BrewAction Running { get; private set; }
        public bool IsBusy => Running != null;

        public bool TryRun(BrewAction action)
        {
            if (IsBusy || action == null) return false;
            StartCoroutine(RunToEnd(action));
            return true;
        }

        IEnumerator RunToEnd(BrewAction action)
        {
            SetRunning(action);
            yield return action.Run();
            SetRunning(null);
        }

        void SetRunning(BrewAction action)
        {
            Running = action;
            RunningChanged?.Invoke();
        }
    }
}
