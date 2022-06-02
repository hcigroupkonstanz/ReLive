using UnityEditor;
using UnityEngine;

namespace HCIKonstanz.Colibri.Samples
{
    [CustomEditor(typeof(SampleObservableModel))]
    public class SampleObservableModelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SampleObservableModel so = (SampleObservableModel)target;

            EditorGUILayout.LabelField($"ID: {so.Id}");

            if (Application.isPlaying)
            {
                EditorGUILayout.PrefixLabel("TestString");
                var input = EditorGUILayout.TextField(so.TestString);
                so.TestString = input;
            }
            else
            {
                EditorGUILayout.LabelField($"TestString: {so.TestString}");
            }
        }
    }
}
