using Cysharp.Threading.Tasks;
using HCIKonstanz.Colibri;
using Relive.Sync;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Relive.Data
{
    public class RemoteDataProvider : IDataProvider
    {
        private UnityWebRequest currentRequest;
        private string selectedStudy;

        public void ActivateStudy(string name)
        {
            var study = StudyManager.Instance.Studies.First(s => s.Name == name);
            study.IsActive = true;
            StudyManager.Instance.ActiveStudy.OnNext(study);
            study.UpdateLocal();
            selectedStudy = name;
        }

        public int GetProgress()
        {
            // TODO: this is no longer really accurate
            if (currentRequest != null)
                return (int)(currentRequest.downloadProgress * 100);
            return 0;
        }

        public async Task<List<Study>> GetStudies()
        {
            var config = SyncConfiguration.Current;
            await StudyManager.Instance.IsInitialized.First(v => v).ToAwaitableEnumerator();
            return StudyManager.Instance.Studies;
        }

        public async Task<List<Session>> GetSessions()
        {
            await SessionManager.Instance.IsInitialized.First(v => v).ToAwaitableEnumerator();
            return SessionManager.Instance.Sessions;
        }

        public async Task<LoadedSession> LoadSession(Session session)
        {
            var entityManager = EntityManager.Instance;
            var eventManager = EventManager.Instance;
            var stateManager = StateManager.Instance;

            var loadedSession = new LoadedSession();
            loadedSession.Session = session;

            await entityManager.IsInitialized.First(v => v).ToAwaitableEnumerator();
            loadedSession.Entities = entityManager.Entities.Where(e => e.sessionId == session.sessionId).ToList();

            // TODO: load entities
            //foreach (var entity in loadedSession.Entities)
            //{
            //    if (entity.attachments != null)
            //    {
            //        foreach (var attachment in entity.attachments)
            //        {
            //            if (attachment.contentType == "persistent")
            //            {
            //                attachment.content = await DownloadAttachment(session.sessionId, entity.entityId, attachment.id);
            //                attachment.contentType = "file";
            //            }
            //        }
            //    }
            //}

            await eventManager.IsInitialized.First(v => v).ToAwaitableEnumerator();
            loadedSession.Events = eventManager.Events.Where(ev => ev.sessionId == session.sessionId).ToList();

            await stateManager.IsInitialized.First(v => v).ToAwaitableEnumerator();
            loadedSession.States = stateManager.States.Where(s => s.sessionId == session.sessionId).ToList();

            return loadedSession;
        }

        private async Task<string> DownloadAttachment(string sessionId, string entityId, string attachmentId)
        {
            var config = SyncConfiguration.Current;
            using (UnityWebRequest request = UnityWebRequest.Get($"{config.Protocol}{config.ServerAddress}:{config.RestPort}/studies/{selectedStudy}/sessions/{sessionId}/entities/{entityId}/attachments/{attachmentId}"))
            {
                request.method = UnityWebRequest.kHttpVerbGET;
                request.downloadHandler = new DownloadHandlerBuffer();
                await request.SendWebRequest();
                if (!request.isNetworkError && request.responseCode == 200)
                {
                    var fileDir = Path.Combine(Application.temporaryCachePath, entityId);
                    var filePath = Path.Combine(fileDir, attachmentId);
                    Debug.Log($"Downloading attachment to {filePath}");
                    Directory.CreateDirectory(fileDir);
                    File.WriteAllBytes(filePath, request.downloadHandler.data);
                    return filePath;
                }
            }

            return null;
        }
    }
}
