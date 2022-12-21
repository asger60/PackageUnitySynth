using UnityEngine;
using UnitySynth.Runtime.Synth.Filter;

namespace UnitySynth.Runtime.Synth
{
    public class FilterFormant : AudioFilterBase
    {
        private readonly FilterBandPass _filterBandPass1;
        private readonly FilterBandPass _filterBandPass2;
        private readonly FilterBandPass _filterBandPass3;

        struct FormantBand
        {
            public float frequency;
            public float q;
            public float gain;


            public FormantBand(float frequency, float bw, float gainDB)
            {
                this.frequency = frequency;
                float halfWidth = bw / 2f;
                this.q = frequency / ((frequency + halfWidth) - (frequency - halfWidth));
                gain = DecibelToLinear(gainDB);
            }
        }

        static float DecibelToLinear(float dB)
        {
            float linear = Mathf.Pow(10.0f, dB / 20.0f);
            return linear;
        }

        readonly struct FormantVowel
        {
            private readonly string name;
            private readonly FormantBand[] _formantBands;

            public FormantBand GetBand(int i)
            {
                return _formantBands[i];
            }

            public FormantVowel(string name, FormantBand[] bands)
            {
                this.name = name;
                _formantBands = bands;
            }
        }

        // vowels from here: http://www.csounds.com/manual/html/MiscFormants.html
        private readonly FormantVowel[] _vowels =
        {
            new(
                "EEE",
                new[]
                {
                    new FormantBand(400, 60, 1),
                    new FormantBand(1600, 80, -24f),
                    new FormantBand(2700, 120, -30f)
                }),
            new(
                "III",
                new[]
                {
                    new FormantBand(350, 50, 0),
                    new FormantBand(1700, 100, -20f),
                    new FormantBand(2700, 120, -30f)
                }),
            new(
                "OOO",
                new[]
                {
                    new FormantBand(450, 80f, 0),
                    new FormantBand(800, 80f, -9f),
                    new FormantBand(2830, 100f, -16f)
                }),
            new(
                "AAA",
                new[]
                {
                    new FormantBand(600, 60, 0),
                    new FormantBand(1040, 70, -7f),
                    new FormantBand(2250, 110, -9f)
                }),
            new(
                "EEE",
                new[]
                {
                    new FormantBand(400, 40, 0),
                    new FormantBand(1620, 80, -12f),
                    new FormantBand(2400, 100, -9f)
                }),
            new(
                "III",
                new[]
                {
                    new FormantBand(250, 60, 0),
                    new FormantBand(1750, 80, -30f),
                    new FormantBand(2600, 100, -16f)
                }),
            new(
                "OOO",
                new[]
                {
                    new FormantBand(400, 40, 0),
                    new FormantBand(750, 80, -11f),
                    new FormantBand(2400, 100, -21f)
                }),
            new(
                "UUU",
                new[]
                {
                    new FormantBand(350, 40, 0),
                    new FormantBand(600, 80, -20f),
                    new FormantBand(2400, 100, -32f)
                }),
        };

        private FormantVowel _currentVowel;

        public FilterFormant(float sampleRate)
        {
            _sampleRate = sampleRate;
            _filterBandPass1 = new FilterBandPass(sampleRate);
            _filterBandPass2 = new FilterBandPass(sampleRate);
            _filterBandPass3 = new FilterBandPass(sampleRate);
            _currentVowel = _vowels[3];

            _filterBandPass1.SetFrequency(_currentVowel.GetBand(0).frequency);
            _filterBandPass1.SetQ(_currentVowel.GetBand(0).q);
            _filterBandPass2.SetFrequency(_currentVowel.GetBand(1).frequency);
            _filterBandPass2.SetQ(_currentVowel.GetBand(1).q);
            _filterBandPass3.SetFrequency(_currentVowel.GetBand(2).frequency);
            _filterBandPass3.SetQ(_currentVowel.GetBand(2).q);
        }

        private void SetVowel(int index)
        {
            _currentVowel = _vowels[index];
            _filterBandPass1.SetFrequency(_currentVowel.GetBand(0).frequency);
            _filterBandPass1.SetQ(_currentVowel.GetBand(0).q);
            _filterBandPass2.SetFrequency(_currentVowel.GetBand(1).frequency);
            _filterBandPass2.SetQ(_currentVowel.GetBand(1).q);
            _filterBandPass3.SetFrequency(_currentVowel.GetBand(2).frequency);
            _filterBandPass3.SetQ(_currentVowel.GetBand(2).q);
        }

        float _sampleRate = 48000; // Sample rate

        public override void SetExpression(float data)
        {
            
        }


        public override void SetParameters(SynthSettingsObjectFilter settingsObjectFilter)
        {
            SetVowel(settingsObjectFilter.formantSettings.vowel);
        }

        public override void HandleModifiers(float mod1)
        {
        }

        public override void process_mono_stride(float[] samples, int sample_count, int offset, int stride)
        {
            float[] mix1 = new float[samples.Length];
            var mix2 = new float[samples.Length];
            var mix3 = new float[samples.Length];
            samples.CopyTo(mix1, 0);
            samples.CopyTo(mix2, 0);
            samples.CopyTo(mix3, 0);


            _filterBandPass1.process_mono_stride(mix1, sample_count, offset, stride);
            _filterBandPass2.process_mono_stride(mix2, sample_count, offset, stride);
            _filterBandPass3.process_mono_stride(mix3, sample_count, offset, stride);

            int idx = offset;
            for (int i = 0; i < sample_count; ++i)
            {
                samples[idx] =
                    ((mix1[idx] * _currentVowel.GetBand(0).gain) +
                     (mix2[idx] * _currentVowel.GetBand(1).gain) +
                     (mix3[idx] * _currentVowel.GetBand(2).gain))
                    / 3f;
                idx += stride;
            }
        }
    }
}