using LooperAPP.AudioSystem;
using UnityEngine;

namespace Rytmos.AudioSystem
{
    public class AudioPlayerBase : MonoBehaviour
    {

        public bool IsReady { get; protected set; }
        protected bool _isArmed;



        public virtual void Init()
        {
            IsReady = true;
        }

  

        


        private void Update()
        {
            if (_isArmed)
            {
                _isArmed = false;
                IsReady = true;
            }
            
        }

       
    }
}