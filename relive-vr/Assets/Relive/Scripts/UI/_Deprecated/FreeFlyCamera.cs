using UnityEngine;
using System.Collections;

namespace Relive.UI
{
    [RequireComponent(typeof(Camera))]
    public class FreeFlyCamera : MonoBehaviour
    {
        #region UI

        [Space]

        [SerializeField]
        private bool _active = true;

        [Space]

        [SerializeField]
        private bool _enableRotation = true;

        [SerializeField]
        private float _mouseSense = 1.8f;

        [Space]

        [SerializeField]
        private bool _enableTranslation = true;

        [SerializeField]
        private float _translationSpeed = 55f;

        [Space]

        [SerializeField]
        private bool _enableMovement = true;

        [SerializeField]
        private float _movementSpeed = 10f;

        [SerializeField]
        private float _boostedSpeed = 50f;

        [Space]

        [SerializeField]
        private bool _enableSpeedAcceleration = true;

        [SerializeField]
        private float _speedAccelerationFactor = 1.5f;

        [Space]

        [SerializeField]
        private KeyCode _initPositonButton = KeyCode.R;

        #endregion UI

        private CursorLockMode _wantedMode;

        private float _currentIncrease = 1;
        private float _currentIncreaseMem = 0;

        private Vector3 _initPosition;
        private Vector3 _initRotation;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_boostedSpeed < _movementSpeed)
                _boostedSpeed = _movementSpeed;
        }
#endif


        private void Start()
        {
            _initPosition = transform.position;
            _initRotation = transform.eulerAngles;
        }

        private void OnEnable()
        {
            if (_active)
                _wantedMode = CursorLockMode.None;
        }

        // Apply requested cursor state
        private void SetCursorState()
        {
            if (UnityEngine.Input.GetMouseButtonUp(1))
            {
                Cursor.lockState = _wantedMode = CursorLockMode.None;
            }

            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                _wantedMode = CursorLockMode.Locked;
            }

            // Apply cursor state
            Cursor.lockState = _wantedMode;
            // Hide cursor when locking
            Cursor.visible = (CursorLockMode.Locked != _wantedMode);
        }

        private void CalculateCurrentIncrease(bool moving)
        {
            _currentIncrease = Time.unscaledDeltaTime;

            if (!_enableSpeedAcceleration || _enableSpeedAcceleration && !moving)
            {
                _currentIncreaseMem = 0;
                return;
            }

            _currentIncreaseMem += Time.unscaledDeltaTime * (_speedAccelerationFactor - 1);
            _currentIncrease = Time.unscaledDeltaTime + Mathf.Pow(_currentIncreaseMem, 3) * Time.unscaledDeltaTime;
        }

        private void Update()
        {
            if (!_active)
                return;

            SetCursorState();

            if (Cursor.visible)
                return;

            // Translation
            if (_enableTranslation)
            {
                transform.Translate(Vector3.forward * UnityEngine.Input.mouseScrollDelta.y * Time.unscaledDeltaTime * _translationSpeed);
            }

            // Movement
            if (_enableMovement)
            {
                Vector3 deltaPosition = Vector3.zero;
                float currentSpeed = _movementSpeed;

                if (UnityEngine.Input.GetKey(KeyCode.LeftShift))
                    currentSpeed = _boostedSpeed;

                if (UnityEngine.Input.GetKey(KeyCode.W))
                    deltaPosition += transform.forward;

                if (UnityEngine.Input.GetKey(KeyCode.S))
                    deltaPosition -= transform.forward;

                if (UnityEngine.Input.GetKey(KeyCode.A))
                    deltaPosition -= transform.right;

                if (UnityEngine.Input.GetKey(KeyCode.D))
                    deltaPosition += transform.right;

                if (UnityEngine.Input.GetKey(KeyCode.Q))
                    deltaPosition += transform.up;

                if (UnityEngine.Input.GetKey(KeyCode.E))
                    deltaPosition -= transform.up;

                // Calc acceleration
                CalculateCurrentIncrease(deltaPosition != Vector3.zero);

                transform.position += deltaPosition * currentSpeed * _currentIncrease;
            }

            // Rotation
            if (_enableRotation)
            {
                // Pitch
                transform.rotation *= Quaternion.AngleAxis(
                    -UnityEngine.Input.GetAxis("Mouse Y") * _mouseSense,
                    Vector3.right
                );

                // Paw
                transform.rotation = Quaternion.Euler(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y + UnityEngine.Input.GetAxis("Mouse X") * _mouseSense,
                    transform.eulerAngles.z
                );
            }

            // Return to init position
            if (UnityEngine.Input.GetKeyDown(_initPositonButton))
            {
                transform.position = _initPosition;
                transform.eulerAngles = _initRotation;
            }
        }
    }
}
