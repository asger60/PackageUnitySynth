using System;
using Synth;
using UnityEngine;

namespace UnitySynth.Runtime.Synth
{
    [CreateAssetMenu(fileName = "New filter", menuName = "UnitySynth/SynthSettingsObjectFilter")]
    public class SynthSettingsObjectFilter : SynthSettingsObjectBase
    {
        public enum FilterTypes
        {
            LowPass,
            BandPass,
            Formant
        }

        public FilterTypes filterType;

        [Serializable]
        public struct LowPassSettings
        {
            [Range(10, 24000)] public float cutoffFrequency;
            [Range(0, 1)] public float resonance;
        }

        public LowPassSettings lowPassSettings;

        [Serializable]
        public struct BandPassSettings
        {
            [Range(10, 24000)] public float frequency;
            [Range(10, 100)] public float bandWidth;
        }

        public BandPassSettings bandPassSettings;

        [Serializable]
        public struct FormantSettings
        {
            [Range(0, 1)] public int vowel;
        }

        public FormantSettings formantSettings;
    }
}