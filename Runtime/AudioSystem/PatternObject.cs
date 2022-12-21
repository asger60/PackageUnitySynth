using UnityEngine;

namespace MusicGame.MusicSystem
{
    public class PatternObject : ScriptableObject
    {
        [System.Serializable]
        public class PatternStep
        {
            public int rootNote;
            public ScaleObject scale;
        }

        public PatternStep[] patternSteps;

    }
}
