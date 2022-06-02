using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HCIKonstanz.Colibri.Networking;

namespace HCIKonstanz.Colibri.Communication
{
    public class VoiceBroadcast : MonoBehaviour
    {
        [Header("Microphone Settings")]
        public int MicrophoneID = 0;
        public int MicrophoneBufferLengthSeconds = 10;

        [Header("Broadcasting Settings")]
        public int FrameSizeInMilliseconds = 20;

        [Header("Debug")]
        public bool Debugging = false;


        // Debug
        private static readonly string DEBUG_HEADER = "[VoiceBroadcast] ";

        // Microphone recording
        private AudioClip recordingAudioClip;
        private List<float> recordingBuffer;
        private int lastRecordingSamplePosition = 0;
        private bool isInitialized = false;
        private bool startAfterInitialized = false;
        private short startId = -1;
        private float lastConvertedSample = 1f;
        private int microphoneSamplingRate;
        private bool broadcast = false;

        // Networking
        private VoiceServerConnection voiceServerConnection;
        private int packageSizeSamples;
        private short localUserId;

        void Start()
        {
            // Get all available recording devices and select recording device
            string[] recordingDevices = Microphone.devices;
            if (recordingDevices.Length == 0)
            {
                Debug.LogError(DEBUG_HEADER + "Voice Broadcast initialization failed: No recording device found");
                return;
            }
            if (Debugging)
            {
                for (int i = 0; i < recordingDevices.Length; i++)
                {
                    Debug.Log(DEBUG_HEADER + "Recording device found: " + recordingDevices[i] + " ID: " + i);
                }
                Debug.Log(DEBUG_HEADER + "Using recording device: " + recordingDevices[MicrophoneID] + " ID: " + MicrophoneID);
            }

            // Get supported sampling rates of the selected recording device
            int minSupportedSamplingRate;
            int maxSupportedSamplingRate;
            Microphone.GetDeviceCaps(recordingDevices[MicrophoneID], out minSupportedSamplingRate, out maxSupportedSamplingRate);
            if (Debugging) Debug.Log(DEBUG_HEADER + "Sampling rates supported: " + minSupportedSamplingRate + " - " + maxSupportedSamplingRate);
            if (maxSupportedSamplingRate >= 48000 && minSupportedSamplingRate <= 48000)
            {
                microphoneSamplingRate = 48000;
            }
            else if (maxSupportedSamplingRate >= 16000 && minSupportedSamplingRate <= 16000)
            {
                microphoneSamplingRate = 16000;
            }
            else
            {
                microphoneSamplingRate = maxSupportedSamplingRate;
            }
            if (Debugging) Debug.Log(DEBUG_HEADER + "Use sampling rate: " + microphoneSamplingRate);

            // Calculate sample count from milliseconds
            packageSizeSamples = (microphoneSamplingRate / 1000) * FrameSizeInMilliseconds;
            if (Debugging) Debug.Log(DEBUG_HEADER + "Frame size: " + FrameSizeInMilliseconds + " ms, " + packageSizeSamples + " samples");

            // Init server connection
            voiceServerConnection = VoiceServerConnection.Instance;

            isInitialized = true;

            Debug.Log(DEBUG_HEADER + "Ready for Voice Broadcast");

            if (startAfterInitialized)
            {
                StartBroadcast(startId);
            }
        }

        void Update()
        {
            if (isInitialized && broadcast)
            {
                AddSamplesToRecordingBuffer();
                SendSamples();
            }
        }

        public void StartBroadcast(short id)
        {
            if (isInitialized)
            {
                if (microphoneSamplingRate == 48000 || microphoneSamplingRate == 16000)
                {
                    localUserId = id;

                    // Start recording using selected recording device
                    Debug.Log(DEBUG_HEADER + "Start voice broadcasting");
                    recordingAudioClip = Microphone.Start(Microphone.devices[MicrophoneID], true, MicrophoneBufferLengthSeconds, microphoneSamplingRate);
                    if (Debugging) Debug.Log(DEBUG_HEADER + "Channel count: " + recordingAudioClip.channels);
                    lastRecordingSamplePosition = Microphone.GetPosition(null);

                    recordingBuffer = new List<float>();
                    broadcast = true;
                }
                else
                {
                    Debug.LogError(DEBUG_HEADER + "Sampling rate " + microphoneSamplingRate + "Hz currently not supported. A resampling method from " + microphoneSamplingRate + "Hz to 48000Hz must be implemented.");
                }
            }
            else
            {
                startId = id;
                startAfterInitialized = true;
            }
        }

        public void StopBroadcast()
        {
            if (broadcast)
            {
                broadcast = false;
                Microphone.End(Microphone.devices[MicrophoneID]);
            }
        }

        private void AddSamplesToRecordingBuffer()
        {
            int differenceSinceLastAdd = 0;

            // Get sample count since last adding
            int currentSamplePosition = Microphone.GetPosition(null);
            if (currentSamplePosition > lastRecordingSamplePosition)
            {
                differenceSinceLastAdd = currentSamplePosition - lastRecordingSamplePosition;
            }
            else if (currentSamplePosition < lastRecordingSamplePosition)
            {
                differenceSinceLastAdd = (recordingAudioClip.samples - lastRecordingSamplePosition) + currentSamplePosition;
            }
            else
            {
                return;
            }

            // Get samples since last adding
            float[] data = new float[differenceSinceLastAdd];
            recordingAudioClip.GetData(data, lastRecordingSamplePosition);

            // Add samples to recording buffer
            recordingBuffer.AddRange(data);
            lastRecordingSamplePosition = currentSamplePosition;

        }

        private void SendSamples()
        {
            // Check if enough data is available to send a frame
            while (recordingBuffer.Count > packageSizeSamples)
            {
                float[] samples = recordingBuffer.GetRange(0, packageSizeSamples).ToArray();

                // The data must be in the format 48000Hz Mono
                if (microphoneSamplingRate == 16000)
                {
                    samples = Convert16kHZMonoTo48kHzMono(samples);
                }

                // Send voice data to server
                byte[] bytes = ToByteArray(samples);
                voiceServerConnection.SendByteData(localUserId, bytes);

                // Remove samples from recording buffer
                recordingBuffer.RemoveRange(0, packageSizeSamples);
            }
        }

        private float[] Convert16kHZMonoTo48kHzMono(float[] samples)
        {
            float[] kHz48Samples = new float[samples.Length * 3];
            // Interpolating first samples using last sample
            kHz48Samples[0] = Mathf.Lerp(lastConvertedSample, samples[0], 0.33f);
            kHz48Samples[1] = Mathf.Lerp(lastConvertedSample, samples[0], 0.66f);
            kHz48Samples[2] = samples[0];
            // Interpolating all other samples
            for (int i = 1; i < samples.Length - 1; i++)
            {
                int kHz48SamplePosition = i * 3;
                kHz48Samples[kHz48SamplePosition] = Mathf.Lerp(samples[i], samples[i + 1], 0.33f);
                kHz48Samples[kHz48SamplePosition + 1] = Mathf.Lerp(samples[i], samples[i + 1], 0.66f);
                kHz48Samples[kHz48SamplePosition + 2] = samples[i + 1];
            }
            lastConvertedSample = samples[samples.Length - 1];
            return kHz48Samples;
        }

        private byte[] ToByteArray(float[] floatArray)
        {
            int len = floatArray.Length * 4;
            byte[] byteArray = new byte[len];
            int pos = 0;
            foreach (float f in floatArray)
            {
                byte[] data = System.BitConverter.GetBytes(f);
                System.Array.Copy(data, 0, byteArray, pos, 4);
                pos += 4;
            }
            return byteArray;
        }
    }
}
