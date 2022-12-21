using UnityEngine;

namespace Synth
{
    [CreateAssetMenu(fileName = "New Envelope Settings Object", menuName = "Rytmos/Synth/Envelope Settings")]

    public class SynthSettingsObjectEnvelope : SynthSettingsObjectBase
    {
        public float attack;
        public float decay;
        public float sustain;
        public float release;
    
    }
}
