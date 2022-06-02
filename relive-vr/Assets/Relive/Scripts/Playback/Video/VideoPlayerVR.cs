using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Relive.Data;
using UnityEngine;
using UnityEngine.Video;

namespace Relive.Playback.Video
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoPlayerVR : MonoBehaviour, IPlayer
    {
        private VideoPlayer videoPlayer;
        private string videoPath;
        private long videoStartTimeMilliseconds;
        private long videoEndTimeMilliseconds;
        private LoadedSession loadedSession;

        void Start()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            // PlaybackManager.Instance.AddPlayer(this);
        }

        void Update()
        {

        }

        // Assign video manually
        public void AssignVideo(string videoPath)
        {
            this.videoPath = videoPath;
            videoPlayer.url = videoPath;
            // string fileName = videoPath.Split('\\').Last();
            string fileName = videoPath.Split('/').Last();
            string start = fileName.Substring(9, 14);
            string end = fileName.Substring(24, 14);

            DateTime startDateTime = DateTime.ParseExact(start, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            videoStartTimeMilliseconds = (long)startDateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            DateTime endDateTime = DateTime.ParseExact(end, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            videoEndTimeMilliseconds = (long)endDateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        public void StartPlayback(LoadedSession loadedSession)
        {
            this.loadedSession = loadedSession;
            // TODO: Get the video path out of the session
        }

        public void Play()
        {
            if (videoPlayer.url != "")
            {
                videoPlayer.Play();
            }
        }

        public void Pause()
        {
            if (videoPlayer.url != "")
            {
                videoPlayer.Pause();
            }
        }

        public void JumpTo(float seconds)
        {
            if (videoPlayer.canSetTime)
            {
                videoPlayer.time = seconds;
            }
        }

        public void StopPlayback()
        {
            throw new NotImplementedException();
        }

        public void Wind(float seconds)
        {
            throw new NotImplementedException();
        }
    }
}
