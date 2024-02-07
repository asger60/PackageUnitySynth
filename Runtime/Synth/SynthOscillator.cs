using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace UnitySynth.Runtime.Synth
{
    public class SynthOscillator : MonoBehaviour
    {
        public UInt32 phase => _phase;
        private UInt32 _phase = 0u; // using an integer type automatically ensures limits
        // phase is in [0 ; 2^(32-1)]

        const float PHASE_MAX = 4294967296;
        private float _amp = 1.0f;
        private UInt32 _freqPhPSmp = 0u;
        private float _pitchModAmount;

        private int _currentNote;


        private int waveTableSize = 128;
        private float[] _sine8Bit;
        private float[] _square8Bit;
        private float[] _saw8Bit;
        private float[] _noiseWhite;

        public SynthSettingsObjectOscillator settings;

        private void Start()
        {
            _sine8Bit = new float[waveTableSize];
            _saw8Bit = new float[waveTableSize];
            _square8Bit = new float[2];
            for (int i = 0; i < waveTableSize; ++i)
            {
                float angle01 = ((float) i) / waveTableSize;
                _sine8Bit[i] = Mathf.Round(Mathf.Sin(angle01 * 2 * Mathf.PI) * 128) / 128;
            }

            for (int i = 0; i < waveTableSize; ++i)
            {
                _saw8Bit[i] = Mathf.Lerp(-1, 1, (float) i / waveTableSize);
            }

            _square8Bit[0] = -1;
            _square8Bit[1] = 1;

            _noiseWhite = new float[8192];
            for (int i = 0; i < _noiseWhite.Length; ++i)
            {
                _noiseWhite[i] = Random.Range(-1f, 1f);
            }
        }

        public void ResetPhase()
        {
            _phase = 0u;
        }
 
        public void DoUpdate()
        {
            _phase += _freqPhPSmp;
        }


        public float Process()
        {
            switch (settings.oscillatorType)
            {
                case SynthSettingsObjectOscillator.OscillatorType.Simple:
                    switch (settings.simpleOscillatorType)
                    {
                        case SynthSettingsObjectOscillator.SimpleOscillatorTypes.Sine:
                            return sin() * settings.amplitude;
                        case SynthSettingsObjectOscillator.SimpleOscillatorTypes.Saw:
                            return sawPolyBLEP() * settings.amplitude;
                        case SynthSettingsObjectOscillator.SimpleOscillatorTypes.Square:
                            return squarePolyBLEP(0.5f) * settings.amplitude;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case SynthSettingsObjectOscillator.OscillatorType.WaveTable:
                    switch (settings.waveTableOscillatorType)
                    {
                        case SynthSettingsObjectOscillator.WaveTableOscillatorTypes.Saw8Bit:
                            return WaveTable() * settings.amplitude;
                        case SynthSettingsObjectOscillator.WaveTableOscillatorTypes.Sine8Bit:
                            return WaveTable() * settings.amplitude;
                        case SynthSettingsObjectOscillator.WaveTableOscillatorTypes.Square8Bit:
                            return WaveTable() * settings.amplitude;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case SynthSettingsObjectOscillator.OscillatorType.Noise:
                    return Noise() * settings.amplitude;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetNote(int note)
        {
            _currentNote = note + settings.tuning;
            set_freq(UnitySynth.FreqTab[_currentNote & 0x7f]);
        }

        public void SetPitchMod(float amount)
        {
            _pitchModAmount = Remap(amount, -1, 1, 0.5f, 2);
            set_freq(UnitySynth.FreqTab[_currentNote & 0x7f]);
        }

        float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }


        private void set_freq(float freq__hz, int sample_rate = 48000)
        {
            float freq__ppsmp = (freq__hz * _pitchModAmount) / sample_rate; // periods per sample
            _freqPhPSmp = (uint) (freq__ppsmp * PHASE_MAX);
        }

        /// Basic oscillators
        /// <returns></returns>
        // Library sine
        // - possibly slow
        public float sin()
        {
            float ph01 = _phase / PHASE_MAX;
            return Mathf.Sin(ph01 * 6.28318530717959f) * _amp;
        }

        // Differentiated Polynomial Waveform (DPW)
        // Based on Valimaki & Huovilainen: 'Oscillator and Filter Algorithms for Virtual Analog Synthesis'
        // 2nd degree, meaning that the polynomial is 2nd degree
        // public float sawDPW()
        // {
        //     if (is_active == false) return 0.0f;
        //     float ph01 = phase / PHASE_MAX;
        //     float bphase = 2.0f * ph01 - 1.0f;  // phasor in [-1;+1]       : saw
        //     float sq = bphase * bphase;         // squared saw             : parabola
        //     float dsq = sq - z1;                // differentiated parabola : bandlimited saw
        //     z1 = sq;                            // store next frame's z^-1
        //     return dsq * dpwScale * amp;
        // }
        // float z1 = 0;
        // float dpwScale = 1.0f;

        /// PolyBLEP oscillators
        // Polynomial Band-Limited Step Function (PolyBLEP)
        // Based on Valimaki 2007: 'Antialiasing Oscillators in Subtractive Synthesis'
        // and https://steemit.com/ableton/@metafunction/all-about-digital-oscillators-part-2-blits-and-bleps
        public float sawPolyBLEP()
        {
            float ph01 = _phase / PHASE_MAX;
            float result = 2.0f * ph01 - 1.0f; // phasor in [-1;+1] : saw

            result -= polyBLEP(ph01);
            return result;
        }

        // FIXME: DC offset when pulseWidth != 0.5, should be fixable by a simple offset
        public float squarePolyBLEP(float pulseWidth)
        {
            float ph01 = _phase / PHASE_MAX;

            float value;
            if (ph01 < pulseWidth)
            {
                value = _amp;
            }
            else
            {
                value = -_amp;
            }

            value += polyBLEP(ph01); // Layer output of Poly BLEP on top (flip)
            value -= polyBLEP((ph01 + 1.0f - pulseWidth) % 1.0f); // Layer output of Poly BLEP on top (flop)

            return value;
        }

        private float polyBLEP(float t)
        {
            // phase step in [0;1]
            float dt = _freqPhPSmp / PHASE_MAX;

            // t-t^2/2 +1/2
            // 0 < t <= 1
            // discontinuities between 0 & 1
            if (t < dt) // one sample width at the start of period
            {
                t /= dt;
                return t + t - t * t - 1.0f;
            }
            // t^2/2 +t +1/2
            // -1 <= t <= 0
            // discontinuities between -1 & 0
            if (t > 1.0f - dt) // one sample width at the end of period
            {
                t = (t - 1.0f) / dt;
                return t * t + t + t + 1.0f;
            }
            return 0.0f;
        }


        private float WaveTable()
        {
            float ph01 = _phase / PHASE_MAX;
            switch (settings.waveTableOscillatorType)
            {
                case SynthSettingsObjectOscillator.WaveTableOscillatorTypes.Sine8Bit:
                    return _sine8Bit[(int) (ph01 * _sine8Bit.Length)];
                case SynthSettingsObjectOscillator.WaveTableOscillatorTypes.Saw8Bit:
                    return _saw8Bit[(int) (ph01 * _saw8Bit.Length)];
                case SynthSettingsObjectOscillator.WaveTableOscillatorTypes.Square8Bit:
                    return _square8Bit[(int) (ph01 * _square8Bit.Length)];
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        float Noise()
        {
            float ph01 = _phase / PHASE_MAX;
            return _noiseWhite[(int) (ph01 * _noiseWhite.Length)];
        }

        public void UpdateSettings(SynthSettingsObjectOscillator settings)
        {
            this.settings = settings;
        }
    }
}