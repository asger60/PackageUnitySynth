// Copyright (c) 2018 Jakob Schmid
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE."

using System;
using System.Collections.Generic;
using Synth;
using UnityEngine;
using UnitySynth.Runtime.AudioSystem;
using UnitySynth.Runtime.Synth.Filter;

namespace UnitySynth.Runtime.Synth
{
    [RequireComponent(typeof(AudioSource))]
    public class UnitySynth : MonoBehaviour
    {
        [Header("Voices")] public SynthOscillator[] oscillators;


        [Header("Pitch")] public bool unison;
        public SynthControlBase[] voiceFrequencyModifiers;

        [Header("Amp")] public SynthControlBase[] amplitudeModifiers;

        [Header("Filter")] public SynthControlBase[] filterModifiers;


        // Synth state
        //private MoogFilter _filter1, _filter2;

        //private FilterBandPass _filterBandPass1, _filterBandPass2;
        //private FilterFormant _filterFormant1, _filterFormant2;
        public AudioFilterBase[] filters;

        //ADSR fenv;
        //Phaser _pulseWidthLfo;
        //Phaser fenv;

        // when note_is_on = true, wait for current_delta samples and set note_is_playing = true
        //bool note_is_playing;
        bool is_initialized = false;

        // Current MIDI evt
        //bool note_is_on;
        private List<int> _notes = new List<int>();
        int current_velocity;

        EventQueue queue;

        int sample_rate;

        public static float[] freqtab;
        float[] freqtabTest;

        private const int QueueCapacity = 320;
        private float[] lastBuffer = new float[2048];
        private readonly object bufferMutex = new object();
        private bool debugBufferEnabled = false;

        private EventQueue.QueuedEvent nextEvent;
        private bool eventIsWaiting = false;
        private UnitySynthPreset _preset;

        /// Public interface
        public bool PlayScheduled(EventQueue.EventType evtType, int note, int data, double eventTime)
        {
            //queueLock = true;
            bool result = queue.Enqueue(evtType, note, data, eventTime);
            //queueLock = false;
            return result;
        }

        public bool PlayScheduled(EventQueue.EventType evtType, List<int> notes, int data, double eventTime)
        {
            //queueLock = true;
            bool result = queue.Enqueue(evtType, notes, data, eventTime);
            //queueLock = false;
            return result;
        }

        public void ClearQueue()
        {
            queue.Clear();
        }


        public void SetPreset(UnitySynthPreset preset)
        {
            _preset = preset;
            _preset.BindToRuntime(this);
            RebuildSynth();
        }

        public void RebuildSynth()
        {
            foreach (var synthControlBase in GetComponentsInChildren<SynthControlBase>())
            {
                Destroy(synthControlBase.gameObject);
            }

            foreach (var synthOscillator in GetComponentsInChildren<SynthOscillator>())
            {
                Destroy(synthOscillator.gameObject);
            }

            oscillators = new SynthOscillator[_preset.oscillatorSettings.Length];
            voiceFrequencyModifiers = new SynthControlBase[_preset.pitchModifiers.Length];
            amplitudeModifiers = new SynthControlBase[_preset.amplitudeModifiers.Length];
            filterModifiers = new SynthControlBase[_preset.filterModifiers.Length];
            filters = new AudioFilterBase[_preset.filterSettings.Length * 2];

            for (int i = 0; i < _preset.oscillatorSettings.Length; i++)
            {
                var oscillatorSetting = _preset.oscillatorSettings[i];
                var modGO = new GameObject("Oscillator");
                modGO.transform.SetParent(transform);
                var newOSC = modGO.AddComponent<SynthOscillator>();
                newOSC.UpdateSettings(oscillatorSetting);
                oscillators[i] = newOSC;
            }

            for (int i = 0; i < _preset.filterSettings.Length; i++)
            {
                switch (_preset.filterSettings[i].filterType)
                {
                    case SynthSettingsObjectFilter.FilterTypes.LowPass:

                        var modGO = new GameObject("LowPassFilter");
                        modGO.transform.SetParent(transform);
                        var newFilter = modGO.AddComponent<FilterLowPass>();
                        newFilter.SetSettings(_preset.filterSettings[i]);
                        filters[i * 2] = newFilter;

                        var newFilter2 = modGO.AddComponent<FilterLowPass>();
                        newFilter2.SetSettings(_preset.filterSettings[i]);
                        filters[(i * 2) + 1] = newFilter2;

                        break;
                    case SynthSettingsObjectFilter.FilterTypes.BandPass:

                        var phFilterGO = new GameObject("BandPassFilter");
                        phFilterGO.transform.SetParent(transform);
                        var newHPFilter = phFilterGO.AddComponent<FilterBandPass>();
                        newHPFilter.SetSettings(_preset.filterSettings[i]);
                        filters[i * 2] = newHPFilter;

                        var newHPFilter2 = phFilterGO.AddComponent<FilterBandPass>();
                        newHPFilter2.SetSettings(_preset.filterSettings[i]);
                        filters[(i * 2) + 1] = newHPFilter2;


                        break;
                    case SynthSettingsObjectFilter.FilterTypes.Formant:

                        var formantFilterGO = new GameObject("FormantFilter");
                        formantFilterGO.transform.SetParent(transform);
                        var newFormantFilter = formantFilterGO.AddComponent<FilterFormant>();
                        newFormantFilter.SetSettings(_preset.filterSettings[i]);
                        filters[i * 2] = newFormantFilter;

                        var newFormantFilter2 = formantFilterGO.AddComponent<FilterFormant>();
                        newFormantFilter2.SetSettings(_preset.filterSettings[i]);
                        filters[(i * 2) + 1] = newFormantFilter2;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                print("create filter");
            }


            for (int i = 0; i < _preset.pitchModifiers.Length; i++)
            {
                var freqMod = _preset.pitchModifiers[i];
                switch (freqMod)
                {
                    case SynthSettingsObjectLFO lfo:
                    {
                        var modGO = new GameObject("Frequency Modifier LFO");
                        modGO.transform.SetParent(transform);
                        var newController = modGO.AddComponent<SynthControlLFO>();
                        newController.UpdateSettings(lfo);
                        voiceFrequencyModifiers[i] = newController;
                        break;
                    }
                    case SynthSettingsObjectEnvelope envelope:
                    {
                        var modGO = new GameObject("Frequency Modifier Envelope");
                        modGO.transform.SetParent(transform);
                        var newController = modGO.AddComponent<SynthControlEnvelope>();
                        newController.UpdateSettings(envelope);
                        voiceFrequencyModifiers[i] = newController;
                        break;
                    }
                }
            }

            for (int i = 0; i < _preset.amplitudeModifiers.Length; i++)
            {
                var ampModSetting = _preset.amplitudeModifiers[i];
                switch (ampModSetting)
                {
                    case SynthSettingsObjectLFO lfo:
                    {
                        var modGO = new GameObject("Amplitude Modifier LFO");
                        modGO.transform.SetParent(transform);
                        var newController = modGO.AddComponent<SynthControlLFO>();
                        newController.UpdateSettings(lfo);
                        amplitudeModifiers[i] = newController;
                        print("create lfo");
                        break;
                    }
                    case SynthSettingsObjectEnvelope envelope:
                    {
                        var modGO = new GameObject("Amplitude Modifier Envelope");
                        modGO.transform.SetParent(transform);
                        var newController = modGO.AddComponent<SynthControlEnvelope>();
                        newController.UpdateSettings(envelope);
                        amplitudeModifiers[i] = newController;
                        break;
                    }
                }
            }

            for (int i = 0; i < _preset.filterModifiers.Length; i++)
            {
                var filterModifier = _preset.filterModifiers[i];
                switch (filterModifier)
                {
                    case SynthSettingsObjectLFO lfo:
                    {
                        var modGO = new GameObject("Filter Modifier LFO");
                        modGO.transform.SetParent(transform);
                        var newController = modGO.AddComponent<SynthControlLFO>();
                        newController.UpdateSettings(lfo);
                        filterModifiers[i] = newController;
                        break;
                    }
                    case SynthSettingsObjectEnvelope envelope:
                    {
                        var modGO = new GameObject("Filter Modifier Envelope");
                        modGO.transform.SetParent(transform);
                        var newController = modGO.AddComponent<SynthControlEnvelope>();
                        newController.UpdateSettings(envelope);
                        filterModifiers[i] = newController;
                        break;
                    }
                }
            }

            is_initialized = true;
        }

        // This should only be called from OnAudioFilterRead
        private void HandleEventNow(EventQueue.QueuedEvent currentEvent)
        {
            if (currentEvent.eventType == EventQueue.EventType.Note_on)
            {
                foreach (var synthOscillator in oscillators)
                {
                    synthOscillator.ResetPhase();
                }


                _notes = currentEvent.notes;
                update_params();


                foreach (var ampModifier in amplitudeModifiers)
                {
                    ampModifier.NoteOn();
                }

                foreach (var filterModifier in filterModifiers)
                {
                    filterModifier.NoteOn();
                }

                foreach (var frequencyModifier in voiceFrequencyModifiers)
                {
                    frequencyModifier.NoteOn();
                }
            }
            else
            {
                foreach (var ampModifier in amplitudeModifiers)
                {
                    ampModifier.NoteOff();
                }

                foreach (var filterModifier in filterModifiers)
                {
                    filterModifier.NoteOff();
                }
            }
        }


        /// Debug
        public void SetDebugBufferEnabled(bool enabled)
        {
            this.debugBufferEnabled = enabled;
        }

        public float[] GetLastBuffer()
        {
            return lastBuffer;
        }

        public object GetBufferMutex()
        {
            return bufferMutex;
        }

        //public bool bufferLock = false;
        //public bool queueLock = false;

        /// Unity
        private void Start()
        {
            Init(1, AudioSettings.outputSampleRate);
        }


        //private Int64 time_smp = 0;

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (is_initialized)
            {
                if (channels == 2)
                {
                    int sampleFrames = data.Length / 2;
                    render_float32_stereo_interleaved(data, sampleFrames);

                    if (debugBufferEnabled)
                    {
                        //bufferLock = true;
                        lock (bufferMutex)
                        {
                            Array.Copy(data, lastBuffer, data.Length);
                        }
                        //bufferLock = false;
                    }
                }
            }
        }

        /// Internal
        private void Init(int queue_length, int sample_rate)
        {
            if (freqtab == null)
            {
                freqtabTest = new float[128];
                freqtab = new float[128];
                for (int i = 0; i < 128; i++)
                {
                    // 128 midi notes
                    freqtab[i] = midi2freq(i);
                    freqtabTest[i] = midi2freq(i);
                }
            }

            this.sample_rate = sample_rate;


            queue = new EventQueue(QueueCapacity);
            _notes = new List<int> { 0 };

            Reset();
        }

        private void Reset()
        {
            foreach (var synthOscillator in oscillators)
            {
                synthOscillator.ResetPhase();
            }
        }


        private void update_params()
        {
            // Set synth params
            foreach (var frequencyModifier in voiceFrequencyModifiers)
            {
                frequencyModifier.DoUpdate();
            }

            foreach (var amplitudeModifier in amplitudeModifiers)
            {
                amplitudeModifier.DoUpdate();
            }

            foreach (var filterModifier in filterModifiers)
            {
                filterModifier.DoUpdate();
            }

            float voiceFreq = 1;
            foreach (var frequencyModifier in voiceFrequencyModifiers)
            {
                voiceFreq *= frequencyModifier.Process();
            }
            //_osc1.set_freq(freq * voiceFreqMod, sample_rate);
            //_subOsc.set_freq(freq * 0.5f * voiceFreqMod, sample_rate);


            if (unison)
            {
                for (int i = 0; i < oscillators.Length; i++)
                {
                    oscillators[i].SetNote(_notes[0]);
                    oscillators[i].SetPitchMod(voiceFreq);
                }
                //_osc2.set_freq(freq * voiceFreqMod * Mathf.LerpUnclamped(1, 1.01f, voiceSpread), sample_rate);
                //_osc3.set_freq(freq * voiceFreqMod * Mathf.LerpUnclamped(1, .99f, voiceSpread), sample_rate);
                //_osc4.set_freq(freq * voiceFreqMod * Mathf.LerpUnclamped(1, 1.005f, voiceSpread), sample_rate);
            }
            else
            {
                for (int i = 0; i < oscillators.Length; i++)
                {
                    int currentNote = (int)Mathf.Repeat(i, _notes.Count);
                    //  oscillators[i].SetNote(_notes[currentNote]);
                    oscillators[i].SetNote(_notes[currentNote]);
                    oscillators[i].SetPitchMod(voiceFreq);
                }
            }

            //fenv.set_freq(1.0f / filterEnvDecay, sample_rate);
            //_pulseWidthLfo.set_freq(_preset.pwmFrequency * 2.3f, sample_rate);

            //float env01 = fenv.quad_down01();


            //_filterFormant1.SetVowel(_preset.vowel);
            //_filterFormant2.SetVowel(_preset.vowel);


            float cutOffFreq = _preset.filterSettings[0].lowPassSettings.cutoffFrequency;

            foreach (var filterModifier in filterModifiers)
            {
                cutOffFreq *= filterModifier.Process();
            }

            for (var i = 0; i < filters.Length; i++)
            {
                var audioFilterBase = filters[i];
                audioFilterBase.SetParameters(_preset.filterSettings[i / 2]);
                audioFilterBase.SetExpression(0);
            }

            //_filter1.SetCutoff(cutOffFreq); // 0 Hz cutoff is bad
            //_filter2.SetCutoff(cutOffFreq);
//
            //_filterBandPass1.SetFrequency(cutOffFreq);
            //_filterBandPass2.SetFrequency(cutOffFreq);
//
//
            //_filter1.SetOversampling(_preset.oversampling);
            //_filter2.SetOversampling(_preset.oversampling);
        }

        private void render_float32_stereo_interleaved(float[] buffer, int sample_frames)
        {
            if (!is_initialized) return;

            int smp = 0;
            int buf_idx = 0;
            //int time_smp = masterClock_smp;

            update_params();

            // Cache this for the entire buffer, we don't need to check for
            // every sample if new events have been enqueued.
            // This assumes that no other metdods call GetFrontAndDequeue.
            int queueSize = queue.GetSize();

            // Render loop
            for (; smp < sample_frames; ++smp)
            {
                // Event handling
                // This is sample accurate event handling.
                // If it's too slow, we can decide to only handle 1 event per buffer and
                // move this code outside the loop.
                while (true)
                {
                    if (eventIsWaiting == false && queueSize > 0)
                    {
                        //queueLock = true;
                        if (queue.GetFrontAndDequeue(ref nextEvent))
                        {
                            eventIsWaiting = true;
                            queueSize--;
                        }
                        //queueLock = false;
                    }

                    if (eventIsWaiting)
                    {
                        //if (nextEvent.time_smp <= time_smp)
                        if (nextEvent.eventTime <= AudioSettings.dspTime)
                        {
                            HandleEventNow(nextEvent);
                            eventIsWaiting = false;
                        }
                        else
                        {
                            // we assume that queued events are in order, so if it's not
                            // now, we stop getting events from the queue
                            break;
                        }
                    }
                    else
                    {
                        // no more events
                        break;
                    }
                }

                foreach (var frequencyModifier in voiceFrequencyModifiers)
                {
                    frequencyModifier.DoUpdate();
                }

                foreach (var amplitudeModifier in amplitudeModifiers)
                {
                    amplitudeModifier.DoUpdate();
                }

                foreach (var filterModifier in filterModifiers)
                {
                    filterModifier.DoUpdate();
                }


                // Rendering
                //if (note_is_on)

                // Render sample
                float ampMod = 1;
                foreach (var ampModifier in amplitudeModifiers)
                {
                    ampMod *= ampModifier.Process();
                }

                //ampMod /= (float)amplitudeModifiers.Length;
                ampMod /= 2f;

                float oscillatorOutput = 0;
                foreach (var synthOscillator in oscillators)
                {
                    oscillatorOutput += synthOscillator.Process();
                }

                oscillatorOutput /= oscillators.Length;


                float sample = oscillatorOutput * ampMod;

                buffer[buf_idx++] = sample;
                buffer[buf_idx++] = sample;

                // Update oscillators
                foreach (var synthOscillator in oscillators)
                {
                    synthOscillator.DoUpdate();
                }


                //float cutOffFreq = _preset.cutoffFrequency /* * fenv.Process() * (filterCutOffModifier.Process())*/;
                foreach (var filterModifier in filterModifiers)
                {
                    //cutOffFreq *= filterModifier.Process();
                }

                foreach (var audioFilterBase in filters)
                {
                    float currentMod = 1;
                    foreach (var filterModifier in filterModifiers)
                    {
                        currentMod *= filterModifier.Process();
                    }

                    audioFilterBase.HandleModifiers(currentMod);
                }
            }


            // Filter entire buffer
            for (var i = 0; i < filters.Length; i++)
            {
                var audioFilterBase = filters[i];
                audioFilterBase.SetParameters(_preset.filterSettings[i / 2]);
                audioFilterBase.process_mono_stride(buffer, sample_frames, i % 2, 2);
            }
        }


        /// Internals
        private static float midi2freq(int note)
        {
            return 440 * (Mathf.Pow(2, ((note - 69) / 12f)));
            // return 32.70319566257483f * Mathf.Pow(2.0f, note / 12.0f + octave);
        }
    }
}