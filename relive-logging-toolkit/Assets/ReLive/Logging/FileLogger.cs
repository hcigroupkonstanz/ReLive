using ReLive.Core;
using ReLive.Sessions;
using ReLive.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace ReLive.Logging
{
    [DefaultExecutionOrder(-1000)]
    public class FileLogger : MonoBehaviour
    {
        private class JsonStreamWriter : IDisposable
        {
            private bool hasEntries = false;
            private StreamWriter writer;

            public JsonStreamWriter(string path)
            {
                writer = new StreamWriter(new FileStream(path, FileMode.Create));
                writer.WriteLine('[');
            }

            public void Dispose()
            {
                writer.WriteLine(Environment.NewLine + ']');
                writer.Dispose();
            }

            public void WriteEntry(string entry)
            {
                if (hasEntries)
                    writer.Write("," + Environment.NewLine);

                writer.Write(entry);
                hasEntries = true;
            }

            public void FlushAsync()
            {
                writer.FlushAsync();
            }
        }

        // uses path based on Application.PersistentDataPath
        public bool UseRelativePersistentPath;
        // TODO: better name?
        public bool UseDateAsFolderName;
        public string Filepath;

        private CancellationTokenSource cts;
        private LockFreeQueue<SyncMessage> queuedUpdates;
        private LockFreeQueue<Traceable.AttachmentMessage> queuedAttachments;

        private void OnEnable()
        {
            cts = new CancellationTokenSource();
            queuedUpdates = new LockFreeQueue<SyncMessage>();
            queuedAttachments = new LockFreeQueue<Traceable.AttachmentMessage>();
            var cancelToken = cts.Token;

            SyncedObject.Changes
                .TakeUntilDisable(this)
                .Subscribe(msg => queuedUpdates.Enqueue(msg));

            Traceable.AttachmentUpdates
                .TakeUntilDisable(this)
                .Subscribe(attachment => queuedAttachments.Enqueue(attachment));

            var baseDir = Filepath;

            if (UseRelativePersistentPath)
            {
                baseDir = Path.Combine(Application.persistentDataPath, Filepath);
            }


            if (UseDateAsFolderName)
            {
                var dateString = DateTime.Now.ToString("yyyy-MM-ddTHHmmss");
                baseDir = Path.Combine(baseDir, dateString);
            }

            _ = Task.Run(() =>
            {
                try
                {
                    Debug.Log("Writing to " + baseDir);
                    Directory.CreateDirectory(baseDir);

                    using (var sessionWriter = new JsonStreamWriter(Path.Combine(baseDir, "sessions.json")))
                    using (var entityWriter = new JsonStreamWriter(Path.Combine(baseDir, "entities.json")))
                    using (var eventWriter = new JsonStreamWriter(Path.Combine(baseDir, "events.json")))
                    using (var stateWriter = new JsonStreamWriter(Path.Combine(baseDir, "states.json")))
                    {

                        while (!cancelToken.IsCancellationRequested)
                        {
                            if (queuedUpdates.Dequeue(out var update))
                            {
                                switch (update.Channel)
                                {
                                    case "sessions":
                                        sessionWriter.WriteEntry(update.Payload);
                                        break;
                                    case "entities":
                                        entityWriter.WriteEntry(update.Payload);
                                        break;
                                    case "events":
                                        eventWriter.WriteEntry(update.Payload);
                                        break;
                                    case "states":
                                        stateWriter.WriteEntry(update.Payload);
                                        break;
                                    default:
                                        Debug.LogError($"Unknown channel: {update}");
                                        break;
                                }
                            }
                            else if (queuedAttachments.Dequeue(out var msg))
                            {
                                // TODO: filetype?
                                // TODO: attach id to entity
                                var attachmentPath = Path.Combine(baseDir, msg.Channel + "_" + msg.Id);
                                Directory.CreateDirectory(attachmentPath);
                                // TODO: inconsistent, may need some data from attachment..
                                File.WriteAllText(Path.Combine(attachmentPath, msg.Id + ".obj"), msg.Attachment);
                            }
                            else
                            {
                                sessionWriter.FlushAsync();
                                entityWriter.FlushAsync();
                                eventWriter.FlushAsync();
                                stateWriter.FlushAsync();
                                Thread.Sleep(100);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }, cancelToken);
        }

        private void OnDisable()
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }
}
