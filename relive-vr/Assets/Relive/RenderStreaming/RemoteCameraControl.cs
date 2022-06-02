using Relive.Data;
using Relive.Sync;
using System.Linq;
using Unity.RenderStreaming;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Relive.RenderStreaming
{
    [RequireComponent(typeof(Camera))]
    public class RemoteCameraControl : MonoBehaviour, IRemoteInput
    {
        [Range(0.01f, 0.4f)]
        public float RotationSensitivityX = 0.4f;
        [Range(0.01f, 0.4f)]
        public float RotationSensitivityY = 0.2f;

        [Range(0.1f, 1.0f)]
        public float MovementSpeed = 0.2f;
        public float MovementBoostFactor = 3f;

        // Scroll values are usually between -125 and 125?
        [Range(0.0001f, 0.001f)]
        public float ScrollSensitivity = 0.0005f;

        private Notebook notebook;

        private Keyboard m_keyboard;
        private Mouse m_mouse;
        private Camera rsCamera;
        private SceneViewType prevViewType;

        private void OnEnable()
        {
            notebook = NotebookManager.Instance.Notebook;
            prevViewType = notebook.SceneView;
            Unity.RenderStreaming.RenderStreaming.Instance?.AddController(this);
            rsCamera = GetComponent<Camera>();
        }

        private void OnDisable()
        {
            Unity.RenderStreaming.RenderStreaming.Instance?.RemoveController(this);
        }

        public void SetInput(IInput input)
        {
            m_mouse = input.RemoteMouse;
            m_keyboard = input.RemoteKeyboard;
        }

        private void FixedUpdate()
        {
            EntityGameObject followEntity = null;
            string sessionId = null;

            if (notebook.SceneViewOptions.ContainsKey("follow-entity-sessionId"))
                sessionId = notebook.SceneViewOptions["follow-entity-sessionId"].Value as string;

            if (notebook.SceneViewOptions.ContainsKey("follow-entity-name"))
            {
                var followEntityName = notebook.SceneViewOptions["follow-entity-name"].Value as string;
                if (sessionId != null)
                    followEntity = EntityGameObject.ActiveEntities.FirstOrDefault(e => e.Entity.name == followEntityName && e.Entity.sessionId == sessionId);
                else
                    followEntity = EntityGameObject.ActiveEntities.FirstOrDefault(e => e.Entity.name == followEntityName);
            }

            bool hasViewChanged = false;
            if (prevViewType != notebook.SceneView)
            {
                hasViewChanged = true;
                prevViewType = notebook.SceneView;
                rsCamera.orthographic = notebook.SceneView == SceneViewType.Isometric;
            }

            switch (notebook.SceneView)
            {
                case SceneViewType.FreeFly:
                    UpdateInputFreefly();
                    break;

                case SceneViewType.FollowEntity:
                    if (followEntity)
                        UpdateInputFollowEntity(followEntity, hasViewChanged);
                    break;

                case SceneViewType.ViewEntity:
                    if (followEntity)
                    {
                        transform.position = followEntity.transform.position;
                        transform.rotation = followEntity.transform.rotation;
                    }
                    break;

                case SceneViewType.VR:
                    transform.position = Camera.main.transform.position;
                    transform.rotation = Camera.main.transform.rotation;
                    break;

                case SceneViewType.Isometric:
                    UpdateInputIsometric(hasViewChanged);
                    break;
            }
        }

        private void UpdateInputFollowEntity(EntityGameObject entity, bool resetToDefault)
        {
            // set position
            if (resetToDefault)
                transform.position = entity.transform.position + Vector3.one * 0.35f;
            transform.position += transform.forward * (-m_mouse.scroll.y.ReadValue() * ScrollSensitivity);


            // set rotation
            transform.LookAt(entity.transform);
            if (m_mouse.rightButton.isPressed)
            {
                var mouseInput = m_mouse.delta.ReadValue();
                transform.RotateAround(entity.transform.position, Vector3.up, -mouseInput.x * RotationSensitivityX);
                transform.RotateAround(entity.transform.position, transform.right, -mouseInput.y * RotationSensitivityY);
            }
        }

        private void UpdateInputIsometric(bool resetToDefault)
        {
            if (resetToDefault)
                rsCamera.orthographicSize = 8;

            var translation = GetInputTranslationDirection() * Time.deltaTime;

            Vector2 mouseInput = Vector2.zero;
            if (m_mouse.rightButton.isPressed)
                mouseInput = m_mouse.delta.ReadValue();

            if (notebook.SceneViewOptions.TryGetValue("iso-perspective", out var v))
            {
                if (v.Value as string == "top-down")
                {
                    if (resetToDefault)
                    {
                        rsCamera.transform.position = new Vector3(0, 2, 0);
                        rsCamera.transform.LookAt(rsCamera.transform.position + Vector3.down);
                    }

                    rsCamera.transform.Rotate(new Vector3(0, 0, -mouseInput.x) * RotationSensitivityX);

                    // adjust tranlation to be more like '2d' top down
                    translation = Quaternion.Euler(-90, 0, 0) * translation;
                    // replace up/down (q/e) with orthoSize to achieve the same effect
                    rsCamera.orthographicSize -= translation.z;
                    translation.z = 0;

                    rsCamera.transform.position += rsCamera.transform.rotation  * translation;
                }

                if (v.Value as string == "side")
                {
                    if (resetToDefault)
                        rsCamera.transform.position = new Vector3(0, 1, 2);

                    // replace forward/back (w/s) with orthoSize to achieve the same effect
                    rsCamera.orthographicSize -= translation.z;
                    translation.z = 0;

                    rsCamera.transform.LookAt(rsCamera.transform.position + Vector3.forward);
                    rsCamera.transform.position += rsCamera.transform.rotation * translation;
                }
            }


        }


        private void UpdateInputFreefly()
        {
            if (m_mouse.rightButton.isPressed)
            {
                var input = m_mouse.delta.ReadValue();

                rsCamera.transform.Rotate(new Vector3(-input.y * RotationSensitivityY, input.x * RotationSensitivityX, 0));

                var rot = rsCamera.transform.rotation.eulerAngles;
                rsCamera.transform.rotation = Quaternion.Euler(rot.x, rot.y, 0);
            }

            var translation = GetInputTranslationDirection() * Time.deltaTime;

            if (m_keyboard.leftShiftKey.isPressed)
                translation *= MovementBoostFactor;

            rsCamera.transform.position += rsCamera.transform.rotation * translation;
        }

        private Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = new Vector3();
            if (m_keyboard.wKey.isPressed)
                direction += Vector3.forward * MovementBoostFactor;
            if (m_keyboard.sKey.isPressed)
                direction += Vector3.back * MovementBoostFactor;
            if (m_keyboard.aKey.isPressed)
                direction += Vector3.left * MovementBoostFactor;
            if (m_keyboard.dKey.isPressed)
                direction += Vector3.right * MovementBoostFactor;
            if (m_keyboard.qKey.isPressed)
                direction += Vector3.up * MovementBoostFactor;
            if (m_keyboard.eKey.isPressed)
                direction += Vector3.down * MovementBoostFactor;

            return direction;
        }
    }
}

