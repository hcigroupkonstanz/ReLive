using System.Collections;
using System.Collections.Generic;
using HCIKonstanz.Colibri.Networking;
using UnityEngine;

namespace HCIKonstanz.Colibri.Communication
{
    [RequireComponent(typeof(AudioSource))]
    public class VoiceReceiver : MonoBehaviour
    {
        // Debug
        public bool Debugging = false;

        
        private static readonly string DEBUG_HEADER = "[VoiceReceiver] ";
        private float timer = 0.0f;

        // Playback audio
        private AudioSource playbackAudioSource;
        private List<float> playbackBuffer;
        private VoiceServerConnection voiceServerConnection;
        private short remoteUserId;
        private bool playback = false;

        void Awake()
        {
            voiceServerConnection = VoiceServerConnection.Instance;
            playbackAudioSource = GetComponent<AudioSource>();
            playbackAudioSource.bypassEffects = false;
            playbackAudioSource.bypassListenerEffects = false;
            playbackAudioSource.bypassReverbZones = true;
            playbackAudioSource.playOnAwake = false;
            playbackAudioSource.loop = true;
        }

        void Start()
        {
            // Debug
            if (Debugging)
            {
                Debug.Log(DEBUG_HEADER + "Output sampling rate: " + AudioSettings.outputSampleRate);
                Debug.Log(DEBUG_HEADER + "Output channel mode: " + AudioSettings.speakerMode);
                Debug.Log(DEBUG_HEADER + "Spatialize: " + playbackAudioSource.spatialize);
            }
        }

        void Update()
        {
            // Debug: Check if data in playback buffer is constant
            if (Debugging && playback)
            {
                timer += Time.deltaTime;
                if (timer > 5f)
                {
                    Debug.Log(DEBUG_HEADER + "Id: " + remoteUserId + " | Samples in playback buffer: " + playbackBuffer.Count);
                    timer = 0f;
                }
            }
        }

        // Use the MonoBehaviour.OnAudioFilterRead callback to playback voice data as fast as possible
        void OnAudioFilterRead(float[] data, int channels)
        {
            if (playback)
            {
                // Check if the playback buffer has enough data to fill up the data array or otherwise use only the available data
                int dataBufferSize = Mathf.Min(data.Length, playbackBuffer.Count);
                // Get the data from the playback buffer, override the data array with it and remove it from the buffer
                float[] dataBuffer = playbackBuffer.GetRange(0, dataBufferSize).ToArray();
                dataBuffer.CopyTo(data, 0);
                playbackBuffer.RemoveRange(0, dataBufferSize);
                // Clear not already overwritten data
                for (int i = dataBufferSize; i < data.Length; i++)
                {
                    data[i] = 0;
                }
            }
        }

        public void StartPlayback(short id)
        {
            // playbackBuffer = new List<float>();
            // remoteUserId = transform.parent.GetComponent<RemoteUserInfo>().UserId;
            remoteUserId = id;
            voiceServerConnection.AddBytesListener(remoteUserId, OnSamplesDataReceived);
            playbackAudioSource.Play();
            playback = true;
            playbackBuffer = new List<float>();
            Debug.Log(DEBUG_HEADER + "Start voice playback with ID: " + remoteUserId);
        }

        public void StopPlayback()
        {
            Debug.Log(DEBUG_HEADER + "Stop voice playback of ID: " + remoteUserId);
            playback = false;
            playbackAudioSource.Stop();
            voiceServerConnection.RemoveBytesListener(remoteUserId, OnSamplesDataReceived);
        }

        private void OnSamplesReceived(float[] samples)
        {
            // Convert mono samples to stereo
            float[] stereoSamples = ConvertToStereo(samples);
            // Add samples to playback buffer
            playbackBuffer.AddRange(stereoSamples);
        }

        public void OnSamplesDataReceived(byte[] samplesData)
        {
            // Convert bytes to float samples
            float[] samples = ToFloatArray(samplesData);
            // Convert mono samples to stereo
            float[] stereoSamples = ConvertToStereo(samples);
            // Add samples to playback buffer
            playbackBuffer.AddRange(stereoSamples); // Sometimes ArgumentOutOfRangeException
        }

        private float[] ConvertToStereo(float[] samples)
        {
            float[] stereoSamples = new float[samples.Length * 2];
            int stereoIndex = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                stereoSamples[stereoIndex] = samples[i];
                stereoSamples[stereoIndex + 1] = samples[i];
                stereoIndex += 2;
            }
            return stereoSamples;
        }

        private float[] ToFloatArray(byte[] byteArray)
        {
            int len = byteArray.Length / 4;
            float[] floatArray = new float[len];
            for (int i = 0; i < byteArray.Length; i += 4)
            {
                floatArray[i / 4] = System.BitConverter.ToSingle(byteArray, i);
            }
            return floatArray;
        }
    }
}
