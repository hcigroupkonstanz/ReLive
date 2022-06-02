using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HCIKonstanz.Colibri;
using Newtonsoft.Json.Linq;
using Relive.Data;
using Relive.Sync;
using Relive.UI;
using Relive.UI.Input;
using UnityEngine;
using UnityEngine.XR;

namespace Relive.Tools
{
    [RequireComponent(typeof(ControllerPanelVR))]
    [RequireComponent(typeof(AudioSource))]
    public class CameraTool : Tool
    {
        [Header("Custom Properties")]
        public int ScreenshotWidth = 1920;
        public int ScreenshotHeight = 1080;
        public RenderTexture CameraToolRenderTexture;
        public Camera CameraToolCamera;
        public AudioClip CameraShutterSound;
        public GameObject BorderCube;

        private AudioSource cameraToolAudioSource;
        private bool takeScreenshot = false;
        private bool primaryPressed = false;

        private void OnEnable()
        {
            // Attach camera to the right controller
            GetComponent<ControllerPanelVR>().Controller = InputManagerVR.Instance.RightXRController;
            cameraToolAudioSource = GetComponent<AudioSource>();
        }

        public override void AddEntity(string entityName) { }
        public override void RemoveEntity(string entityName) { }

        public override bool AddEntity(EntityGameObject entityGameObject)
        {
            // Tool needs no entities
            return false;
        }

        private void Update()
        {
            if (InputManagerVR.Instance.RightInputDevice != null)
            {
                bool primaryButtonPressed = false;
                InputManagerVR.Instance.RightInputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPressed);
                OnPrimaryButtonPressed(primaryButtonPressed);
            }
        }

        private void LateUpdate()
        {
            if (takeScreenshot)
            {
                TakeScreenshot(CameraToolRenderTexture, CameraToolCamera);
                takeScreenshot = false;

                // Play camera shutter sound
                cameraToolAudioSource.PlayOneShot(CameraShutterSound);

                StartCoroutine("RecordAnimation", 0.4f);
            }
        }

        private void OnPrimaryButtonPressed(bool primaryButtonPressed)
        {
            if (primaryButtonPressed && !primaryPressed)
            {
                BorderCube.GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 255);
                takeScreenshot = true;
            }

            primaryPressed = primaryButtonPressed;
        }

        private IEnumerator RecordAnimation(float time)
        {
            yield return new WaitForSeconds(time);

            BorderCube.GetComponent<Renderer>().material.color = new Color32(0, 0, 0, 255);
        }

        public override void AddInstance(Session session, Color color)
        {
            // not applicable
        }

        public override void RemoveInstance(Session session)
        {
            // not applicable
        }

        public static void TakeScreenshot(RenderTexture rt, Camera cam)
        {
            RenderTexture mRt = new RenderTexture(rt.width, rt.height, rt.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            mRt.antiAliasing = rt.antiAliasing;

            var tex = new Texture2D(mRt.width, mRt.height, TextureFormat.ARGB32, false);
            var prevTargetTexture = cam.targetTexture;
            cam.targetTexture = mRt;
            cam.Render();
            RenderTexture.active = mRt;
            tex.ReadPixels(new Rect(0, 0, mRt.width, mRt.height), 0, 0);
            tex.Apply();

            if (SyncConfiguration.Current.EnableSync)
            {
                var rawData = tex.EncodeToJPG(50);
                var img64 = System.Convert.ToBase64String(rawData);
                var session = SessionManager.Instance.Sessions.FirstOrDefault(s => s.IsActive);
                if (session == null)
                    session = SessionManager.Instance.Sessions.First();

                var ev = new Data.Event
                {
                    eventId = "screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
                    entityIds = null,
                    sessionId = session.sessionId,
                    eventType = "screenshot",
                    timestamp = session.startTime + (long)(NotebookManager.Instance.Notebook.PlaybackTimeSeconds * 1000),
                    attachments = new List<Attachment>
                    {
                        new Attachment
                        {
                            id = "screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
                            type = "image",
                            content = img64,
                            contentType = "attached"
                        }
                    },
                    position = new JsonVector3
                    {
                        x = cam.transform.position.x,
                        y = cam.transform.position.y,
                        z = cam.transform.position.z
                    }
                };

                EventManager.Instance.AddEvent(ev);
            }
            else
            {
                string filename = "Assets/Data/screenshots/screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
                System.IO.File.WriteAllBytes(filename, tex.EncodeToPNG());
                Debug.Log("Screenshot saved: " + filename);
            }

            DestroyImmediate(tex);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;

            DestroyImmediate(mRt);
            cam.targetTexture = prevTargetTexture;
        }

        public override void RemoveEvent(string eventName)
        {

        }

        public override void AddEvent(string eventName)
        {

        }

        public override bool AddEvent(EventGameObject eventGameObject)
        {
            return false;
        }

        private static JObject ToJson(Vector3 v)
        {
            var json = new JObject();
            json.Add("x", v.x);
            json.Add("y", v.y);
            json.Add("z", v.z);
            return json;
        }
    }
}