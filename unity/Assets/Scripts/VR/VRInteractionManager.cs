using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using DirectorsEye.Core;

namespace DirectorsEye.VR
{
    public class VRInteractionManager : MonoBehaviour
    {
        public static VRInteractionManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private Transform headset;
        [SerializeField] private LineRenderer pointerLine;

        [Header("Interaction Settings")]
        [SerializeField] private float grabDistance = 0.1f;
        [SerializeField] private float pointerDistance = 50f;
        [SerializeField] private LayerMask interactableLayers;

        [Header("Prefabs")]
        [SerializeField] private GameObject actorPrefab;
        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private GameObject markerPrefab;

        [Header("Events")]
        public UnityEvent<GameObject> OnObjectGrabbed;
        public UnityEvent<GameObject> OnObjectReleased;
        public UnityEvent OnTriggerPressed;
        public UnityEvent OnGripPressed;

        private InputDevice _leftController;
        private InputDevice _rightController;
        private GameObject _heldObject;
        private Transform _holdingHand;
        private InteractionMode _currentMode = InteractionMode.Pointer;

        public enum InteractionMode
        {
            Pointer,
            Grab,
            PlaceActor,
            PlaceCamera,
            PlaceMarker
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            InitializeControllers();
        }

        void Update()
        {
            UpdateControllerInput();
            UpdatePointer();
            UpdateHeldObject();
        }

        private void InitializeControllers()
        {
            var leftHandDevices = new System.Collections.Generic.List<InputDevice>();
            var rightHandDevices = new System.Collections.Generic.List<InputDevice>();

            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

            if (leftHandDevices.Count > 0) _leftController = leftHandDevices[0];
            if (rightHandDevices.Count > 0) _rightController = rightHandDevices[0];

            Debug.Log("[VRInteraction] Controllers initialized");
        }

        private void UpdateControllerInput()
        {
            // Right trigger - Primary action (capture shot, confirm placement)
            if (_rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
            {
                HandleTriggerPress();
            }

            // Right grip - Grab objects
            if (_rightController.TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed))
            {
                if (gripPressed && _heldObject == null)
                {
                    TryGrabObject(rightHand);
                }
                else if (!gripPressed && _heldObject != null && _holdingHand == rightHand)
                {
                    ReleaseObject();
                }
            }

            // Left grip - Grab with left hand
            if (_leftController.TryGetFeatureValue(CommonUsages.gripButton, out bool leftGripPressed))
            {
                if (leftGripPressed && _heldObject == null)
                {
                    TryGrabObject(leftHand);
                }
                else if (!leftGripPressed && _heldObject != null && _holdingHand == leftHand)
                {
                    ReleaseObject();
                }
            }

            // A button (Pico) - Voice input toggle
            if (_rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool aPressed) && aPressed)
            {
                VoiceInputManager.Instance?.ToggleRecording();
            }

            // B button (Pico) - Cycle interaction mode
            if (_rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bPressed) && bPressed)
            {
                CycleInteractionMode();
            }
        }

        private void UpdatePointer()
        {
            if (pointerLine == null) return;

            var origin = rightHand.position;
            var direction = rightHand.forward;

            pointerLine.SetPosition(0, origin);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, pointerDistance, interactableLayers))
            {
                pointerLine.SetPosition(1, hit.point);
            }
            else
            {
                pointerLine.SetPosition(1, origin + direction * pointerDistance);
            }
        }

        private void UpdateHeldObject()
        {
            if (_heldObject != null && _holdingHand != null)
            {
                _heldObject.transform.position = _holdingHand.position;
                _heldObject.transform.rotation = _holdingHand.rotation;
            }
        }

        private void HandleTriggerPress()
        {
            OnTriggerPressed?.Invoke();

            switch (_currentMode)
            {
                case InteractionMode.PlaceActor:
                    PlaceObjectAtPointer(actorPrefab);
                    break;
                case InteractionMode.PlaceCamera:
                    PlaceObjectAtPointer(cameraPrefab);
                    break;
                case InteractionMode.PlaceMarker:
                    PlaceObjectAtPointer(markerPrefab);
                    break;
                case InteractionMode.Pointer:
                    // Capture shot or interact with UI
                    ShotCaptureManager.Instance?.CaptureShot();
                    break;
            }
        }

        private void TryGrabObject(Transform hand)
        {
            var colliders = Physics.OverlapSphere(hand.position, grabDistance, interactableLayers);
            
            foreach (var col in colliders)
            {
                var grabbable = col.GetComponent<GrabbableObject>();
                if (grabbable != null)
                {
                    _heldObject = grabbable.gameObject;
                    _holdingHand = hand;
                    grabbable.OnGrab();
                    OnObjectGrabbed?.Invoke(_heldObject);
                    OnGripPressed?.Invoke();
                    return;
                }
            }
        }

        private void ReleaseObject()
        {
            if (_heldObject != null)
            {
                var grabbable = _heldObject.GetComponent<GrabbableObject>();
                grabbable?.OnRelease();
                OnObjectReleased?.Invoke(_heldObject);
                _heldObject = null;
                _holdingHand = null;
            }
        }

        private void PlaceObjectAtPointer(GameObject prefab)
        {
            if (prefab == null) return;

            var origin = rightHand.position;
            var direction = rightHand.forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, pointerDistance, interactableLayers))
            {
                var instance = Instantiate(prefab, hit.point, Quaternion.identity);
                instance.transform.up = hit.normal;
                
                Debug.Log($"[VRInteraction] Placed {prefab.name} at {hit.point}");
            }
        }

        private void CycleInteractionMode()
        {
            _currentMode = (InteractionMode)(((int)_currentMode + 1) % Enum.GetValues(typeof(InteractionMode)).Length);
            Debug.Log($"[VRInteraction] Mode: {_currentMode}");
            
            // Update UI to show current mode
            DirectorsEyeUI.Instance?.SetInteractionMode(_currentMode);
        }

        public void SetInteractionMode(InteractionMode mode)
        {
            _currentMode = mode;
            DirectorsEyeUI.Instance?.SetInteractionMode(mode);
        }

        public InteractionMode GetCurrentMode() => _currentMode;
    }

    public class GrabbableObject : MonoBehaviour
    {
        public UnityEvent OnGrabbed;
        public UnityEvent OnReleased;

        private Rigidbody _rb;
        private bool _wasKinematic;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void OnGrab()
        {
            if (_rb != null)
            {
                _wasKinematic = _rb.isKinematic;
                _rb.isKinematic = true;
            }
            OnGrabbed?.Invoke();
        }

        public void OnRelease()
        {
            if (_rb != null)
            {
                _rb.isKinematic = _wasKinematic;
            }
            OnReleased?.Invoke();
        }
    }
}
