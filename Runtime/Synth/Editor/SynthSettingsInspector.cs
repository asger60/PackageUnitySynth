using LooperAPP.AudioSystem;
using LooperAPP.AudioSystem.Editor;
using Rytmos.AudioSystem;
using UnityEditor;
using UnityEngine;

namespace Synth.Editor
{
    [CustomEditor(typeof(InstrumentObjectSynth))]
    public class SynthSettingsInspector : UnityEditor.Editor
    {
        private InstrumentObjectSynth _settingsObject;
        private bool _showFilterMods;
        private bool _showAmpMods;
        private bool _showPitchMods;
        private bool _showOscillators;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (_settingsObject == null) _settingsObject = (InstrumentObjectSynth) this.target;
            if (_settingsObject == null) return;

            GUILayout.Space(10);
            _showOscillators = EditorGUILayout.Foldout(_showOscillators, "Oscillators");
            if (_showOscillators)
            {
                foreach (var osc in _settingsObject.oscillatorSettings)
                {
                    SynthOSCInspector.Draw(this, osc);
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("add oscillator"))
                {
                    CreateOscillator("oscillatorSettings", "Oscillator");
                }

                

                GUILayout.EndHorizontal();
            }
            
            
            GUILayout.Space(10);
            _showPitchMods = EditorGUILayout.Foldout(_showPitchMods, "Pitch Modifiers");
            if (_showPitchMods)
            {
                foreach (var ampMod in _settingsObject.pitchModifiers)
                {
                    switch (ampMod)
                    {
                        case SynthSettingsObjectLFO lfoMod:
                            SynthLFOInspector.Draw(this, lfoMod);
                            break;
                        case SynthSettingsObjectEnvelope envMod:
                            SynthEnvelopeInspector.Draw(this, envMod);
                            break;
                    }
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("add envelope modifier"))
                {
                    CreateEnvelopeMod("pitchModifiers", "Pitch Envelope");
                }

                if (GUILayout.Button("add LFO modifier"))
                {
                    CreateLFOMod("pitchModifiers", "Pitch LFO");
                }

                GUILayout.EndHorizontal();
            }
            
            
            GUILayout.Space(10);
            _showAmpMods = EditorGUILayout.Foldout(_showAmpMods, "Amplitude Modifiers");
            if (_showAmpMods)
            {
                foreach (var ampMod in _settingsObject.amplitudeModifiers)
                {
                    switch (ampMod)
                    {
                        case SynthSettingsObjectLFO lfoMod:
                            SynthLFOInspector.Draw(this, lfoMod);
                            break;
                        case SynthSettingsObjectEnvelope envMod:
                            SynthEnvelopeInspector.Draw(this, envMod);
                            break;
                    }
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("add envelope modifier"))
                {
                    CreateEnvelopeMod("amplitudeModifiers", "Amplitude Envelope");
                }

                if (GUILayout.Button("add LFO modifier"))
                {
                    CreateLFOMod("amplitudeModifiers", "Amplitude LFO");
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            _showFilterMods = EditorGUILayout.Foldout(_showFilterMods, "Filter Modifiers");
            if (_showFilterMods)
            {
                EditorGUI.indentLevel = 1;
                foreach (var filterMod in _settingsObject.filterModifiers)
                {
                    switch (filterMod)
                    {
                        case SynthSettingsObjectLFO lfoMod:
                            SynthLFOInspector.Draw(this, lfoMod);
                            break;
                        case SynthSettingsObjectEnvelope envMod:
                            SynthEnvelopeInspector.Draw(this, envMod);
                            break;
                    }
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("add envelope modifier"))
                {
                    CreateEnvelopeMod("filterModifiers", "Filter Envelope");
                }

                if (GUILayout.Button("add LFO modifier"))
                {
                    CreateLFOMod("filterModifiers", "Filter LFO");
                }

                GUILayout.EndHorizontal();
                EditorGUI.indentLevel = 0;
            }

            void CreateEnvelopeMod(string propertyName, string settingsName)
            {
                SerializedProperty filterList = serializedObject.FindProperty(propertyName);
                var newEnvelope = _settingsObject.AddElement<SynthSettingsObjectEnvelope>(filterList, settingsName);
                newEnvelope.attack = 1;
                newEnvelope.decay = 1;
                newEnvelope.sustain = 0.5f;
                newEnvelope.release = 1;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_settingsObject);
                _settingsObject.ReBuildSynth();
            }
            
            void CreateLFOMod(string propertyName, string settingsName)
            {
                SerializedProperty filterList = serializedObject.FindProperty(propertyName);
                var newEnvelope = _settingsObject.AddElement<SynthSettingsObjectLFO>(filterList, settingsName);
                newEnvelope.amp = 1;
                newEnvelope.frequency = 10;
                newEnvelope.fadeInDuration = 0.01f;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_settingsObject);
                _settingsObject.ReBuildSynth();
            }

            void CreateOscillator(string propertyName, string settingsName)
            {
                SerializedProperty filterList = serializedObject.FindProperty(propertyName);
                var newOSC = _settingsObject.AddElement<SynthSettingsObjectOscillator>(filterList, settingsName);
                
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_settingsObject);
                _settingsObject.ReBuildSynth();
            }
        }

        public void DeleteElement(SynthSettingsObjectBase synthSettings)
        {
            SerializedProperty filterList = serializedObject.FindProperty("filterModifiers");
            synthSettings.RemoveElement<SynthSettingsObjectEnvelope>(filterList);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_settingsObject);
        }
    }
}