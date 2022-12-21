using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Synth
{
    public class SynthControlBase : MonoBehaviour
    {
    

        
        public virtual void DoUpdate()
        {
      
        }
        
        public virtual void NoteOn()
        {
            
        }
        
        public virtual void NoteOff()
        {
            
        }

        public virtual float Process(bool unipolar = false)
        {
            return 0;
        }
    }
}
