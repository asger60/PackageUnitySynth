using UnityEngine;

namespace Synth
{
    [CreateAssetMenu(fileName = "New LFO Settings Object", menuName = "Rytmos/Synth/LFO Settings")]

    public class SynthSettingsObjectLFO : SynthSettingsObjectBase
    {
        public float amp;
        public float frequency;
        public float fadeInDuration;
    }
}
