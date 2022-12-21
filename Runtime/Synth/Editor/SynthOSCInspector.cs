using System;
using UnityEditor;
using UnityEngine;

namespace Synth.Editor
{
    public class SynthOSCInspector : UnityEditor.Editor
    {
        public static void Draw(SynthSettingsInspector parent, SynthSettingsObjectOscillator settings)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                parent.DeleteElement(settings);
            }

            EditorGUILayout.PrefixLabel("Voice", new GUIStyle {fontStyle = FontStyle.Bold});
            GUILayout.EndHorizontal();

            //AnimationCurve curve = new AnimationCurve();
            //EditorGUILayout.CurveField(curve, Color.red, new Rect(0, -1, 1, 2), GUILayout.Height(60));

            var types = Enum.GetNames(typeof(SynthSettingsObjectOscillator.OscillatorType));

            GUILayout.Space(10);
            settings.oscillatorType =
                (SynthSettingsObjectOscillator.OscillatorType) GUILayout.SelectionGrid((int) settings.oscillatorType,
                    types, 3);
            GUILayout.Space(5);

            settings.amplitude = EditorGUILayout.Slider("Volume", settings.amplitude, 0f, 1f);
            

            switch (settings.oscillatorType)
            {
                case SynthSettingsObjectOscillator.OscillatorType.Simple:
                    settings.tuning = (int) EditorGUILayout.Slider("Tuning", settings.tuning, -24, 24);
                    settings.simpleOscillatorType =
                        (SynthSettingsObjectOscillator.SimpleOscillatorTypes) EditorGUILayout.EnumPopup(
                            "Waveform", settings.simpleOscillatorType);
                    break;
                case SynthSettingsObjectOscillator.OscillatorType.WaveTable:
                    settings.tuning = (int) EditorGUILayout.Slider("Tuning", settings.tuning, -24, 24);
                     EditorGUILayout.Slider("Resolution", 32, 8, 128);
                    settings.waveTableOscillatorType =
                        (SynthSettingsObjectOscillator.WaveTableOscillatorTypes) EditorGUILayout.EnumPopup(
                            "Wavetable", settings.waveTableOscillatorType);
                    break;
                case SynthSettingsObjectOscillator.OscillatorType.Noise:
                    settings.noiseType =
                        (SynthSettingsObjectOscillator.NoiseTypes) EditorGUILayout.EnumPopup(
                            "Noise type", settings.noiseType);
                    break;
            }


            GUILayout.Space(10);
        }
    }
}