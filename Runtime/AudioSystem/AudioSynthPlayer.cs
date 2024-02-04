using Rytmos.AudioSystem;
using Synth;
using UnitySynth.Runtime.AudioSystem;
using UnitySynth.Runtime.Synth;

namespace LooperAPP.AudioSystem
{
    public class AudioSynthPlayer : AudioPlayerBase
    {
        private UnitySynth.Runtime.Synth.UnitySynth _synth;
        private int _currentNote;
        private UnitySynthPreset _settings;
        public UnitySynthPreset Settings => _settings;

        public override void Init()
        {
            base.Init();
            TryGetComponent(out _synth);
        }

        public void NoteOn(NoteEvent noteEvent)
        {
            _currentNote = noteEvent.note;
            _synth.PlayScheduled(EventQueue.EventType.Note_on, noteEvent.note, noteEvent.data,
                noteEvent.scheduledPlaytime);
            _isArmed = false;
            IsReady = false;
        }

        public  void NoteOff(double stopTime)
        {
            _synth.PlayScheduled(EventQueue.EventType.Note_off, _currentNote, 0, stopTime);
            _isArmed = true;
            IsReady = true;
        }

        public void SetPreset(UnitySynthPreset preset)
        {
            _synth.SetPreset(preset);
            _settings = preset;
        }
    }
}