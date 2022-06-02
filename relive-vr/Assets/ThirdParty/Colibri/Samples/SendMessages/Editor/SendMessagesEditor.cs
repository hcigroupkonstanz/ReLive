using HCIKonstanz.Colibri.Sync;
using UnityEditor;
using UnityEngine;

namespace HCIKonstanz.Colibri.Samples
{
    [CustomEditor(typeof(SendMessages))]
    public class SendMessagesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SendMessages s = (SendMessages)target;

            if (s && Application.isPlaying)
            {
                var cmds = SyncCommands.Instance;
                if (GUILayout.Button("Sync Bool"))
                    cmds.SendData(SendMessages.Channel, s.SyncedBool);
                if (GUILayout.Button("Sync Int"))
                    cmds.SendData(SendMessages.Channel, s.SyncedInt);
                if (GUILayout.Button("Sync Float"))
                    cmds.SendData(SendMessages.Channel, s.SyncedFloat);
                if (GUILayout.Button("Sync String"))
                    cmds.SendData(SendMessages.Channel, s.SyncedString);
                if (GUILayout.Button("Sync Vector2"))
                    cmds.SendData(SendMessages.Channel, s.SyncedVector2);
                if (GUILayout.Button("Sync Vector3"))
                    cmds.SendData(SendMessages.Channel, s.SyncedVector3);
                if (GUILayout.Button("Sync Quaternion"))
                    cmds.SendData(SendMessages.Channel, s.SyncedQuaternion);
                if (GUILayout.Button("Sync Color"))
                    cmds.SendData(SendMessages.Channel, s.SyncedColor);

                if (GUILayout.Button("Sync BoolArray"))
                    cmds.SendData(SendMessages.Channel, s.SyncedBoolArray);
                if (GUILayout.Button("Sync IntArray"))
                    cmds.SendData(SendMessages.Channel, s.SyncedIntArray);
                if (GUILayout.Button("Sync FloatArray"))
                    cmds.SendData(SendMessages.Channel, s.SyncedFloatArray);
                if (GUILayout.Button("Sync StringArray"))
                    cmds.SendData(SendMessages.Channel, s.SyncedStringArray);
                if (GUILayout.Button("Sync Vector2Array"))
                    cmds.SendData(SendMessages.Channel, s.SyncedVector2Array);
                if (GUILayout.Button("Sync Vector3Array"))
                    cmds.SendData(SendMessages.Channel, s.SyncedVector3Array);
                if (GUILayout.Button("Sync QuaternionArray"))
                    cmds.SendData(SendMessages.Channel, s.SyncedQuaternionArray);
                if (GUILayout.Button("Sync ColorArray"))
                    cmds.SendData(SendMessages.Channel, s.SyncedColorArray);
            }
        }
    }
}
