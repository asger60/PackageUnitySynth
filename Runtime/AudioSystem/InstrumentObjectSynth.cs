using Synth;
using UnityEngine;

namespace LooperAPP.AudioSystem
{
    [CreateAssetMenu(fileName = "New instrument object", menuName = "Rytmos/AudioObjects/InstrumentObjectSynth")]
    public class InstrumentObjectSynth : ScriptableObject
    {
        public int noteOffset = 0;
        [Header("Filter")] [Range(10, 24000)] public float cutoffFrequency;
        [Range(0, 1)] public float resonance;

        [Range(1, 4)] public int oversampling = 1;

        public enum FilterTypes
        {
            LowPass,
            BandPass,
            Formant
        }

        public FilterTypes filterType;
        
        //[Header("Pulse-width Modulation")]
        //[Range(0, 1)]
        //public float pwmStrength;
        //[Range(0, 1)]
        //public float pwmFrequency;


        public SynthSettingsObjectOscillator[] oscillatorSettings;

        [HideInInspector] public SynthSettingsObjectBase[] pitchModifiers;

        [HideInInspector] public SynthSettingsObjectBase[] amplitudeModifiers;

        [HideInInspector] public SynthSettingsObjectBase[] filterModifiers;


        private MoogSynth _runtimeSynth;

        public void RuntimeBind(MoogSynth synth)
        {
            _runtimeSynth = synth;
        }

        public void ReBuildSynth()
        {
            if (_runtimeSynth != null)
                _runtimeSynth.ReBuildSynth();
        }


        public  float GetVolume()
        {
            return 0;
        }

        public  void PreviewSound()
        {
        }
    }
}