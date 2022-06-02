using System.Collections.Generic;
using UnityEngine;

namespace Relive.Data
{
    public class LoadedSession
    {
        // Session
        public Session Session;

        // Entities
        private List<Entity> entities;
        public List<Entity> Entities
        {
            get { return entities; }
            set
            {
                entities = value;
                EntityDictionary = new Dictionary<string, Entity>();
                foreach (Entity entity in entities)
                {
                    EntityDictionary[entity.entityId] = entity;
                }
            }
        }
        public Dictionary<string, Entity> EntityDictionary { get; private set; }

        // Events
        private List<Event> events;
        public List<Event> Events
        {
            get { return events; }
            set
            {
                events = value;
                EventDictionary = new Dictionary<string, Event>();
                foreach (Event reliveEvent in events)
                {
                    EventDictionary[reliveEvent.eventId] = reliveEvent;
                }
            }
        }
        public Dictionary<string, Event> EventDictionary { get; private set; }

        // States
        private List<State> states;
        public List<State> States
        {
            get { return states; }
            set
            {
                states = value;
                StatesDictionary = new Dictionary<string, List<State>>();
                foreach (State state in states)
                {
                    if (!StatesDictionary.ContainsKey(state.parentId))
                    {
                        StatesDictionary[state.parentId] = new List<State>();
                    }

                    StatesDictionary[state.parentId].Add(state);

                    if (state.stateType == "entity")
                    {
                        if (EntityDictionary.ContainsKey(state.parentId))
                        {
                            // if (EntityDictionary[state.parentId].name == "192.168.1.4")
                            // {
                            //     // MonoBehaviour.print(EntityDictionary[state.parentId].maxSpeed + ", " + state.speed);
                            // }


                            EntityDictionary[state.parentId].averageSpeed += state.speed;
                            // EntityDictionary[state.parentId].stdevSpeed += (state.speed - mean * state.speed);
                        }

                    }
                }

                // ========= Z SCORE CALCULATION =========

                foreach (Entity entity in Entities)
                {
                    if (StatesDictionary.ContainsKey(entity.entityId))
                    {
                        entity.averageSpeed /= StatesDictionary[entity.entityId].Count;

                        foreach (State state in StatesDictionary[entity.entityId])
                        {
                            entity.stdevSpeed += (state.speed - entity.averageSpeed) * (state.speed - entity.averageSpeed);
                        }

                        entity.stdevSpeed = Mathf.Sqrt(entity.stdevSpeed / StatesDictionary[entity.entityId].Count);
                    }
                    else
                    {
                        Debug.LogWarning("Entity " + entity.name + " has no states");
                    }

                }



                foreach (Entity entity in Entities)
                {
                    if (StatesDictionary.ContainsKey(entity.entityId))
                    {
                        for (int i = 1; i < StatesDictionary[entity.entityId].Count; i++)
                        {
                            State state = StatesDictionary[entity.entityId][i];
                            float zScore = (state.speed - EntityDictionary[state.parentId].averageSpeed) / EntityDictionary[state.parentId].stdevSpeed;
                            if (zScore > 2.5f)
                            {
                                state.speed = StatesDictionary[entity.entityId][i - 1].speed;
                            }

                            // Determine max speed for each entity
                            if (EntityDictionary[state.parentId].maxSpeed < state.speed)
                            {
                                EntityDictionary[state.parentId].maxSpeed = state.speed;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Entity " + entity.name + " has no states");
                    }
                }
            }
        }
        public Dictionary<string, List<State>> StatesDictionary { get; private set; }

        private int playbackStateIndex = 0;
        public int PlaybackStateIndex
        {
            get => Mathf.Min(playbackStateIndex, states.Count - 1);
            set => playbackStateIndex = value;
        }

        public bool HasReachedEnd
        {
            get { return playbackStateIndex >= states.Count; }
        }
    }
}
