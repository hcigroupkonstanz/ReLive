using System.Collections.Generic;
using HCIKonstanz.Colibri.Communication;
using HCIKonstanz.Colibri.Sync;
using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    public static readonly string CHANNEL = "VoiceChatSample";
    public short Id;
    public float DisconnectUserAfterSeconds = 2f;
    public VoiceBroadcast VoiceBroadcast;
    public GameObject VoiceReceiverPrefab;

    
    private SyncCommands syncCommands;
    private float timeSinceLastSended = 1f;
    private Dictionary<int, GameObject> clients;
    private Dictionary<int, float> clientsLastHeartbeat;

    void OnEnable()
    {
        // Create random id
        Id = (short)UnityEngine.Random.Range(1, 32000);

        // Receive IDs of other clients
        syncCommands = SyncCommands.Instance;
        syncCommands.AddIntListener(CHANNEL, OnIdArrived);

        // Start broadcasting voice
        VoiceBroadcast.StartBroadcast(Id);
    }

    void OnDisable()
    {
        // Remove receive ID listener
        syncCommands.RemoveIntListener(CHANNEL, OnIdArrived);

        // Stop broadcasting voice
        VoiceBroadcast.StopBroadcast();
    }

    void Update()
    {
        // Send the own ID every second to other clients
        timeSinceLastSended += Time.deltaTime;
        if (timeSinceLastSended > 1f)
        {
            syncCommands.SendData(CHANNEL, Id);
            timeSinceLastSended = 0f;
        }

        // Check if clients are not available anymore
        if (clientsLastHeartbeat != null)
        {
            List<int> removeIds = new List<int>();
            foreach (int id in clientsLastHeartbeat.Keys)
            {
                if (clientsLastHeartbeat[id] >= DisconnectUserAfterSeconds)
                {
                    removeIds.Add(id);
                }
            }
            // Remove all not available clients
            foreach (int id in removeIds)
            {
                Destroy(clients[id]);
                clients.Remove(id);
                clientsLastHeartbeat.Remove(id);
            }
        }
    }

    private void OnIdArrived(int id)
    {
        if (clients == null)
        {
            clients = new Dictionary<int, GameObject>();
            clientsLastHeartbeat = new Dictionary<int, float>();
        }

        // If a ID arrives that has no VoiceReceiver at the moment instantiate one
        if (!clients.ContainsKey(id))
        {
            GameObject voiceReceiverPrefab = Instantiate(VoiceReceiverPrefab);
            clients.Add(id, voiceReceiverPrefab);
            clientsLastHeartbeat.Add(id, Time.time);
            // Start plaback the voice of the user with the id
            voiceReceiverPrefab.GetComponent<VoiceReceiver>().StartPlayback((short)id);
        }
        else
        {
            // Update last heartbeat time
            clientsLastHeartbeat[id] = Time.time;
        }

    }

}
