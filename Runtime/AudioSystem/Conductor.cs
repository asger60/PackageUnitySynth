using LooperAPP.AudioSystem;
using MusicGame;
using MusicGame.MusicSystem;
using UnityEngine;

namespace Rytmos.AudioSystem
{
    public class Conductor : MonoBehaviour
    {
        private int _rootNote;
        private ScaleObject _scale;
    
        private static Conductor _instance;
        public static Conductor instance => _instance;
    
        private PatternObject _currentPattern;
        private int _currentPatternStep;
        public PatternObject PatternObject;
        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            Metronome.Instance.OnNextBar += OnNextBar;
            _currentPattern = PatternObject;
            _scale = _currentPattern.patternSteps[0].scale;
            _rootNote = _currentPattern.patternSteps[0].rootNote;
        }


        private void OnNextBar()
        {
            _rootNote = _currentPattern.patternSteps[_currentPatternStep].rootNote;
            _scale = _currentPattern.patternSteps[_currentPatternStep].scale;
            _currentPatternStep++;
            _currentPatternStep = (int)Mathf.Repeat(_currentPatternStep, _currentPattern.patternSteps.Length);
        }


        public int GetScaledNote(int noteStep)
        {
            if (_scale == null) return 0;
            if (_scale.notes == null || _scale.notes.Length == 0) return 0;
            int octave = (noteStep / _scale.notes.Length) * 12;
            return _scale.notes[(int)Mathf.Repeat( noteStep, _scale.notes.Length)] + octave + _rootNote;
        }
    }
}
