using System.Collections;
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
        Metronome.Instance.OnTick2 += OnTick4;
    }

    private void OnTick4()
    {
        if (!_isPlaying)
        {
            _isPlaying = true;
            int note = Conductor.instance.GetScaledNote(Random.Range(20, 26));
            int expression = Random.Range(0, 5);
            faceExpression.SetExpression(expression);
            faceExpression.SetNote(note);
            int[] notes = new int[4];
            notes[0] = note;
            for (var i = 1; i < notes.Length; i++)
            {
                notes[i] = note + Conductor.instance.GetScaledNote(Random.Range(2, 6));
            }

            var n = new NoteEvent(notes)
            {
                expression1 = Random.Range(0, 1f)
            };

            _audioSynthPlayer.NoteOn(n);
            StartCoroutine(NoteOff());
        }
        else
        {
            //faceExpression.SetExpression(-1);
            //_isPlaying = false;
            //_audioSynthPlayer.NoteOff(Metronome.Instance.GetNextTime16());
        }
    }

    IEnumerator NoteOff()
    {
        yield return new WaitForSeconds(2f);
        faceExpression.SetExpression(-1);
        _isPlaying = false;
        _audioSynthPlayer.NoteOff(Metronome.Instance.GetNextTime16());
    }
}