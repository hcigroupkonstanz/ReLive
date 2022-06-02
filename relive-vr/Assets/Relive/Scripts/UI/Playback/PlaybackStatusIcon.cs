using UnityEngine;
using UnityEngine.UI;
using Relive.Playback;
using Relive.Sync;

namespace Relive.UI.Playback
{
    public class PlaybackStatusIcon : MonoBehaviour
    {
        public Sprite playIcon;
        public Sprite pauseIcon;

        private Image image;
        private bool pauseIconVisible = true;

        void OnEnable()
        {
            image = GetComponent<Image>();
        }

        void Update()
        {
            var notebook = NotebookManager.Instance.Notebook;
            if (notebook.IsPaused && pauseIconVisible)
            {
                image.sprite = playIcon;
                pauseIconVisible = false;
            }
            else if (!notebook.IsPaused && !pauseIconVisible)
            {
                image.sprite = pauseIcon;
                pauseIconVisible = true;
            }
        }
    }
}