using UnityEngine;
using UnitySynth.Runtime.Synth.Filter;

namespace UnitySynth.Runtime.Synth
{
    public class FilterBandPass : AudioFilterBase
    {
        public FilterBandPass(float sampleRate)
        {
            _sampleRate = sampleRate;
            _q = 5;
        }

        readonly float _sampleRate = 48000; // Sample rate

        // // DSP variables
        private float _vF, _vD, _vZ1, _vZ2, _vZ3;
        private float _filterFrequency;
        private float _q; // 1-10

        public void SetFrequency(float freq)
        {
            _filterFrequency = freq;
        }

        public void SetQ(float q)
        {
            _q = q;
        }


        public override void SetExpression(float data)
        {
        }

        public override void SetParameters(SynthSettingsObjectFilter settingsObjectFilter)
        {
            _filterFrequency = settingsObjectFilter.bandPassSettings.frequency;
            _q = settingsObjectFilter.bandPassSettings.bandWidth;
        }

        public override void HandleModifiers(float mod1)
        {
        }


        public override void process_mono_stride(float[] samples, int sample_count, int offset, int stride)
        {
            var f = 2 / 1.85f * Mathf.Sin(Mathf.PI * _filterFrequency / _sampleRate);
            _vD = 1 / _q;
            _vF = (1.85f - 0.75f * _vD * f) * f;

            int idx = offset;
            for (int i = 0; i < sample_count; ++i)
            {
                var si = samples[idx];

                var _vZ1 = 0.5f * si;
                var _vZ3 = this._vZ2 * _vF + this._vZ3;
                var _vZ2 = (_vZ1 + this._vZ1 - _vZ3 - this._vZ2 * _vD) * _vF + this._vZ2;
                samples[idx] = _vZ2;
                this._vZ1 = _vZ1;
                this._vZ2 = _vZ2;
                this._vZ3 = _vZ3;

                idx += stride;
            }
        }
    }
}