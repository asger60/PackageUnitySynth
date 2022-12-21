namespace UnitySynth.Runtime.Synth.Filter
{
    public abstract class AudioFilterBase
    {
        public abstract void SetExpression(float data);


        
        public abstract void SetParameters(SynthSettingsObjectFilter settingsObjectFilter);

        public abstract void HandleModifiers(float mod1);
        

        public virtual void process_mono_stride(float[] samples, int sample_count, int offset, int stride)
        {
        }
    }
}