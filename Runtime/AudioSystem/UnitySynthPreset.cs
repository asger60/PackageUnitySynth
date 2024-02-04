using UnityEngine;
using UnitySynth.Runtime.Synth;

namespace UnitySynth.Runtime.AudioSystem
{
    [CreateAssetMenu(fileName = "New synth preset", menuName = "UnitySynth/SynthPreset")]
    public class UnitySynthPreset : ScriptableObject
    {
        [HideInInspector] public bool isInit;
        [HideInInspector] public bool showOscillator;
        [HideInInspector] public SynthSettingsObjectOscillator[] oscillatorSettings;
        [HideInInspector] public SynthSettingsObjectFilter[] filterSettings;
        [HideInInspector] public SynthSettingsObjectBase[] pitchModifiers;
        [HideInInspector] public SynthSettingsObjectBase[] amplitudeModifiers;
        [HideInInspector] public SynthSettingsObjectBase[] filterModifiers;


        private Synth.UnitySynth _runtimeSynth;

        public void BindToRuntime(Synth.UnitySynth synth)
        {
            _runtimeSynth = synth;
        }

        public void RebuildSynth()
        {
            if (_runtimeSynth != null)
                _runtimeSynth.RebuildSynth();
        }

        [ContextMenu("Clean up preset object")]
        void CleanUpPreset()
        {
        }

        private void Reset()
        {
            isInit = false;
        }
    }
}