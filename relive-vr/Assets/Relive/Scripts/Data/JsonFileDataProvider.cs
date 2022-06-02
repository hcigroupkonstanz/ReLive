using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Relive.Data
{
    public class JsonFileDataProvider : IDataProvider
    {
        public bool IsReading { get; private set; }

        private JsonFile jsonFile;
        private string filePath;

        private long fileLength;
        private Stream stream;
        private Task<JsonFile> jsonReaderTask;

        public JsonFileDataProvider(string filePath)
        {
            this.filePath = filePath;
            LoadJson();
        }


        public async Task<List<Session>> GetSessions()
        {
            var sessions = new List<Session>();
            await Task.Run(async () =>
            {
                if (jsonReaderTask != null)
                    jsonFile = await jsonReaderTask;
                sessions = jsonFile.sessions;
            });

            return sessions;
        }

        public async Task<LoadedSession> LoadSession(Session session)
        {
            var sessionFile = new LoadedSession();
            sessionFile.Session = session;

            await Task.Run(async () =>
            {
                if (jsonReaderTask != null)
                    jsonFile = await jsonReaderTask;
                sessionFile.Entities = jsonFile.entities.Where(entity => entity.sessionId == session.sessionId).ToList();
                sessionFile.Events = jsonFile.events.Where(ourEvent => ourEvent.sessionId == session.sessionId).ToList();
                List<State> states = jsonFile.states.Where(state => state.sessionId == session.sessionId).ToList();
                sessionFile.States = FillStates(states);
            });

            return sessionFile;
        }

        public int GetProgress()
        {
            if (IsReading && stream != null && fileLength > 0)
                return (int)(((double)stream.Position / fileLength) * 100);
            else
                return -1;
        }

        private async void LoadJson()
        {
            jsonReaderTask = OpenFile(filePath);
            jsonFile = await jsonReaderTask;
        }

        private Task<JsonFile> OpenFile(string path)
        {
            return Task.Run(() =>
            {
                IsReading = true;
                JsonFile result = null;

                FileInfo fileInfo = new FileInfo(path);
                fileLength = fileInfo.Length;
                JsonSerializer jsonSerializer = new JsonSerializer();
                using (StreamReader streamReader = new StreamReader(path))
                using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                {
                    stream = streamReader.BaseStream;
                    result = jsonSerializer.Deserialize<JsonFile>(jsonTextReader);
                }
                stream = null;
                IsReading = false;
                return result;
            });
        }

        private List<State> FillStates(List<State> states)
        {
            Dictionary<string, State> lastValuesDictionary = new Dictionary<string, State>();
            State lastState = null;
            foreach (State state in states)
            {
                if (!lastValuesDictionary.ContainsKey(state.parentId))
                {
                    lastValuesDictionary[state.parentId] = new State
                    {
                        status = state.status,
                        position = state.position,
                        rotation = state.rotation,
                        scale = state.scale,
                        color = state.color,
                        shader = state.shader,
                        speed = 0f,
                        distanceMoved = 0f,
                    };
                }
                else
                {
                    // Status
                    if (state.status == null)
                    {
                        state.status = lastValuesDictionary[state.parentId].status;
                    }
                    else
                    {
                        lastValuesDictionary[state.parentId].status = state.status;
                    }
                    // Position
                    if (state.position == null)
                    {
                        state.position = lastValuesDictionary[state.parentId].position;
                    }
                    else
                    {
                        // If there was no position set before set position here (I think this is also necessary for the other values like rotation, ...)
                        if (lastValuesDictionary[state.parentId].position == null)
                        {
                            lastValuesDictionary[state.parentId].position = state.position;
                        }

                        // Calculate speed
                        float meters = Vector3.Distance(state.position.GetVector3(), lastValuesDictionary[state.parentId].position.GetVector3());
                        float seconds = (float)(state.timestamp - lastState.timestamp) / 1000f;
                        if (seconds != 0f)
                        {
                            state.speed = meters / seconds;
                        }

                        lastValuesDictionary[state.parentId].position = state.position;

                        // Calculate distance moved
                        state.distanceMoved = lastValuesDictionary[state.parentId].distanceMoved + meters;
                        lastValuesDictionary[state.parentId].distanceMoved = state.distanceMoved;
                    }
                    // Rotation
                    if (state.rotation == null)
                    {
                        state.rotation = lastValuesDictionary[state.parentId].rotation;
                    }
                    else
                    {
                        lastValuesDictionary[state.parentId].rotation = state.rotation;
                    }
                    // Scale
                    if (state.scale == null)
                    {
                        state.scale = lastValuesDictionary[state.parentId].scale;
                    }
                    else
                    {
                        lastValuesDictionary[state.parentId].scale = state.scale;
                    }
                    // Color
                    if (state.color == null)
                    {
                        state.color = lastValuesDictionary[state.parentId].color;
                    }
                    else
                    {
                        lastValuesDictionary[state.parentId].color = state.color;
                    }
                    // Shader
                    if (state.shader == null)
                    {
                        state.shader = lastValuesDictionary[state.parentId].shader;
                    }
                    else
                    {
                        lastValuesDictionary[state.parentId].shader = state.shader;
                    }
                    // Speed
                    if (state.speed == -1f)
                    {
                        state.speed = lastValuesDictionary[state.parentId].speed;
                    }
                    else
                    {
                        lastValuesDictionary[state.parentId].speed = state.speed;
                    }
                }
                lastState = state;
            }
            return states;
        }
    }
}
