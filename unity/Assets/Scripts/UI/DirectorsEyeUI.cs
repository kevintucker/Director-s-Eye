using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DirectorsEye.Core;
using DirectorsEye.VR;

namespace DirectorsEye.UI
{
    public class DirectorsEyeUI : MonoBehaviour
    {
        public static DirectorsEyeUI Instance { get; private set; }

        [Header("Main Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject recordingIndicator;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private GameObject shotListPanel;

        [Header("Status Display")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI modeText;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Image recordingPulse;

        [Header("Shot List")]
        [SerializeField] private Transform shotListContent;
        [SerializeField] private GameObject shotItemPrefab;

        [Header("Voice Feedback")]
        [SerializeField] private Image voiceWaveform;
        [SerializeField] private TextMeshProUGUI transcriptionPreview;

        [Header("Tool Palette")]
        [SerializeField] private Button actorButton;
        [SerializeField] private Button cameraButton;
        [SerializeField] private Button markerButton;
        [SerializeField] private Button captureButton;
        [SerializeField] private Button voiceButton;

        private Color _defaultButtonColor;
        private Color _activeButtonColor = new Color(0.2f, 0.8f, 0.4f);

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
            if (actorButton != null)
                _defaultButtonColor = actorButton.colors.normalColor;

            SetupEventListeners();
            HideAllIndicators();
        }

        private void SetupEventListeners()
        {
            if (VoiceInputManager.Instance != null)
            {
                VoiceInputManager.Instance.OnRecordingStarted.AddListener(ShowRecordingIndicator);
                VoiceInputManager.Instance.OnRecordingStopped.AddListener(HideRecordingIndicator);
                VoiceInputManager.Instance.OnTranscriptionComplete.AddListener(OnTranscriptionReceived);
            }

            if (SceneGenerationManager.Instance != null)
            {
                SceneGenerationManager.Instance.OnGenerationStarted.AddListener(ShowLoadingIndicator);
                SceneGenerationManager.Instance.OnSceneGenerated.AddListener((_) => HideLoadingIndicator());
                SceneGenerationManager.Instance.OnGenerationFailed.AddListener(OnGenerationError);
            }

            if (ShotCaptureManager.Instance != null)
            {
                ShotCaptureManager.Instance.OnShotCaptured.AddListener(OnShotCaptured);
                ShotCaptureManager.Instance.OnShotListUpdated.AddListener(UpdateShotList);
            }

            // Tool buttons
            actorButton?.onClick.AddListener(() => SetMode(VRInteractionManager.InteractionMode.PlaceActor));
            cameraButton?.onClick.AddListener(() => SetMode(VRInteractionManager.InteractionMode.PlaceCamera));
            markerButton?.onClick.AddListener(() => SetMode(VRInteractionManager.InteractionMode.PlaceMarker));
            captureButton?.onClick.AddListener(() => ShotCaptureManager.Instance?.CaptureShot());
            voiceButton?.onClick.AddListener(() => VoiceInputManager.Instance?.ToggleRecording());
        }

        public void SetInteractionMode(VRInteractionManager.InteractionMode mode)
        {
            if (modeText != null)
            {
                modeText.text = mode.ToString().Replace("Place", "Place ");
            }

            ResetButtonColors();
            
            switch (mode)
            {
                case VRInteractionManager.InteractionMode.PlaceActor:
                    HighlightButton(actorButton);
                    break;
                case VRInteractionManager.InteractionMode.PlaceCamera:
                    HighlightButton(cameraButton);
                    break;
                case VRInteractionManager.InteractionMode.PlaceMarker:
                    HighlightButton(markerButton);
                    break;
            }
        }

        private void SetMode(VRInteractionManager.InteractionMode mode)
        {
            VRInteractionManager.Instance?.SetInteractionMode(mode);
        }

        private void HighlightButton(Button button)
        {
            if (button == null) return;
            var colors = button.colors;
            colors.normalColor = _activeButtonColor;
            button.colors = colors;
        }

        private void ResetButtonColors()
        {
            var buttons = new[] { actorButton, cameraButton, markerButton };
            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                var colors = btn.colors;
                colors.normalColor = _defaultButtonColor;
                btn.colors = colors;
            }
        }

        public void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        public void ShowRecordingIndicator()
        {
            if (recordingIndicator != null)
            {
                recordingIndicator.SetActive(true);
            }
            SetStatus("Listening...");
        }

        public void HideRecordingIndicator()
        {
            if (recordingIndicator != null)
            {
                recordingIndicator.SetActive(false);
            }
            SetStatus("Processing...");
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(true);
            }
            SetStatus("Generating scene...");
        }

        public void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }
            SetStatus("Ready");
        }

        private void HideAllIndicators()
        {
            recordingIndicator?.SetActive(false);
            loadingIndicator?.SetActive(false);
            SetStatus("Ready");
        }

        private void OnTranscriptionReceived(string text)
        {
            if (promptText != null)
            {
                promptText.text = $"\"{text}\"";
            }
            SetStatus("Transcription received");
        }

        private void OnGenerationError(string error)
        {
            HideLoadingIndicator();
            SetStatus($"Error: {error}");
        }

        private void OnShotCaptured(ShotData shot)
        {
            SetStatus($"Captured Shot {shot.shotNumber}");
        }

        private void UpdateShotList(System.Collections.Generic.List<ShotData> shots)
        {
            if (shotListContent == null || shotItemPrefab == null) return;

            foreach (Transform child in shotListContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var shot in shots)
            {
                var item = Instantiate(shotItemPrefab, shotListContent);
                var text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"Shot {shot.shotNumber}: {shot.focalLength}mm";
                }

                var thumbnail = item.transform.Find("Thumbnail")?.GetComponent<RawImage>();
                if (thumbnail != null && !string.IsNullOrEmpty(shot.framePath))
                {
                    StartCoroutine(LoadThumbnail(shot.framePath, thumbnail));
                }
            }
        }

        private System.Collections.IEnumerator LoadThumbnail(string path, RawImage target)
        {
            if (!System.IO.File.Exists(path)) yield break;

            var data = System.IO.File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            target.texture = texture;
        }

        public void ToggleShotList()
        {
            if (shotListPanel != null)
            {
                shotListPanel.SetActive(!shotListPanel.activeSelf);
            }
        }
    }
}
