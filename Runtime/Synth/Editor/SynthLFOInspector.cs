using Synth;
using Synth.Editor;
using UnityEditor;
using UnityEngine;

namespace UnitySynth.Runtime.Synth.Editor
{
    public class SynthLfoInspector : UnityEditor.Editor
    {
        public static void Draw(SynthSettingsInspector parent, SynthSettingsObjectLFO settings, string listName)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                parent.DeleteElement<SynthSettingsObjectLFO>(settings, listName);
            }

            EditorGUILayout.PrefixLabel("LFO", new GUIStyle { fontStyle = FontStyle.Bold });
            GUILayout.EndHorizontal();

            AnimationCurve curve = new AnimationCurve();
            int count = Mathf.Min((int)Mathf.Abs(settings.frequency * 20), 100);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float fadeIn = Mathf.InverseLerp(0, settings.fadeInDuration, t);
                curve.AddKey(t,
                    Mathf.Sin((t * 10) * settings.frequency) * settings.amp *
                    fadeIn);
            }


            EditorGUILayout.CurveField(curve, Color.red, new Rect(0, -1, 1, 2), GUILayout.Height(60));

            settings.amp = EditorGUILayout.Slider("Amp", settings.amp, .01f, 1);
            settings.frequency = EditorGUILayout.Slider("Frequency", settings.frequency, 10, 1000);
            settings.fadeInDuration = EditorGUILayout.Slider("Fade In Duration", settings.fadeInDuration, 0.001f, 15);


            GUILayout.Space(10);
        }
    }
}