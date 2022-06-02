using HCIKonstanz.Colibri.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace HCIKonstanz.Colibri.Networking
{
    public enum ConnectionStatus { Connected, Disconnected, Connecting, Reconnecting };

    [DefaultExecutionOrder(-100)]
    public class WebServerConnection : SingletonBehaviour<WebServerConnection>
    {
        const int BUFFER_SIZE = 100 * 1024 * 1024;
        const long HEARTBEAT_TIMEOUT_THRESHOLD_MS = 5000;
        const int SOCKET_TIMEOUT_MS = 500;
        const int RECONNECT_DELAY_MS = 5 * 1000;

        private static Socket _socket;
        private static AsyncCallback _receiveCallback = new AsyncCallback(ReceiveData);
        private static byte[] _receiveBuffer = new byte[BUFFER_SIZE];
        private static int _receiveBufferOffset = 0;
        private static int _expectedPacketSize = -1;

        private static UTF8Encoding _encoding = new UTF8Encoding();
        private static LockFreeQueue<InPacket> _queuedCommands = new LockFreeQueue<InPacket>();

        public delegate void MessageAction(string channel, string command, JToken payload);
        public event MessageAction OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;

        private Task _connectionTask;
        private static long LastHeartbeatTime;

        private BehaviorSubject<bool> _isConnected = new BehaviorSubject<bool>(false);
        public IObservable<bool> Connected => _isConnected.Where(x => x).First();


        // workaround to execute events in main unity thread
        private bool fireOnConnected;
        private bool fireOnDisconnected;
        private ConnectionStatus _status = ConnectionStatus.Disconnected;
        public ConnectionStatus Status
        {
            get { return _status; }
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    if (_status == ConnectionStatus.Connected)
                    {
                        fireOnConnected = true;
                        _isConnected.OnNext(true);
                    }
                    else
                    {
                        _isConnected.OnNext(false);
                    }

                    if (_status == ConnectionStatus.Disconnected)
                        fireOnDisconnected = true;
                }
            }
        }


        private struct PacketHeader
        {
            public int PacketSize;
            public int PacketStartOffset;
            public bool IsHeartbeat;
        }

        private struct InPacket
        {
            public string channel;
            public string command;
            public JToken payload;
        }


        private struct OutPacket
        {
            public string group;
            public string channel;
            public string command;
            public JToken payload;
        }





        private void OnEnable()
        {
            DontDestroyOnLoad(this);

            var config = SyncConfiguration.Current;
            if (config.EnableSync && !String.IsNullOrEmpty(config.ServerAddress))
                Connect();

            // to get rid of unity warning
            var ignoreUnityCompilerWarning = new InPacket
            {
                channel = "",
                command = "",
                payload = null
            };
        }

        private void OnDisable()
        {
            if (_socket != null)
                _socket.Dispose();
            _socket = null;
        }

        private void Reconnect()
        {
            if (_socket != null)
            {
                try
                {
                    _socket.Disconnect(false);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }

                _socket.Dispose();
                _socket = null;
            }

            Status = ConnectionStatus.Disconnected;

            Connect();
        }

        private void Connect()
        {
            Status = ConnectionStatus.Connecting;
            bool isReconnect = _socket != null;

            if (_socket != null)
                _socket.Dispose();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true;
            _socket.ReceiveTimeout = SOCKET_TIMEOUT_MS;
            _socket.SendTimeout = SOCKET_TIMEOUT_MS;

            // avoid multithreading problems due to variable changes...
            var config = SyncConfiguration.Current;
            var ip = config.ServerAddress;
            var port = config.UnityPort;
            var socket = _socket;

            Task.Run(async () =>
            {
                if (isReconnect)
                {
                    Status = ConnectionStatus.Reconnecting;
                    await Task.Delay(RECONNECT_DELAY_MS);
                }

                try
                {
                    _receiveBufferOffset = 0;
                    _expectedPacketSize = -1;

                    socket.Connect(ip, port);
                    socket.BeginReceive(_receiveBuffer, _receiveBufferOffset, _receiveBuffer.Length - _receiveBufferOffset, SocketFlags.None, _receiveCallback, null);
                    Debug.Log("Connection to web server established");
                    Status = ConnectionStatus.Connected;
                    LastHeartbeatTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }
                catch (SocketException ex)
                {
                    Debug.LogError(ex.Message);
                    Debug.Log($"Unable to connect to server {ip}, trying again in a few seconds...");
                    Status = ConnectionStatus.Disconnected;
                }
            });

        }

        private void Update()
        {
            if (fireOnConnected)
            {
                OnConnected?.Invoke();
                fireOnConnected = false;
            }

            if (fireOnDisconnected)
            {
                OnDisconnected?.Invoke();
                fireOnDisconnected = false;
            }

            if (Status == ConnectionStatus.Connected)
            {
                while (_queuedCommands.Dequeue(out var packet))
                {
                    OnMessageReceived?.Invoke(packet.channel, packet.command, packet.payload);
                }

                //if (Math.Abs(LastHeartbeatTime - DateTimeOffset.Now.ToUnixTimeMilliseconds()) > HEARTBEAT_TIMEOUT_THRESHOLD_MS)
                //{
                //    Status = ConnectionStatus.Disconnected;
                //    Debug.Log("Connection lost, trying to reconnect...");
                //}
            }

            if (Status == ConnectionStatus.Disconnected && !String.IsNullOrEmpty(SyncConfiguration.Current.ServerAddress))
            {
                Connect();
            }
        }


        private static void ReceiveData(IAsyncResult asyncResult)
        {
            try
            {
                int numReceived = _socket.EndReceive(asyncResult);
                Debug.Assert(numReceived >= 0, "Received negative amount of bytes from surface connection");

                var processingOffset = 0;
                var bufferEnd = _receiveBufferOffset + numReceived;

                while (processingOffset < bufferEnd)
                {
                    if (_expectedPacketSize <= 0)
                    {
                        if (HasPacketHeader(processingOffset))
                        {
                            var header = GetPacketHeader(processingOffset);

                            if (header.IsHeartbeat)
                            {
                                LastHeartbeatTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                processingOffset += header.PacketSize;
                            }
                            else
                            {
                                processingOffset = header.PacketStartOffset;
                                _expectedPacketSize = header.PacketSize;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Invalid packet received, skipping ahead!");
                            while (processingOffset < bufferEnd && !HasPacketHeader(processingOffset))
                            {
                                processingOffset++;
                            }

                        }
                    }
                    else if (processingOffset + _expectedPacketSize <= bufferEnd)
                    {
                        byte[] rawPacket = new byte[_expectedPacketSize];
                        Buffer.BlockCopy(_receiveBuffer, processingOffset, rawPacket, 0, rawPacket.Length);
                        var packet = _encoding.GetString(rawPacket);

                        try
                        {
                            // messages have to be handled in main update() thread, to avoid possible threading issues in handlers
                            var incomingPacket = JsonConvert.DeserializeObject<InPacket>(packet);
                            _queuedCommands.Enqueue(incomingPacket);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                            Debug.LogError(packet);
                        }

                        processingOffset += _expectedPacketSize;
                        _expectedPacketSize = -1;
                    }
                    else
                    {
                        // neither header nor complete package
                        // -> currently incomplete packet in buffer, wait for rest
                        break;
                    }
                }

                if (processingOffset == bufferEnd)
                {
                    // cleared buffer entirely, no need to rearrange memory due to incomplete packet
                    _receiveBufferOffset = 0;
                }
                else
                {
                    // incomplete packet in buffer, move to front
                    _receiveBufferOffset = bufferEnd - processingOffset;
                    Buffer.BlockCopy(_receiveBuffer, processingOffset, _receiveBuffer, 0, _receiveBufferOffset);
                }


                if (_receiveBuffer.Length - _receiveBufferOffset < 100)
                {
                    var error = "Receive buffer getting too small, aborting receive";
                    Debug.LogError(error);
                    throw new OverflowException(error);
                }

                _socket.BeginReceive(_receiveBuffer, _receiveBufferOffset, _receiveBuffer.Length - _receiveBufferOffset, SocketFlags.None, _receiveCallback, null);
            }
            catch (Exception)
            {
                // ignore.
            }
        }


        /*
         *  Message format:
         *  \0\0\0 (Packet header as string) \0 (Actual packet json string)
         */

        private static bool HasPacketHeader(int offset)
        {
            if (offset + 2 >= _receiveBuffer.Length)
            {
                return false;
            }

            if (_receiveBuffer[offset] == '\0' &&
                _receiveBuffer[offset + 1] == '\0' &&
                _receiveBuffer[offset + 2] == '\0')
            {
                return true;
            }

            return false;
        }

        private static PacketHeader GetPacketHeader(int offset)
        {
            var start = offset + 3;
            var end = start;

            if (_receiveBuffer[start] == 'h')
            {
                return new PacketHeader
                {
                    PacketSize = 5,
                    IsHeartbeat = true
                };
            }

            while (end < _receiveBuffer.Length && _receiveBuffer[end] != '\0')
            {
                // searching ...
                end++;
            }

            if (end >= _receiveBuffer.Length)
            {
                throw new OverflowException("Receive buffer overflow");
            }

            // don't want to deal with integer formatting, so it's transmitted as text instead
            byte[] packetSizeRaw = new byte[end - start + 1];
            Buffer.BlockCopy(_receiveBuffer, start, packetSizeRaw, 0, packetSizeRaw.Length);
            var packetSizeText = _encoding.GetString(packetSizeRaw);

            return new PacketHeader
            {
                PacketSize = int.Parse(packetSizeText),
                PacketStartOffset = end + 1,
                IsHeartbeat = false
            };
        }



        private void SendDataAsync(byte[] data)
        {
            if (_socket != null)
            {
                SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
                socketAsyncData.SetBuffer(data, 0, data.Length);
                _socket.SendAsync(socketAsyncData);
            }
        }

        private bool SendData(byte[] data)
        {
            try
            {
                _socket.Send(data);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            return false;
        }

        public void SendCommand(string channel, string command, JToken payload)
        {
            var packet = new OutPacket
            {
                group = "",
                channel = channel,
                command = command,
                payload = payload
            };

            var encoding = new UTF8Encoding();
            var rawData = encoding.GetBytes(JsonConvert.SerializeObject(packet));
            SendDataAsync(rawData);
        }

        public bool SendCommandSync(string channel, string command, JToken payload)
        {
            var packet = new OutPacket
            {
                group = "",
                channel = channel,
                command = command,
                payload = payload
            };

            var encoding = new UTF8Encoding();
            var rawData = encoding.GetBytes(JsonConvert.SerializeObject(packet));

            return SendData(rawData);
        }
    }
}
