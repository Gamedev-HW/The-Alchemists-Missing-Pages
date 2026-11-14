using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>
    /// One thing the player can do (pick up, slice, pour...). Each action is its own small component and runs as a
    /// coroutine through the ActionRunner, which makes sure only one action runs at a time.
    /// </summary>
    public abstract class BrewAction : MonoBehaviour
    {
        [SerializeField] protected ActionRunner runner;
        [SerializeField] TextEvent speech;
        [SerializeField] TextEvent journal;

        /// <summary>The bar button that starts this action, if it has one.</summary>
        public virtual BrewActionId? Button => null;

        /// <summary>The button shown instead while it runs, to stop it early (POUR turns into STOP).</summary>
        public virtual BrewActionId? StopButton => null;

        public abstract IEnumerator Run();

        /// <summary>Asks the runner to start this action. Returns false while another action is running.</summary>
        protected bool Request()
        {
            return runner.TryRun(this);
        }

        protected void Say(string message)
        {
            speech.Raise(message);
        }

        protected void Log(string line)
        {
            journal.Raise(line);
        }
    }
}
