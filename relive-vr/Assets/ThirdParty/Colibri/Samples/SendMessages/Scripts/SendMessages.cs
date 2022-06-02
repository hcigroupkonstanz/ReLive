using HCIKonstanz.Colibri.Sync;
using UnityEngine;

namespace HCIKonstanz.Colibri.Samples
{
    public class SendMessages : MonoBehaviour
    {
        public static readonly string Channel = "SendTestMessages";

        public bool SyncedBool;
        public int SyncedInt;
        public float SyncedFloat;
        public string SyncedString;
        public Vector2 SyncedVector2;
        public Vector3 SyncedVector3;
        public Quaternion SyncedQuaternion;
        public Color SyncedColor;

        public bool[] SyncedBoolArray;
        public int[] SyncedIntArray;
        public float[] SyncedFloatArray;
        public string[] SyncedStringArray;
        public Vector2[] SyncedVector2Array;
        public Vector3[] SyncedVector3Array;
        public Quaternion[] SyncedQuaternionArray;
        public Color[] SyncedColorArray;


        private SyncCommands _cmds;

        private void OnEnable()
        {
            _cmds = SyncCommands.Instance;
            _cmds.AddBoolListener(Channel, OnBoolMessage);
            _cmds.AddIntListener(Channel, OnIntMessage);
            _cmds.AddFloatListener(Channel, OnFloatMessage);
            _cmds.AddStringListener(Channel, OnStringMessage);
            _cmds.AddVector2Listener(Channel, OnVector2Message);
            _cmds.AddVector3Listener(Channel, OnVector3Message);
            _cmds.AddQuaternionListener(Channel, OnQuaternionMessage);
            _cmds.AddColorListener(Channel, OnColorMessage);

            _cmds.AddBoolArrayListener(Channel, OnBoolArrayMessage);
            _cmds.AddIntArrayListener(Channel, OnIntArrayMessage);
            _cmds.AddFloatArrayListener(Channel, OnFloatArrayMessage);
            _cmds.AddStringArrayListener(Channel, OnStringArrayMessage);
            _cmds.AddVector2ArrayListener(Channel, OnVector2ArrayMessage);
            _cmds.AddVector3ArrayListener(Channel, OnVector3ArrayMessage);
            _cmds.AddQuaternionArrayListener(Channel, OnQuaternionArrayMessage);
            _cmds.AddColorArrayListener(Channel, OnColorArrayMessage);
        }

        private void OnDisable()
        {
            _cmds.RemoveBoolListener(Channel, OnBoolMessage);
            _cmds.RemoveIntListener(Channel, OnIntMessage);
            _cmds.RemoveFloatListener(Channel, OnFloatMessage);
            _cmds.RemoveStringListener(Channel, OnStringMessage);
            _cmds.RemoveVector2Listener(Channel, OnVector2Message);
            _cmds.RemoveVector3Listener(Channel, OnVector3Message);
            _cmds.RemoveQuaternionListener(Channel, OnQuaternionMessage);
            _cmds.RemoveColorListener(Channel, OnColorMessage);

            _cmds.RemoveBoolArrayListener(Channel, OnBoolArrayMessage);
            _cmds.RemoveIntArrayListener(Channel, OnIntArrayMessage);
            _cmds.RemoveFloatArrayListener(Channel, OnFloatArrayMessage);
            _cmds.RemoveStringArrayListener(Channel, OnStringArrayMessage);
            _cmds.RemoveVector2ArrayListener(Channel, OnVector2ArrayMessage);
            _cmds.RemoveVector3ArrayListener(Channel, OnVector3ArrayMessage);
            _cmds.RemoveQuaternionArrayListener(Channel, OnQuaternionArrayMessage);
            _cmds.RemoveColorArrayListener(Channel, OnColorArrayMessage);
        }


        private void OnBoolMessage(bool val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedBool = val;
        }

        private void OnIntMessage(int val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedInt = val;
        }

        private void OnFloatMessage(float val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedFloat = val;
        }

        private void OnStringMessage(string val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedString = val;
        }

        private void OnVector2Message(Vector2 val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedVector2 = val;
        }

        private void OnVector3Message(Vector3 val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedVector3 = val;
        }

        private void OnQuaternionMessage(Quaternion val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedQuaternion = val;
        }

        private void OnColorMessage(Color val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedColor = val;
        }



        private void OnBoolArrayMessage(bool[] val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedBoolArray = val;
        }

        private void OnIntArrayMessage(int[] val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedIntArray = val;
        }

        private void OnFloatArrayMessage(float[] val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedFloatArray = val;
        }

        private void OnStringArrayMessage(string[] val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedStringArray = val;
        }

        private void OnVector2ArrayMessage(Vector2[] val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedVector2Array = val;
        }

        private void OnVector3ArrayMessage(Vector3[] val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedVector3Array = val;
        }

        private void OnQuaternionArrayMessage(Quaternion[] val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedQuaternionArray = val;
        }

        private void OnColorArrayMessage(Color[] val)
        {
            Debug.Log($"Received message with value '{val}'");
            SyncedColorArray = val;
        }
    }
}
