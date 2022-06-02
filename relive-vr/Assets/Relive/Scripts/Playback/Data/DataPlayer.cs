using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dummiesman;
using Relive.Data;
using Relive.Playback.Video;
using Relive.UI;
using Relive.UI.Hierarchy;
using UnityEngine;

namespace Relive.Playback.Data
{
    public class DataPlayer : MonoBehaviour, IPlayer
    {
        // FIXME: workaround for dynamically creating events...
        public static readonly List<DataPlayer> ActiveDataPlayers = new List<DataPlayer>();

        [Header("Default Entity Prefabs")]
        public GameObject DefaultPrefab;
        public GameObject IPadPrefab;
        public GameObject HMDPrefab;
        public GameObject ControllerPrefab;
        public GameObject VideoEntityPrefab;
        public AttachmentVideoPlayer VideoPrefab;
        public GameObject streamIPadPrefab;
        public GameObject streamHMDPrefab;
        public GameObject ARVRTarget;

        [Header("Default Event Prefabs")]
        public GameObject DefaultEventPrefab;

        [HideInInspector]
        public PlaybackSession PlaybackSession;

        private GameObject parentGameObject;
        private LoadedSession loadedSession;
        private bool isStartingPlayback = false;
        private Dictionary<string, EntityGameObject> entityGameObjectDictionary = new Dictionary<string, EntityGameObject>();
        private Dictionary<string, EventGameObject> eventGameObjectDictionary = new Dictionary<string, EventGameObject>();

        private void Awake()
        {
            ActiveDataPlayers.Add(this);
            parentGameObject = DataPlayerObjects.Instance.gameObject;
        }

        private void OnDestroy()
        {
            ActiveDataPlayers.Remove(this);
        }

        public void JumpTo(float seconds)
        {
            if (loadedSession != null && !isStartingPlayback)
            {
                // Create a state with "jump to" timestamp to perform binary search
                State jumpToTimestampState = new State();
                jumpToTimestampState.timestamp = loadedSession.Session.startTime + (long)(seconds * 1000);

                // Calculate and set playback state index of every entity that has states
                foreach (Entity currentEntity in loadedSession.Entities)
                {
                    if (loadedSession.StatesDictionary.ContainsKey(currentEntity.entityId))
                    {
                        int entityPlaybackStateIndex = loadedSession.StatesDictionary[currentEntity.entityId].BinarySearch(jumpToTimestampState, new StateTimestampComparer());
                        if (entityPlaybackStateIndex < 0)
                        {
                            entityPlaybackStateIndex = ~entityPlaybackStateIndex;
                            entityPlaybackStateIndex--;
                        }

                        // When first state in future, but entity has already an GameObject then destroy it
                        if (entityPlaybackStateIndex == -1)
                        {
                            entityGameObjectDictionary[currentEntity.entityId].SetVisible(false);
                            entityGameObjectDictionary[currentEntity.entityId].PlaybackStateIndex = entityPlaybackStateIndex;
                            continue;
                        }

                        // Update entity
                        State currentState = loadedSession.StatesDictionary[currentEntity.entityId][entityPlaybackStateIndex];
                        switch (currentEntity.entityType)
                        {
                            // Create/Update entity GameObject
                            case "object":
                            case "camera":
                                EntityGameObject entityGameObject = entityGameObjectDictionary[currentEntity.entityId];
                                UpdateEntityGameObject(entityGameObject, currentState);
                                entityGameObject.PlaybackStateIndex = entityPlaybackStateIndex; // That is the difference to the Update method
                                break;
                            // Set parent to zero values
                            case "zero":
                                if (currentState.position != null)
                                {
                                    parentGameObject.transform.position = currentState.position.GetVector3();
                                }
                                if (currentState.rotation != null)
                                {
                                    parentGameObject.transform.rotation = currentState.rotation.GetQuaternion();
                                }
                                if (currentState.scale != null)
                                {
                                    parentGameObject.transform.localScale = currentState.scale.GetVector3();
                                }
                                break;
                        }
                    }
                }

                // Calculate and set playback state index of every event that has states
                foreach (EventGameObject eventGameObject in eventGameObjectDictionary.Values)
                {
                    Relive.Data.Event currentEvent = eventGameObject.Event;
                    if (loadedSession.StatesDictionary.ContainsKey(currentEvent.eventId))
                    {
                        int eventPlaybackStateIndex = loadedSession.StatesDictionary[currentEvent.eventId].BinarySearch(jumpToTimestampState, new StateTimestampComparer());
                        if (eventPlaybackStateIndex < 0)
                        {
                            eventPlaybackStateIndex = ~eventPlaybackStateIndex;
                            eventPlaybackStateIndex--;
                        }

                        eventGameObject.PlaybackStateIndex = eventPlaybackStateIndex;
                        // When first state is in future hide GameObject
                        if (eventPlaybackStateIndex == -1)
                        {
                            eventGameObject.SetVisible(false);
                        }
                        else
                        {
                            eventGameObject.SetVisible(true);
                            State currentState = loadedSession.StatesDictionary[currentEvent.eventId][eventPlaybackStateIndex];
                            UpdateEventGameObject(eventGameObject, currentState);
                        }
                    }
                }

                // Calculate and set global playback state index
                int playbackStateIndex = loadedSession.States.BinarySearch(jumpToTimestampState, new StateTimestampComparer());
                if (playbackStateIndex < 0)
                {
                    playbackStateIndex = ~playbackStateIndex;
                    playbackStateIndex--;
                }
                loadedSession.PlaybackStateIndex = Math.Max(0, playbackStateIndex);
            }
        }

        public void Play()
        {
        }

        public void Pause()
        {
        }
        public void StartPlayback(LoadedSession loadedSession)
        {
            isStartingPlayback = true;
            this.loadedSession = loadedSession;
            CreateEntityGameObjects();
            CreateEventGameObjects();
            isStartingPlayback = false;
        }

        private void CreateEntityGameObjects()
        {
            foreach (Entity entity in loadedSession.Entities)
            {
                if (!entityGameObjectDictionary.ContainsKey(entity.entityId))
                {
                    CreateEntityGameObject(entity);
                }
            }
        }

        private async void CreateEntityGameObject(Entity entity)
        {
            GameObject gameObject;
            if (entity.filePaths != null)
            {
                // TODO: move to relive data format
                if (entity.filePaths.ToLower().Contains("ipad_stream"))
                {
                    gameObject = GameObject.Instantiate(streamIPadPrefab);
                }
                else if (entity.filePaths.ToLower().Contains("ipad"))
                {
                    gameObject = GameObject.Instantiate(IPadPrefab);
                }
                else if (entity.filePaths.ToLower().Contains("controller"))
                {
                    gameObject = GameObject.Instantiate(ControllerPrefab);
                }
                else if (entity.filePaths.ToLower().Contains("stream/hmd.obj"))
                {
                    gameObject = GameObject.Instantiate(streamHMDPrefab);
                }
                else if (entity.filePaths.ToLower().Contains("hmd"))
                {
                    gameObject = GameObject.Instantiate(HMDPrefab);
                }
                else if (entity.filePaths.ToLower().Contains("arvrtarget"))
                {
                    gameObject = GameObject.Instantiate(ARVRTarget);
                }
                else
                {
                    string path = "Assets/Data/objects/" + entity.filePaths;
                    if (File.Exists(path))
                    {
                        gameObject = new OBJLoader().Load(path);

                        if (!entity.filePaths.Contains("mediaroom"))
                        {
                            // Generate mesh for MeshCollider
                            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
                            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

                            int i = 0;
                            while (i < meshFilters.Length)
                            {
                                combine[i].mesh = meshFilters[i].sharedMesh;
                                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

                                i++;
                            }
                            Mesh mesh = new Mesh();
                            mesh.CombineMeshes(combine);
                            gameObject.AddComponent<MeshCollider>();
                            gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
                        }
                    }
                    else
                    {
                        gameObject = GameObject.Instantiate(DefaultPrefab);
                    }
                }
            }
            else if (entity.attachments != null && entity.attachments.Any(a => a.type == "model"))
            {
                var attachment = entity.attachments.First(a => a.type == "model");

                Stream stream;
                if (attachment.contentType == "file")
                {
                    // Check if content is global directory and otherwise use local directory
                    if (attachment.content.Contains("/") || attachment.content.Contains("\\"))
                    {
                        stream = File.OpenRead(attachment.content);
                    }
                    else
                    {
                        string path = "Assets/Data/objects/" + attachment.content;
                        if (File.Exists(path))
                        {
                            // stream = File.OpenRead(path);

                            // Load local file without stream
                            gameObject = new OBJLoader().Load(path);
                            goto GOCreated;
                        }
                        else
                        {
                            gameObject = GameObject.Instantiate(DefaultPrefab);
                            goto ModelFinished;
                        }
                    }
                }
                else
                {
                    stream = new MemoryStream();
                    var writer = new StreamWriter(stream);

                    writer.Write(attachment.content);
                    writer.Flush();
                    stream.Position = 0;
                }

                gameObject = new OBJLoader().Load(stream);

            GOCreated:

                if (!attachment.content.Contains("mediaroom"))
                {
                    // Generate mesh for MeshCollider
                    MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
                    CombineInstance[] combine = new CombineInstance[meshFilters.Length];

                    int i = 0;
                    while (i < meshFilters.Length)
                    {
                        combine[i].mesh = meshFilters[i].sharedMesh;
                        combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

                        i++;
                    }
                    Mesh mesh = new Mesh();
                    mesh.CombineMeshes(combine);
                    gameObject.AddComponent<MeshCollider>();
                    gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
                }
            }
            else if (entity.entityType == "video")
            {
                gameObject = Instantiate(VideoEntityPrefab);
            }
            else
            {
                gameObject = GameObject.Instantiate(DefaultPrefab);
            }
        ModelFinished:

            gameObject.transform.parent = parentGameObject.transform;
            gameObject.name = entity.name;

            EntityGameObject entityGameObject = gameObject.AddComponent<EntityGameObject>();
            entityGameObject.Entity = entity;

            if (loadedSession.StatesDictionary.ContainsKey(entity.entityId))
                entityGameObject.States = loadedSession.StatesDictionary[entity.entityId];
            else
                entityGameObject.States = new List<State>();

            entityGameObject.PlaybackSession = PlaybackSession;
            entityGameObjectDictionary.Add(entity.entityId, entityGameObject);

            EntityPanelVR.Instance.AddEntity(entityGameObject);

            // set this at last so that set visible event is called on HierarchyEntityItem
            entityGameObject.SetVisible(false);
        }

        private void UpdateEntityGameObject(EntityGameObject entityGameObject, State state)
        {
            GameObject gameObject = entityGameObject.gameObject;

            // Check if active status changed
            if (state.status == "active" && !gameObject.activeSelf)
            {
                // gameObject.SetActive(true);
                entityGameObject.SetVisible(true);
            }
            else if (state.status == "inactive" && gameObject.activeSelf)
            {
                // gameObject.SetActive(false);
                entityGameObject.SetVisible(false);
            }

            // Check if position changed
            if (state.position != null)
            {
                gameObject.transform.localPosition = state.position.GetVector3();
            }

            // Check if rotation changed
            if (state.rotation != null)
            {
                gameObject.transform.localRotation = state.rotation.GetQuaternion();
            }

            // Check if scaling changed
            if (state.scale != null)
            {
                gameObject.transform.localScale = state.scale.GetVector3();
            }

            // TODO: workaround for STREAM because vive tracker model is crap
            if (entityGameObject.Entity.filePaths != null && entityGameObject.Entity.filePaths.Contains("ObjModelViveTracker"))
            {
                gameObject.transform.localScale = Vector3.one * 0.001f;
            }

            // Set shader if given
            if (state.shader != null)
            {
                Shader shader = Shader.Find(state.shader);
                if (shader)
                {
                    Material material = new Material(shader);
                    gameObject.GetComponent<Renderer>().material = material;
                }
            }

            // Check if color changed
            if (state.color != null)
            {
                Color newColor;
                bool colorParseSuccess = ColorUtility.TryParseHtmlString(state.color, out newColor);
                if (colorParseSuccess && gameObject.GetComponent<Renderer>())
                {
                    gameObject.GetComponent<Renderer>().material.color = newColor;
                }
            }
        }

        private void CreateEventGameObjects()
        {
            foreach (Relive.Data.Event reliveEvent in loadedSession.Events)
            {
                if (!eventGameObjectDictionary.ContainsKey(reliveEvent.eventId))
                {
                    CreateEventGameObject(reliveEvent);
                }
            }
        }

        public void CreateEventGameObject(Relive.Data.Event reliveEvent)
        {
            List<State> eventStates = null;
            if (loadedSession.StatesDictionary.ContainsKey(reliveEvent.eventId))
            {
                eventStates = loadedSession.StatesDictionary[reliveEvent.eventId];
            }
            else if (reliveEvent.position != null)
            {
                var state = new State
                {
                    parentId = reliveEvent.eventId,
                    sessionId = reliveEvent.sessionId,
                    stateType = "event",
                    timestamp = reliveEvent.timestamp,
                    status = "active",
                    position = reliveEvent.position
                };
                eventStates = new List<State> { state };
                loadedSession.StatesDictionary.Add(reliveEvent.eventId, eventStates);
            }

            if (eventStates != null)
            {
                JsonVector3 eventPosition = null;
                foreach (State eventState in eventStates)
                {
                    if (eventState.position != null)
                    {
                        eventPosition = eventState.position;
                        break;
                    }
                }
                if (eventPosition != null)
                {
                    GameObject gameObject = Instantiate(DefaultEventPrefab);
                    gameObject.transform.parent = parentGameObject.transform;
                    // gameObject.transform.localPosition = eventPosition.GetVector3();

                    EventGameObject eventGameObject = gameObject.AddComponent<EventGameObject>();
                    eventGameObject.Event = reliveEvent;
                    eventGameObject.States = eventStates;
                    eventGameObject.PlaybackSession = PlaybackSession;
                    eventGameObjectDictionary.Add(reliveEvent.eventId, eventGameObject);

                    if (reliveEvent.name != null)
                    {
                        gameObject.name = "[Event] " + reliveEvent.name;
                        EventPanelVR.Instance.AddEvent(eventGameObject);
                    }
                    else
                    {
                        gameObject.name = "[Event] " + reliveEvent.eventType;
                    }

                    // set this at last so that set visible event is called on EventItem
                    eventGameObject.SetVisible(false);
                }
            }
        }

        private void UpdateEventGameObject(EventGameObject eventGameObject, State state)
        {
            GameObject gameObject = eventGameObject.gameObject;

            // Check if position changed
            if (state.position != null)
            {
                gameObject.transform.localPosition = state.position.GetVector3();
            }

            // // Check if rotation changed ???
            // if (state.rotation != null)
            // {
            //     gameObject.transform.localRotation = state.rotation.GetQuaternion();
            // }

            // // Check if color changed ???
            // if (state.color != null)
            // {
            //     Color newColor;
            //     bool colorParseSuccess = ColorUtility.TryParseHtmlString(state.color, out newColor);
            //     if (colorParseSuccess && gameObject.GetComponent<Renderer>())
            //     {
            //         gameObject.GetComponent<Renderer>().material.color = newColor;
            //     }
            // }
        }

        private void DestroyDataGameObjects()
        {
            foreach (EntityGameObject entityGameObject in entityGameObjectDictionary.Values)
            {
                Destroy(entityGameObject.gameObject);
            }
            entityGameObjectDictionary = new Dictionary<string, EntityGameObject>();
            foreach (EventGameObject eventGameObject in eventGameObjectDictionary.Values)
            {
                Destroy(eventGameObject.gameObject);
            }
            eventGameObjectDictionary = new Dictionary<string, EventGameObject>();
        }

        public void StopPlayback()
        {
            DestroyDataGameObjects();
        }

        public void Wind(float seconds)
        {
        }
    }
}
