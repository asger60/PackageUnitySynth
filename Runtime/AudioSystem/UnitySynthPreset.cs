using Synth;
using UnityEngine;
using UnitySynth.Runtime.Synth;

namespace UnitySynth.Runtime.AudioSystem
{
    [CreateAssetMenu(fileName = "New instrument object", menuName = "UnitySynth/SynthPreset")]
    public class UnitySynthPreset : ScriptableObject
    {
        //[Header("Filter")] [Range(10, 24000)] public float cutoffFrequency;
        //[Range(0, 1)] public float resonance;

        [Range(1, 4)] public int oversampling = 1;


        
        //[Header("Pulse-width Modulation")]
        //[Range(0, 1)]
        //public float pwmStrength;
        //[Range(0, 1)]
        //public float pwmFrequency;


        public SynthSettingsObjectOscillator[] oscillatorSettings;


        public SynthSettingsObjectFilter[] filterSettings;
        
        
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

        
    }
}