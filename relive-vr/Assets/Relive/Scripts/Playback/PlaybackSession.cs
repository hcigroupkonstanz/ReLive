using System.Collections.Generic;
using Relive.Data;
using Relive.Playback;
using Relive.Playback.Data;
using Relive.Visualizations;
using UnityEngine;

public class PlaybackSession : MonoBehaviour, IPlayer
{
    public float PlaybackTimeSeconds;
    public float PlaybackLengthSeconds
    {
        get
        {
            if (LoadedSession != null)
            {
                return (LoadedSession.Session.endTime - LoadedSession.Session.startTime) / 1000.0f;
            }
            else
            {
                return 1f;
            }
        }
    }

    public LoadedSession LoadedSession { get; private set; }

    public Color SessionColor
    {
        get
        {
            if (LoadedSession.Session.Color == null) LoadedSession.Session.Color = ColorGenerator.GenerateSessionColorHex();
            if (ColorUtility.TryParseHtmlString(LoadedSession.Session.Color, out var color))
                return color;
            return Color.red;
        }
    }
    public bool ShowOutline = false;

    private List<IPlayer> players = new List<IPlayer>();

    public void StartPlayback(LoadedSession loadedSession)
    {
        LoadedSession = loadedSession;

        // Create DataPlayer
        DataPlayer dataPlayer = Instantiate(PlaybackManager.Instance.DataPlayerPrefab);
        dataPlayer.gameObject.name = "DataPlayer(" + loadedSession.Session.name + ")";
        dataPlayer.PlaybackSession = this;
        players.Add(dataPlayer);

        // TODO: Create for every Video a VideoPlayerVR
        // Something with entity.attachments???

        foreach (IPlayer player in players)
        {
            player.StartPlayback(loadedSession);
        }
    }

    public void StopPlayback()
    {
        foreach (IPlayer player in players)
        {
            player.StopPlayback();
            if (player is MonoBehaviour)
            {
                Destroy(((MonoBehaviour)player).gameObject);
            }
        }
    }

    public void JumpTo(float seconds)
    {
        if (seconds >= 0f)
        {
            PlaybackTimeSeconds = seconds;
        }
        else
        {
            PlaybackTimeSeconds = 0f;
        }

        foreach (IPlayer player in players)
        {
            player.JumpTo(seconds);
        }
    }

    public void Pause()
    {
        foreach (IPlayer player in players)
        {
            player.Pause();
        }
    }

    public void Play()
    {
        foreach (IPlayer player in players)
        {
            player.Play();
        }
    }

    public void Wind(float seconds)
    {
        foreach (IPlayer player in players)
        {
            player.Wind(seconds);
        }
    }
}
