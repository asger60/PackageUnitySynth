using LooperAPP.AudioSystem;

namespace UnitySynth.Runtime.AudioSystem
{
    public class NoteEvent
    {
        private enum EventTypes
        {
            NoteOn,
            NoteDown,
            NoteOff
        }

        public int note;
        public int[] notes;
        public int data;
        public double scheduledPlaytime;
        private EventTypes _eventType;
        
        public NoteEvent(int note)
        {
            scheduledPlaytime = Metronome.Instance.GetNextTime16();
            _eventType = EventTypes.NoteOn;
            this.note = note;
        }
        
        public NoteEvent(int note, int data)
        {
            this.data = data;
            scheduledPlaytime = Metronome.Instance.GetNextTime16();
            _eventType = EventTypes.NoteOn;
            this.note = note;
        }
        
        
    }
}