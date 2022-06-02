using Cysharp.Threading.Tasks;
using ReLive.Core;
using ReLive.Sessions;
using ReLive.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace ReLive.Logging
{
    [DefaultExecutionOrder(-1000)]
    public class WebLogger : MonoBehaviour
    {
        public string StudyName;
        public StringReactiveProperty ServerAddress = new StringReactiveProperty("localhost:55211");

        private CancellationTokenSource cts;
        private LockFreeQueue<byte[]> sendQueue = new LockFreeQueue<byte[]>();

        // workaround to delay OnDisable() and still send messages
        private bool isActive;
        private bool isReconnecting = false;

        private void OnEnable()
        {
            if (String.IsNullOrEmpty(StudyName))
            {
                Debug.LogError("Please specify study name!");
                enabled = false;
                return;
            }
            StudyName = Uri.EscapeDataString(StudyName.Replace(' ', '_'));

            isActive = true;
            ServerAddress
                .TakeWhile(_ => isActive)
                .Subscribe(Reconnect);

            SyncedObject.Changes
                .TakeWhile(_ => isActive)
                .Subscribe(msg =>
                {
                    // TODO: benchmark if this is really faster
                    sendQueue.Enqueue(Encoding.UTF8.GetBytes($"{{ \"study\": \"{StudyName}\", \"channel\": \"{msg.Channel}\", \"data\": {msg.Payload} }}"));
                });

            Traceable.AttachmentUpdates
                .TakeUntilDisable(this)
                .Subscribe(attachment =>
                {
                    sendQueue.Enqueue(Encoding.UTF8.GetBytes($"{{ \"study\": \"{StudyName}\", \"channel\": \"{attachment.Channel}\", \"id\": \"{attachment.Id}\", \"sessionId\": \"{attachment.SessionId}\", \"data\": {attachment.Attachment} }}"));
                });
        }

        private async void OnDisable()
        {
            // FIXME: wait until session has been sent out... this should ideally not be time-based!
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            isActive = false;

            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        private void Reconnect(string address)
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }

            isReconnecting = true;

            cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    var token = cts.Token;
                    using (var socket = new ClientWebSocket())
                    {
                        var uri = new Uri("wss://" + address);
                        Debug.Log($"Connecting to logging server at {address}");

                        await socket.ConnectAsync(uri, cts.Token);
                        Debug.Log("Connected to logging server!");
                        isReconnecting = false;

                        // start send task
                        while (!token.IsCancellationRequested)
                        {
                            if (socket.State != WebSocketState.Open)
                            {
                                Debug.LogError("WebSocket connection not open, reconnecting...");
                                Reconnect(address);
                                return;
                            }

                            if (sendQueue.Dequeue(out var data))
                            {
                                await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, endOfMessage: true, cancellationToken: cts.Token);
                            }
                            else
                                Thread.Sleep(100);
                        }

                    }

                }
                catch (Exception e)
                {
                    if (!isReconnecting)
                        Reconnect(address);
                    Debug.LogError(e);
                }
            }, cts.Token);
        }

        private async void UploadFile(string entityId, string fileId, string fileData)
        {
            var request = new UnityWebRequest($"http://localhost:55210/file/", "POST");

            var uploadContent = new Dictionary<string, object>
            {
                { "entityId", entityId },
                { "sessionId", SessionController.Instance.SessionId },
                { "fileId", fileId },
                { "fileData", fileData }
            };

            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(uploadContent)));
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            await request.SendWebRequest().AsObservable();
            if (request.isNetworkError)
                Debug.LogError(request.error);
            else
                Debug.Log("File upload success: " + request.downloadHandler.text);
        }
    }
}
