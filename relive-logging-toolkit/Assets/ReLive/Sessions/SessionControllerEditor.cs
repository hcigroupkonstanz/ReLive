#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ReLive.Sessions
{
    [CustomEditor(typeof(SessionController))]
    public class SessionControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var controller = target as SessionController;

            GUILayout.Space(16);

            using (new EditorGUI.DisabledScope(!Application.isPlaying || controller.IsSessionActive))
            {
                if (GUILayout.Button("Start Session"))
                    controller.StartSession();
            }

            using (new EditorGUI.DisabledScope(!Application.isPlaying || !controller.IsSessionActive))
            {
                if (GUILayout.Button("Stop session"))
                    controller.StopSession();
            }
        }
    }
}
#endif
