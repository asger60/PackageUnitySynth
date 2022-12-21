

namespace Synth.Filter
{
    public abstract class AudioFilterBase
    {
        public abstract void SetExpression(float data);
        public virtual void process_mono_stride(float[] samples, int sample_count, int offset, int stride)
        {
        }
    }
}
