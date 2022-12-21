using Synth;
using UnityEngine;

namespace UnitySynth.Runtime.Synth
{
    [CreateAssetMenu(fileName = "New Oscillator", menuName = "Rytmos/Synth/SynthSettingsObjectOscillator")]
    public class SynthSettingsObjectOscillator : SynthSettingsObjectBase
    {
        public int tuning;
        public float amplitude;

        public enum OscillatorType
        {
            Simple,
            WaveTable,
            Noise
        }

        public enum SimpleOscillatorTypes
        {
            Sine,
            Saw,
            Square
        }

        public enum WaveTableOscillatorTypes
        {
            Sine8Bit,
            Saw8Bit,
            Square8Bit
        }

        public enum NoiseTypes
        {
            White,
            Brown
        }

        public SimpleOscillatorTypes simpleOscillatorType;
        public NoiseTypes noiseType;


        public WaveTableOscillatorTypes waveTableOscillatorType;
        public OscillatorType oscillatorType;
    }
}