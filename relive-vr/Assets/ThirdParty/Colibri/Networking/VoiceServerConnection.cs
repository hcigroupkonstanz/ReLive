using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using HCIKonstanz.Colibri.Core;
using UnityEngine;

namespace HCIKonstanz.Colibri.Networking
{
    public class VoiceServerConnection : SingletonBehaviour<VoiceServerConnection>
    {

        private UdpClient udpSend;
        private UdpClient udpReceive;
        private IPEndPoint sendIPEndPoint;
        private IPEndPoint receiveIPEndPoint;
        private IPEndPoint inEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private List<Byte> receivedBytes;
        private Thread udpThread;
        private static LockFreeQueue<VoicePacket> queuedBytes = new LockFreeQueue<VoicePacket>();
        private readonly Dictionary<int, List<Action<byte[]>>> bytesListeners = new Dictionary<int, List<Action<byte[]>>>();
        private bool isConnected = false;


        private void OnEnable()
        {
            DontDestroyOnLoad(this);
            var config = SyncConfiguration.Current;
            if (config.EnableSync && !String.IsNullOrEmpty(config.ServerAddress))
                Connect();
        }

        private void OnDisable()
        {
            if (udpReceive != null)
                udpReceive.Dispose();
            if (udpSend != null)
                udpSend.Dispose();
            udpThread.Abort();
        }

        private void Update()
        {
            if (isConnected)
            {
                while (queuedBytes.Dequeue(out var packet))
                {
                    Invoke(packet.Id, packet.Data);
                }
            }
        }

        private void Connect()
        {
            sendIPEndPoint = new IPEndPoint(IPAddress.Parse(SyncConfiguration.Current.ServerAddress), 9003);
            receiveIPEndPoint = new IPEndPoint(IPAddress.Any, 9004);

            udpReceive = new UdpClient();
            udpReceive.ExclusiveAddressUse = false;
            udpReceive.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpReceive.Client.Bind(receiveIPEndPoint);
            udpThread = new Thread(new ThreadStart(Receive));
            udpThread.Name = "Voice UDP Thread";
            udpThread.Start();

            udpSend = new UdpClient();
            udpSend.ExclusiveAddressUse = false;
            udpSend.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpSend.Client.Bind(receiveIPEndPoint);

            isConnected = true;
        }

        private void Receive()
        {
            while (true)
            {
                try
                {
                    byte[] bytes = udpReceive.Receive(ref inEndPoint);
                    VoicePacket voicePacket = GetVoicePacket(bytes);
                    if (voicePacket.Id != 0)
                    {
                        queuedBytes.Enqueue(voicePacket);
                    }
                    // Debug.Log(bytes.Length + " bytes recieved");
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
                // Thread.Sleep(20);
            }
        }

        public void SendByteData(short id, byte[] data)
        {
            byte[] bytes = AddIdBytes(id, data);
            udpSend.Send(bytes, bytes.Length, sendIPEndPoint);
            // Debug.Log(data.Length + " bytes sended");
        }

        public void AddBytesListener(short id, Action<byte[]> listener)
        {
            if (!bytesListeners.ContainsKey(id))
                bytesListeners.Add(id, new List<Action<byte[]>>());
            bytesListeners[id].Add(listener);
        }

        public void RemoveBytesListener(short id, Action<byte[]> listener)
        {
            if (bytesListeners.ContainsKey(id))
        {
            var list = bytesListeners[id];
            list.Remove(listener);
            if (list.Count == 0)
                bytesListeners.Remove(id);
        }
        }

        private void Invoke(int id, byte[] data)
        {
            if (bytesListeners.ContainsKey(id))
            {
                foreach (var bytesListener in bytesListeners[id].ToArray())
                    bytesListener.Invoke(data);
            }
        }

        private VoicePacket GetVoicePacket(byte[] data)
        {
            short id = System.BitConverter.ToInt16(data, 0);
            byte[] sampleData = new byte[data.Length - 2];
            Array.Copy(data, 2, sampleData, 0, sampleData.Length);
            return new VoicePacket() { Id = id, Data = sampleData };
        }

        private byte[] AddIdBytes(short id, byte[] data)
        {
            byte[] bytes = new byte[data.Length + 2];
            byte[] idBytes = System.BitConverter.GetBytes(id);
            Array.Copy(idBytes, bytes, 2);
            Array.Copy(data, 0, bytes, 2, data.Length);
            return bytes;
        }

        private struct VoicePacket
        {
            public short Id;
            public byte[] Data;
        }
    }
}
