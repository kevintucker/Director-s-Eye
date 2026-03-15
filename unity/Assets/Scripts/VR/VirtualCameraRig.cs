using UnityEngine;
using UnityEngine.Events;

namespace DirectorsEye.VR
{
    public class VirtualCameraRig : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera virtualCamera;
        [SerializeField] private RenderTexture viewfinderTexture;
        [SerializeField] private float defaultFocalLength = 35f;
        [SerializeField] private float minFocalLength = 16f;
        [SerializeField] private float maxFocalLength = 200f;

        [Header("Display")]
        [SerializeField] private MeshRenderer viewfinderDisplay;
        [SerializeField] private Transform viewfinderPivot;
        [SerializeField] private TextMesh focalLengthText;
        [SerializeField] private TextMesh frameInfoText;

        [Header("Frame Guides")]
        [SerializeField] private bool showRuleOfThirds = true;
        [SerializeField] private bool showCenterCross = true;
        [SerializeField] private AspectRatio aspectRatio = AspectRatio.Widescreen_16_9;

        public UnityEvent<Texture2D> OnFrameCaptured;

        private float _currentFocalLength;
        private int _shotNumber = 1;

        public enum AspectRatio
        {
            Widescreen_16_9,
            Cinemascope_2_39_1,
            Academy_1_85_1,
            IMAX_1_43_1,
            Square_1_1
        }

        void Awake()
        {
            if (virtualCamera == null)
            {
                virtualCamera = GetComponentInChildren<Camera>();
            }

            if (viewfinderTexture == null)
            {
                viewfinderTexture = new RenderTexture(1920, 1080, 24);
                virtualCamera.targetTexture = viewfinderTexture;
            }

            if (viewfinderDisplay != null)
            {
                viewfinderDisplay.material.mainTexture = viewfinderTexture;
            }

            SetFocalLength(defaultFocalLength);
        }

        void Start()
        {
            UpdateFrameInfo();
        }

        public void SetFocalLength(float mm)
        {
            _currentFocalLength = Mathf.Clamp(mm, minFocalLength, maxFocalLength);
            
            // Convert focal length to FOV
            // FOV = 2 * atan(sensorHeight / (2 * focalLength))
            // Using 35mm full-frame sensor height (24mm)
            float sensorHeight = 24f;
            float fovRadians = 2f * Mathf.Atan(sensorHeight / (2f * _currentFocalLength));
            float fovDegrees = fovRadians * Mathf.Rad2Deg;
            
            virtualCamera.fieldOfView = fovDegrees;

            if (focalLengthText != null)
            {
                focalLengthText.text = $"{_currentFocalLength:F0}mm";
            }

            UpdateFrameInfo();
        }

        public void AdjustFocalLength(float delta)
        {
            SetFocalLength(_currentFocalLength + delta);
        }

        public void SetAspectRatio(AspectRatio ratio)
        {
            aspectRatio = ratio;
            
            float aspect = ratio switch
            {
                AspectRatio.Widescreen_16_9 => 16f / 9f,
                AspectRatio.Cinemascope_2_39_1 => 2.39f,
                AspectRatio.Academy_1_85_1 => 1.85f,
                AspectRatio.IMAX_1_43_1 => 1.43f,
                AspectRatio.Square_1_1 => 1f,
                _ => 16f / 9f
            };

            virtualCamera.aspect = aspect;
            UpdateFrameInfo();
        }

        public Texture2D CaptureFrame()
        {
            var currentRT = RenderTexture.active;
            RenderTexture.active = viewfinderTexture;

            var frame = new Texture2D(viewfinderTexture.width, viewfinderTexture.height, TextureFormat.RGB24, false);
            frame.ReadPixels(new Rect(0, 0, viewfinderTexture.width, viewfinderTexture.height), 0, 0);
            frame.Apply();

            RenderTexture.active = currentRT;

            OnFrameCaptured?.Invoke(frame);
            _shotNumber++;
            UpdateFrameInfo();

            Debug.Log($"[VirtualCamera] Captured frame #{_shotNumber - 1}");
            return frame;
        }

        public CameraMetadata GetMetadata()
        {
            return new CameraMetadata
            {
                position = transform.position,
                rotation = transform.rotation.eulerAngles,
                focalLength = _currentFocalLength,
                fieldOfView = virtualCamera.fieldOfView,
                aspectRatio = aspectRatio.ToString(),
                shotNumber = _shotNumber
            };
        }

        private void UpdateFrameInfo()
        {
            if (frameInfoText != null)
            {
                frameInfoText.text = $"Shot {_shotNumber} | {_currentFocalLength:F0}mm | {aspectRatio}";
            }
        }

        void OnDrawGizmos()
        {
            if (virtualCamera == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.matrix = virtualCamera.transform.localToWorldMatrix;
            Gizmos.DrawFrustum(Vector3.zero, virtualCamera.fieldOfView, 10f, 0.1f, virtualCamera.aspect);
        }
    }

    [System.Serializable]
    public class CameraMetadata
    {
        public Vector3 position;
        public Vector3 rotation;
        public float focalLength;
        public float fieldOfView;
        public string aspectRatio;
        public int shotNumber;
    }
}
