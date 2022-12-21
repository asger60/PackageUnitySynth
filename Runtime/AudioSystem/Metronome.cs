using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace LooperAPP.AudioSystem
{
    public class Metronome : MonoBehaviour
    {
        public int Bpm;

        public bool Playing;
        public int sub2;
        public int sub4;

        public int sub8;
        public int Sub16;
        public int Sub32;


        private double _sub32Length;
        private double _sub16Length;
        private double _sub8Length;

        private double _sub4Length;
        private double _sub2Length;


        public double bufferTime = 0.2f;
        private double _nextTime32 = 0;
        private double _nextTime16 = 0;


        public enum TickRate
        {
            Sub2 = 20,
            Sub4 = 40,
            Sub8 = 80,
            Sub16 = 160,
            Sub32 = 320,
        }

        public Action OnTick2;
        public Action OnTick4;
        public Action OnTick8;
        public Action OnTick16;
        public Action OnTick32;
        public Action OnNextBar;

        private int _currentBar;
        public int CurrentBar => _currentBar;
        private bool _isInit;
        public bool IsInit => _isInit;


        private bool _isActive;
        public static Metronome Instance { get; private set; }
        public bool debugMode;


        private void Awake()
        {
            Instance = this;
            
        }


        private void Start()
        {
            Init();
            Playing = true;
            _currentBar = 0;
            _nextTime32 = AudioSettings.dspTime + bufferTime;
        }

        public void SetTempo(int newBpm)
        {
            Bpm = newBpm;
            Init();
        }


        public void Init()
        {
            _sub4Length = ((float)60000 / Bpm) / 1000;
            _sub2Length = _sub4Length * 2;

            _sub8Length = _sub4Length * 0.5f;
            _sub16Length = _sub8Length * 0.5f;
            _sub32Length = _sub16Length * 0.5f;

            _isInit = true;
        }


        private void Update()
        {
            if (!_isActive) return;
            if (!Playing) return;
            //if (!instrumentPlayer.IsInit) return;
            if (!(AudioSettings.dspTime + bufferTime >= _nextTime32)) return;


            if (Sub32 == 0)
            {
                _currentBar++;
                OnNextBar?.Invoke();
            }

            if (Sub32 % 2 == 0)
            {
                OnTick16?.Invoke();
                Sub16++;
                _nextTime16 = _nextTime32 + _sub16Length;
            }

            if (Sub32 % 4 == 0)
            {
                OnTick8?.Invoke();
                sub8++;
            }

            if (Sub32 % 8 == 0)
            {
                OnTick4?.Invoke();
                sub4++;
            }

            if (Sub32 % 16 == 0)
            {
                OnTick2?.Invoke();
                sub2++;
            }


            OnTick32?.Invoke();


            Sub32++;
            if (Sub32 == 32)
            {
                Sub32 = 0;
                Sub16 = 0;
                sub8 = 0;
                sub4 = 0;
                sub2 = 0;
            }

            _nextTime32 += _sub32Length;
            if (_nextTime32 <= AudioSettings.dspTime + bufferTime)
            {
                if (debugMode)
                    Debug.LogWarning("Buffertime is too big");
            }
        }


        public double GetLength(TickRate tickRate)
        {
            switch (tickRate)
            {
                case TickRate.Sub2:
                    return _sub2Length;
                case TickRate.Sub4:
                    return _sub4Length;
                case TickRate.Sub8:
                    return _sub8Length;
                case TickRate.Sub16:
                    return _sub16Length;
                case TickRate.Sub32:
                    return _sub32Length;
                default:
                    //Debug.Log("trying to fetch ");
                    return 0;
            }
        }

        public double GetDrift()
        {
            double targetTime = _nextTime16 - _sub16Length;
            return (targetTime - AudioSettings.dspTime);
        }

        public struct StepTiming
        {
            public int step;
            public float drift;

            public StepTiming(int step, float drift)
            {
                this.step = step;
                this.drift = drift;
            }
        }

        public StepTiming GetQuantizedStep()
        {
            double this16dspTime = _nextTime16 - _sub16Length;
            float this16Drift = (float)(AudioSettings.dspTime - this16dspTime);
            float next16Drift = (float)(AudioSettings.dspTime - _nextTime16);
            int step = Sub16 - 1;
            float drift = next16Drift;
            print("distance to this " + this16Drift);
            if (Mathf.Abs(this16Drift) > _sub32Length)
            {
                print("late");
                step = Sub16 - 2;
                drift = this16Drift;
            }

            step = (int)Mathf.Repeat(step, 16);

            return new StepTiming(step, drift);
            
        }

        public double GetNextTime16()
        {
            return _nextTime16;
        }


        public void SetActive(bool b)
        {
            _isActive = b;
            if (!_isActive) return;

            _nextTime32 = AudioSettings.dspTime;
            _nextTime16 = AudioSettings.dspTime;
        }

 
    }
}