using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HCIKonstanz.Colibri;
using Relive.Data;
using Relive.Playback.Data;
using Relive.Playback.Video;
using Relive.Sync;
using Relive.UI;
using Relive.UI.Playback;
using UniRx;
using UnityEngine;

namespace Relive.Playback
{
    public class PlaybackManager : MonoBehaviour
    {
        public static PlaybackManager Instance;

        public DataPlayer DataPlayerPrefab;
        public VideoPlayerVR VideoPlayerVRPrefab;
        public PlaybackSessionsPanelVR PlaybackSessionsPanelVR;
        public InfoPanelVR InfoPanelVR;

        private Notebook notebook;
        private IDataProvider dataProvider;

        public float PlaybackLengthSeconds
        {
            get
            {
                if (PlaybackSessions.Count != 0)
                {
                    float longestSessionLength = 0f;
                    foreach (PlaybackSession playbackSession in PlaybackSessions)
                    {
                        if (playbackSession.PlaybackLengthSeconds > longestSessionLength)
                        {
                            longestSessionLength = playbackSession.PlaybackLengthSeconds;
                        }
                    }

                    return longestSessionLength;
                }
                else
                {
                    return 1f;
                }
            }
        }

        public int PlaybackSessionCount
        {
            get { return PlaybackSessions.Count; }
        }

        public readonly List<PlaybackSession> PlaybackSessions = new List<PlaybackSession>();

        private void Update()
        {
            if (!notebook.IsPaused)
            {
                if (PlaybackSessions.Count > 0)
                {
                    notebook.PlaybackTimeSeconds += Time.deltaTime;
                }
            }

            foreach (var playbackSession in PlaybackSessions)
            {
                playbackSession.JumpTo(notebook.PlaybackTimeSeconds - PlaybackSessionsPanelVR.GetSecondsForPositionOnSlider(playbackSession));
            }

            PlaybackSessionsPanelVR.UpdateSliderValue(notebook.PlaybackTimeSeconds);
        }

        private async void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(Instance.gameObject);
            }
            else if (this != Instance)
            {
                Destroy(gameObject);
            }

            notebook = NotebookManager.Instance.Notebook;
            dataProvider = DataManager.Instance.DataProvider;

            InfoPanelVR.gameObject.SetActive(true);

            List<Session> sessions;
            if (dataProvider is RemoteDataProvider rdp)
            {
                sessions = await dataProvider.GetSessions();
                // states may take a while longer to load during remote sync
                await StateManager.Instance.IsInitialized.First(v => v).ToAwaitableEnumerator();
            }
            else
                sessions = await dataProvider.GetSessions();

            InfoPanelVR.gameObject.SetActive(false);

            // react to any changes (local or remote) to the sessions
            foreach (var session in sessions)
            {
                session.OnUpdate += OnSessionUpdate;
                if (session.IsActive)
                    StartPlayback(session);
            }
        }

        private void OnSessionUpdate(Session session)
        {
            if (session.IsActive)
                StartPlayback(session);
            else
            {
                var playbackSession = PlaybackSessions.FirstOrDefault(ps => ps.LoadedSession.Session == session);
                if (playbackSession != null)
                    StopPlayback(playbackSession);
            }
        }


        public async void StartPlayback(Session session)
        {
            // do nothing if session is already playing
            if (PlaybackSessions.Any(ps => ps.LoadedSession.Session == session))
                return;

            PlaybackSession playbackSession = gameObject.AddComponent<PlaybackSession>();
            var loadedSession = await dataProvider.LoadSession(session);
            playbackSession.StartPlayback(loadedSession);
            PlaybackSessions.Add(playbackSession);

            if (PlaybackSessions.Count == 1 && !SyncConfiguration.Current.EnableSync)
            {
                Play();
            }
            else if (PlaybackSessions.Count > 1)
            {
                foreach (PlaybackSession ps in PlaybackSessions)
                {
                    ps.ShowOutline = true;
                }
            }

            PlaybackSessionsPanelVR.AddSlider(playbackSession);
        }

        public void StopPlayback()
        {
            foreach (var session in PlaybackSessions)
            {
                StopPlayback(session);
            }
        }

        private void StopPlayback(PlaybackSession session)
        {
            session.StopPlayback();
            Destroy(session);
            PlaybackSessions.Remove(session);

            if (PlaybackSessions.Count < 2)
            {
                foreach (var s in PlaybackSessions)
                {
                    s.ShowOutline = false;
                }
            }

            PlaybackSessionsPanelVR.RemoveSlider(session);
        }

        public void Play()
        {
            notebook.IsPaused = false;
            notebook.UpdateLocal();

            foreach (PlaybackSession playbackSession in PlaybackSessions)
            {
                playbackSession.Play();
            }
        }

        public void Pause()
        {
            notebook.IsPaused = true;
            notebook.UpdateLocal();

            foreach (PlaybackSession playbackSession in PlaybackSessions)
            {
                playbackSession.Pause();
            }
        }

        public void JumpTo(float seconds)
        {
            // FIXME: workaround so that sessions dont jump to 0 if no session is active
            if (seconds > 0)
            {
                notebook.PlaybackTimeSeconds = seconds;
                notebook.UpdateLocal();
            }
        }

        public void Wind(float seconds)
        {
            notebook.PlaybackTimeSeconds += seconds;
            notebook.UpdateLocal();

            // foreach (PlaybackSession playbackSession in playbackSessions)
            // {
            //     playbackSession.Wind(seconds);
            // }
        }
    }
}