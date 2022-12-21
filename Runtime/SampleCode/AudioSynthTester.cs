using LooperAPP;
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
            _audioSynthPlayer.NoteOn(new NoteEvent(note, expression));
        }
        else
        {
            faceExpression.SetExpression(-1);
            _isPlaying = false;
            _audioSynthPlayer.NoteOff(Metronome.Instance.GetNextTime16());
        }
    }
}