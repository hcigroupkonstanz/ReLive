using HTC.UnityPlugin.Multimedia;
using Relive.Data;
using System.Collections;
using UnityEngine;

namespace Relive.Playback.Video
{
    [RequireComponent(typeof(ViveMediaDecoder))]
    public class AttachmentVideoPlayer : MonoBehaviour
    {
        public string VideoFile;
        public EntityGameObject Parent;
        private ViveMediaDecoder decoder;
        private bool isPlaybackReady;
        private bool isPlaying;

        private void Start()
        {
            decoder = GetComponent<ViveMediaDecoder>();
            decoder.mediaPath = VideoFile;
            decoder.onInitComplete.AddListener(() => isPlaybackReady = true);
            decoder.initDecoder(VideoFile);
        }

        private void OnEnable()
        {
            if (isPlaying)
                StartPlayback();
        }

        private void OnDisable()
        {
            if (!isPlaying)
                decoder.stopDecoding();
        }

        public void StartPlayback()
        {
            isPlaying = true;
            if (isActiveAndEnabled)
                StartCoroutine(StartPlaybackAsync());
        }

        private IEnumerator StartPlaybackAsync()
        {
            while (!isPlaybackReady)
                yield return new WaitForEndOfFrame();

            if (isPlaying)
            {
                decoder.startDecoding();

                int width = 0;
                int height = 0;
                decoder.getVideoResolution(ref width, ref height);
                float ratio = ((float)height) / width;
                transform.localScale = new Vector3(1, ratio, 1);
            }
        }

        public void PausePlayback()
        {
            if (isPlaying)
            {
                isPlaying = false;
                decoder.setPause();
            }
        }

        public void JumpTo(float seconds)
        {
            if (isPlaying)
                decoder.setSeekTime(seconds);
        }
    }
}
