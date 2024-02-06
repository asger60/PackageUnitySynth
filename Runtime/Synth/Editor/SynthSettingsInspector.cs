using Synth;
using Synth.Editor;
using UnityEditor;
using UnityEngine;
using UnitySynth.Runtime.AudioSystem;
using UnitySynth.Runtime.AudioSystem.Editor;

namespace UnitySynth.Runtime.Synth.Editor
{
    [CustomEditor(typeof(UnitySynthPreset))]
    public class SynthSettingsInspector : UnityEditor.Editor
    {
        private UnitySynthPreset _settingsObject;
        private bool _showFilterMods => EditorPrefs.GetBool("Filter Modifiers");
        private bool _showAmpMods => EditorPrefs.GetBool("Amplitude Modifiers");
        private bool _showPitchMods => EditorPrefs.GetBool("Pitch Modifiers");
        private bool _showFilters => EditorPrefs.GetBool("Filters");
        private bool _showDevStuff => EditorPrefs.GetBool("Show Dev Stuff");

        public override void OnInspectorGUI()
        {
            EditorPrefs.SetBool("Show Dev Stuff", EditorGUILayout.Foldout(_showDevStuff, "Dev stuff"));
            if (_showDevStuff)
            {
                DrawDefaultInspector();
            }

            if (_settingsObject == null) _settingsObject = (UnitySynthPreset)this.target;
            if (_settingsObject == null) return;
            if (!_settingsObject.isInit)
            {
                if (GUILayout.Button("Init"))
                {
                    InitPreset();
                }

                return;
            }

            GUILayout.Space(10);
            _settingsObject.showOscillator = EditorGUILayout.Foldout(_settingsObject.showOscillator, "Oscillators");
            if (_settingsObject.showOscillator)
            {
                if (_settingsObject.oscillatorSettings.Length > 0)
                {
                    foreach (var osc in _settingsObject.oscillatorSettings)
                    {
                        SynthOSCInspector.Draw(this, osc);
                    }
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("add oscillator"))
                {
                    CreateOscillator("oscillatorSettings", "Oscillator");
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            EditorPrefs.SetBool("Filters", EditorGUILayout.Foldout(_showFilters, "Filters"));
            if (_showFilters)
            {
                foreach (var osc in _settingsObject.filterSettings)
                {
                    SynthFilterInspector.Draw(this, osc);
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("add filter"))
                {
                    CreateFilter("filterSettings", "Filter");
                }

                GUILayout.EndHorizontal();
            }


            GUILayout.Space(10);
            EditorPrefs.SetBool("Pitch Modifiers", EditorGUILayout.Foldout(_showPitchMods, "Pitch Modifiers"));
            if (_showPitchMods)
            {
                foreach (var ampMod in _settingsObject.pitchModifiers)
                {
                    switch (ampMod)
                    {
                        case SynthSettingsObjectLFO lfoMod:
                            SynthLfoInspector.Draw(this, lfoMod, "pitchModifiers");
                            break;
                        case SynthSettingsObjectEnvelope envMod:
                            SynthEnvelopeInspector.Draw(this, envMod, "pitchModifiers");
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
            EditorPrefs.SetBool("Amplitude Modifiers", EditorGUILayout.Foldout(_showAmpMods, "Amplitude Modifiers"));
            if (_showAmpMods)
            {
                foreach (var ampMod in _settingsObject.amplitudeModifiers)
                {
                    switch (ampMod)
                    {
                        case SynthSettingsObjectLFO lfoMod:
                            SynthLfoInspector.Draw(this, lfoMod, "amplitudeModifiers");
                            break;
                        case SynthSettingsObjectEnvelope envMod:
                            SynthEnvelopeInspector.Draw(this, envMod, "amplitudeModifiers");
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
            EditorPrefs.SetBool("Filter Modifiers", EditorGUILayout.Foldout(_showFilterMods, "Filter Modifiers"));
            if (_showFilterMods)
            {
                EditorGUI.indentLevel = 1;
                foreach (var filterMod in _settingsObject.filterModifiers)
                {
                    switch (filterMod)
                    {
                        case SynthSettingsObjectLFO lfoMod:
                            SynthLfoInspector.Draw(this, lfoMod, "filterModifiers");
                            break;
                        case SynthSettingsObjectEnvelope envMod:
                            SynthEnvelopeInspector.Draw(this, envMod, "filterModifiers");
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
        }

        void CreateEnvelopeMod(string propertyName, string settingsName)
        {
            SerializedProperty filterList = serializedObject.FindProperty(propertyName);
            var newEnvelope = _settingsObject.AddElement<SynthSettingsObjectEnvelope>(filterList, settingsName);
            newEnvelope.attack = 0.1f;
            newEnvelope.decay = 0.5f;
            newEnvelope.sustain = 0.5f;
            newEnvelope.release = 0.5f;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_settingsObject);
            _settingsObject.RebuildSynth();
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
            _settingsObject.RebuildSynth();
        }

        void CreateOscillator(string propertyName, string settingsName)
        {
            SerializedProperty filterList = serializedObject.FindProperty(propertyName);
            var newOSC = _settingsObject.AddElement<SynthSettingsObjectOscillator>(filterList, settingsName);
            newOSC.Init();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_settingsObject);
            _settingsObject.RebuildSynth();
        }

        void CreateFilter(string propertyName, string settingsName)
        {
            SerializedProperty filterList = serializedObject.FindProperty(propertyName);
            var newFilter = _settingsObject.AddElement<SynthSettingsObjectFilter>(filterList, settingsName);
            newFilter.Init();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_settingsObject);
            _settingsObject.RebuildSynth();
        }

        public void RebuildSynth()
        {
            _settingsObject.RebuildSynth();
        }

        private void InitPreset()
        {
            Debug.Log("init preset");
            CreateOscillator("oscillatorSettings", "Oscillator");
            CreateFilter("filterSettings", "Filter");
            _settingsObject.isInit = true;
            _settingsObject.showOscillator = true;
            EditorUtility.SetDirty(_settingsObject);
        }

        public void DeleteElement<T>(SynthSettingsObjectBase synthSettings, string listName) where T : ScriptableObject
        {
            SerializedProperty filterList = serializedObject.FindProperty(listName);
            synthSettings.RemoveElement<T>(filterList);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_settingsObject);
            _settingsObject.CleanUpPreset();
        }
    }
}