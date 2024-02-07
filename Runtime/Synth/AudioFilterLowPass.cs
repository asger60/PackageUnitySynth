﻿// Copyright (c) 2018 Jakob Schmid
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE."

// Huovilainen moog filter:
//
//                      I_ctl          x(n) - 4 r y_d(n-1)
//  y_a(n) = y_a(n-1) + ----- ( tanh ( ------------------- ) - W_a(n-1) )
//                       C Fs                  2V_t
//
//                      I_ctl
//  y_b(n) = y_b(n-1) + ----- ( W_a(n) - W_b(n-1) )
//                       C Fs
//
//                      I_ctl
//  y_c(n) = y_c(n-1) + ----- ( W_b(n) - W_c(n-1) )
//                       C Fs
//
//                      I_ctl                  y_d(n-1)
//  y_d(n) = y_d(n-1) + ----- ( W_c(n) - tanh( -------- ) )
//                       C Fs                    2V_t
//
//
//  where           x(n) : input
//        y_{a,b,c,d}(n) : outputs of individual filter stages
//                     r : resonance amount in ]0;1]
//                 I_ctl : control current
//                     C : capacitance ?
//                   V_t : transistor thermal voltage (constant) ?
//                    Fs : sample rate
//
//                               y_{a,b,c}(n)
//          W_{a,b,c}(n) = tanh( ------------ )
//                                   2Vt
//
//  also
//
//                   tanh x = -i tan( ix )
//
// - see Huovilainen's paper:
//  'Non-linear Digital Implementation of the Moog Ladder Filter' (2004)
//
// Performance notes from the paper:
//
//   It can be seen that each stage uses as input the tanh of the output
//   of the previous stage. This is also used by the previous stage
//   during the next sample. The calculation result can be stored and
//   thus only five tanh calculations per sample are required. These can
//   be implemented efficiently with table lookups or polynomial 
//   approximation.
//
// Algorithm:
//
//  x      = input sample
//  reso   = resonance
//  cutoff = cutoff frequency (I_ctl)
//  Fs     = sample rate
//  v      = 1 / (2 * V_t) = 0.5 * V_t
//
//  s = cutoff / C / Fs
//
//  y_a += s * ( tanh( x - 4 * reso * y_d * v ) - w_a
//  w_a  = tanh( y_a * v ); y_b += s * ( w_a - w_b )
//  w_b  = tanh( y_b * v ); y_c += s * ( w_b - w_c )
//  w_c  = tanh( y_c * v ); y_d += s * ( w_c - tanh( y_d * v )
//
//  output = y_d
//
// Quality notes:
//
//  Huovilainen suggests using oversampling to avoid artifacts.
//  He also suggests a half sample delay as phase compensation, which
//  is not implemented yet.

using UnitySynth.Runtime.Synth.Filter;
using static System.Math;

namespace UnitySynth.Runtime.Synth
{
    public class AudioFilterLowPass : AudioFilterBase
    {
        /// Static config
        const float C = 1.0f; // ????

        const float V_t = 1.22070313f; // From Diakopoulos

        /// Config
        float reso, Fs;

        int oversampling = 1; // 1 means don't oversample

        /// State
        double y_a, y_b, y_c, y_d;

        double w_a, w_b, w_c;

        /// Cache
        double s, v;

        float cutoff;

        public AudioFilterLowPass(float sampleRate)
        {
            Fs = sampleRate;
            v = V_t * 0.5f; // 1/2V_t
        }


        public override void SetExpression(float data)
        {
        }

        public override void SetParameters(SynthSettingsObjectFilter settingsObjectFilter)
        {
            settings = settingsObjectFilter;
            reso = settings.lowPassSettings.resonance;
            SetCutoff(settings.lowPassSettings.cutoffFrequency);
            SetOversampling(settings.lowPassSettings.oversampling);
        }

        private float _cutoffMod = 1;

        public override void HandleModifiers(float mod1)
        {
            _cutoffMod = mod1;
        }

        public override void SetSettings(SynthSettingsObjectFilter newSettings)
        {
            Fs = 44100;
            v = V_t * 0.5f; // 1/2V_t
            settings = newSettings;
        }

        public override void process_mono_stride(float[] samples, int sampleCount, int offset, int stride)
        {
            int idx = offset;
            for (int i = 0; i < sampleCount; ++i)
            {
                float x = samples[idx]; // x = input sample
                for (int j = 0; j < oversampling; ++j)
                {
                    y_a += s * (Tanh(x - 4 * reso * y_d * v) - w_a);
                    w_a = Tanh(y_a * v);
                    y_b += s * (w_a - w_b);
                    w_b = Tanh(y_b * v);
                    y_c += s * (w_b - w_c);
                    w_c = Tanh(y_c * v);
                    y_d += s * (w_c - Tanh(y_d * v));
                }

                samples[idx] = (float)y_d; // y_d = output sample
                idx += stride;
            }
        }


        public override float Process(float sample)
        {
            float x = sample; // x = input sample
            for (int j = 0; j < oversampling; ++j)
            {
                y_a += s * (Tanh(x - 4 * reso * y_d * v) - w_a);
                w_a = Tanh(y_a * v);
                y_b += s * (w_a - w_b);
                w_b = Tanh(y_b * v);
                y_c += s * (w_b - w_c);
                w_c = Tanh(y_c * v);
                y_d += s * (w_c - Tanh(y_d * v));
            }

            return (float)y_d; // y_d = output sample
            //idx += stride;
        }


        private void SetCutoff(float c)
        {
            cutoff = c;
            s = c / C / Fs / oversampling * 6.28318530717959f * _cutoffMod;
        }

        public void SetOversampling(int iterationCount)
        {
            oversampling = iterationCount;
            if (oversampling < 1)
                oversampling = 1;
            SetCutoff(cutoff);
        }
    }
}