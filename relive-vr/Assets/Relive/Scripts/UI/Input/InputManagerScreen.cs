using Relive.Data;
using Relive.Playback.Data;
using Relive.Sync;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Input
{
    public class InputManagerScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button OpenFileButton;
        public Button SessionListButton;
        public GameObject PlayIcon;
        public GameObject PauseIcon;
        public Slider PlaybackTimeSlider;

        [Header("Other")]
        public DataPlayer DataPlayer;

        // Instance
        public static InputManagerScreen Instance;

        private Notebook notebook;

        void Awake()
        {
            Instance = this;
            notebook = NotebookManager.Instance.Notebook;
        }

        void Update()
        {
            if ((UnityEngine.Input.GetKeyDown(KeyCode.Plus) || UnityEngine.Input.GetKeyDown(KeyCode.KeypadPlus)) && Time.timeScale < 20)
            {
                notebook.PlaybackSpeed += 1;
                notebook.UpdateLocal();
            }
            else if ((UnityEngine.Input.GetKeyDown(KeyCode.Minus) || UnityEngine.Input.GetKeyDown(KeyCode.KeypadMinus)) && Time.timeScale > 1)
            {
                notebook.PlaybackSpeed -= 1;
                notebook.UpdateLocal();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                notebook.IsPaused = !notebook.IsPaused;
                notebook.UpdateLocal();
            }
        }
    }
}
