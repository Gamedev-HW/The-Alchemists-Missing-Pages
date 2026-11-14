using System.Collections;
using UnityEngine;

namespace PotionLab
{
    /// <summary>What happens when something goes wrong: a puff of smoke, the student's "oops", and a note in the journal.</summary>
    public class MishapReaction : MonoBehaviour
    {
        [SerializeField] StudentAnimator student;
        [SerializeField] EffectSpawner effects;
        [SerializeField] TextEvent speech;
        [SerializeField] TextEvent journal;
        [SerializeField] EffectSpec smoke = new EffectSpec("fx_smoke_fail", 66);
        [SerializeField] string oopsClip = "wizard_oops";

        public IEnumerator Play(Vector3 at, string said, string journalLine)
        {
            effects.Spawn(smoke, at);
            student.Play(oopsClip);
            speech.Raise(said);
            journal.Raise(journalLine);
            yield return student.WaitUntilDone();
            student.Idle();
        }
    }
}
