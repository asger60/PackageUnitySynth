using LooperAPP.AudioSystem;

namespace UnitySynth.Runtime.AudioSystem
{
    public class NoteEvent
    {
        public enum EventTypes
        {
            NoteOn,
            NoteDown,
            NoteOff
        }

        public int[] notes;
        public int data;
        public double scheduledPlaytime;
        public EventTypes _eventType;

        public NoteEvent(int note)
        {
            scheduledPlaytime = Metronome.Instance.GetNextTime16();
            _eventType = EventTypes.NoteOn;
            notes = new int[1];
            notes[0] = note;
        }
        
        public NoteEvent(int[] notes)
        {
            scheduledPlaytime = Metronome.Instance.GetNextTime16();
            _eventType = EventTypes.NoteOn;
            this.notes = notes;
        }

        public NoteEvent(int note, int data)
        {
            this.data = data;
            scheduledPlaytime = Metronome.Instance.GetNextTime16();
            _eventType = EventTypes.NoteOn;
            notes = new int[1];
            notes[0] = note;
        }

        public NoteEvent(int note, EventTypes noteState)
        {
            this.data = data;
            scheduledPlaytime = Metronome.Instance.GetNextTime16();
            _eventType = noteState;
            notes = new int[1];
            notes[0] = note;
        }
    }
}