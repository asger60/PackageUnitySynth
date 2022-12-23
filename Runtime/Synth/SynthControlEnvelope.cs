using Synth;
using UnityEngine;

namespace UnitySynth.Runtime.Synth
{
    public class SynthControlEnvelope : SynthControlBase
    {
        private enum EnvState
        {
            env_idle = 0,
            env_attack,
            env_decay,
            env_sustain,
            env_release
        };

        private EnvState state;
        private float output;
        private float attackRate;
        private float decayRate;
        private float releaseRate;
        private float attackCoef;
        private float decayCoef;
        private float releaseCoef;
        private float sustainLevel;
        private float targetRatioA;
        private float targetRatioDR;
        private float attackBase;
        private float decayBase;
        private float releaseBase;

        //Range(0, 1)] public float attack;
        //Range(0, 1)] public float decay;
        //Range(0, 1)] public float sustain;
        //Range(0, 10)] public float release;

        private int _sampleRate = 48000;
        public SynthSettingsObjectEnvelope settings;

        public override void NoteOn()
        {
            if (settings == null) return;
            setAttackRate(settings.attack * _sampleRate);
            setDecayRate(settings.decay * _sampleRate);
            setSustainLevel(settings.sustain);
            setReleaseRate(settings.release * _sampleRate);
            setTargetRatioA(0.3f);
            setTargetRatioDR(0.0001f);
            state = EnvState.env_attack;
        }

        public override void NoteOff()
        {
            state = EnvState.env_release;
        }

        private void setAttackRate(float rate)
        {
            attackRate = rate;
            attackCoef = CalcCoef(rate, targetRatioA);
            attackBase = (1.0f + targetRatioA) * (1.0f - attackCoef);
        }

        private void setDecayRate(float rate)
        {
            decayRate = rate;
            decayCoef = CalcCoef(rate, targetRatioDR);
            decayBase = (sustainLevel - targetRatioDR) * (1.0f - decayCoef);
        }

        private void setReleaseRate(float rate)
        {
            releaseRate = rate;
            releaseCoef = CalcCoef(rate, targetRatioDR);
            releaseBase = -targetRatioDR * (1.0f - releaseCoef);
        }

        private void setSustainLevel(float level)
        {
            sustainLevel = level;
            decayBase = (sustainLevel - targetRatioDR) * (1.0f - decayCoef);
        }

        private void setTargetRatioA(float targetRatio)
        {
            if (targetRatio < 0.000000001f)
                targetRatio = 0.000000001f; // -180 dB
            targetRatioA = targetRatio;
            attackCoef = CalcCoef(attackRate, targetRatioA);
            attackBase = (1.0f + targetRatioA) * (1.0f - attackCoef);
        }

        private void setTargetRatioDR(float targetRatio)
        {
            if (targetRatio < 0.000000001f)
                targetRatio = 0.000000001f; // -180 dB
            targetRatioDR = targetRatio;
            decayCoef = CalcCoef(decayRate, targetRatioDR);
            releaseCoef = CalcCoef(releaseRate, targetRatioDR);
            decayBase = (sustainLevel - targetRatioDR) * (1.0f - decayCoef);
            releaseBase = -targetRatioDR * (1.0f - releaseCoef);
        }

        public void reset()
        {
            state = EnvState.env_idle;
            output = 0.0f;
        }

        private float CalcCoef(float rate, float targetRatio)
        {
            return (rate <= 0) ? 0 : Mathf.Exp(-Mathf.Log((1.0f + targetRatio) / targetRatio) / rate);
        }


        public override float Process(bool unipolar = false)
        {
            switch (state)
            {
                case EnvState.env_idle:
                    break;
                case EnvState.env_attack:
                    output = attackBase + output * attackCoef;
                    if (output >= 1.0f)
                    {
                        output = 1.0f;
                        state = EnvState.env_decay;
                    }

                    break;
                case EnvState.env_decay:
                    output = decayBase + output * decayCoef;
                    if (output <= sustainLevel)
                    {
                        output = sustainLevel;
                        state = EnvState.env_sustain;
                    }

                    break;
                case EnvState.env_sustain:
                    break;
                case EnvState.env_release:
                    output = releaseBase + output * releaseCoef;
                    if (output <= 0.0f)
                    {
                        output = 0.0f;
                        state = EnvState.env_idle;
                    }

                    break;
            }

            return output;
        }

        public void UpdateSettings(SynthSettingsObjectEnvelope settings)
        {
            this.settings = settings;
        }
    }
}