using LooperAPP.AudioSystem;
using Rytmos.AudioSystem;
using UnityEngine;
using UnitySynth.Runtime.AudioSystem;
using UnitySynth.Samples.Code;
using Random = UnityEngine.Random;

public class AudioSynthTester : MonoBehaviour
{
    public UnitySynthPreset preset;
    private AudioSynthPlayer _audioSynthPlayer;
    private float _lastNoteTime;
    public float playNoteInterval;
    private bool _isPlaying;
    public FaceExpression faceExpression;


    private void Start()
    {
        TryGetComponent(out _audioSynthPlayer);
        _audioSynthPlayer.Init();
        _audioSynthPlayer.SetPreset(preset);
        Metronome.Instance.SetActive(true);
        Metronome.Instance.OnTick4 += OnTick4;
    }

    private void OnTick4()
    {
        if (!_isPlaying)
        {
            _isPlaying = true;
            int note = Conductor.instance.GetScaledNote(Random.Range(20, 30));
            int expression = Random.Range(0, 5);
            faceExpression.SetExpression(expression);
            faceExpression.SetNote(note);
            int[] notes = new int[3];
            notes[0] = note;
            for (var i = 1; i < notes.Length; i++)
            {
                notes[i] = Conductor.instance.GetScaledNote(note + Random.Range(1, 5));
            }

            notes[0] = 10;
            notes[1] = 10;
            notes[2] = 10;
            
            _audioSynthPlayer.NoteOn(new NoteEvent(note));
        }
        else
        {
            faceExpression.SetExpression(-1);
            _isPlaying = false;
            _audioSynthPlayer.NoteOff(Metronome.Instance.GetNextTime16());
        }
    }
}