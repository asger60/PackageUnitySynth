using Synth;
using UnityEngine;
using UnitySynth.Runtime.Synth;

namespace UnitySynth.Runtime.AudioSystem
{
    [CreateAssetMenu(fileName = "New synth preset", menuName = "UnitySynth/SynthPreset")]
    public class UnitySynthPreset : ScriptableObject
    {
        


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