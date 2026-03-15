using UnityEngine;
using DirectorsEye.VR;
using DirectorsEye.UI;

namespace DirectorsEye.Core
{
    public class DirectorsEyeApp : MonoBehaviour
    {
        public static DirectorsEyeApp Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private string backendUrl = "http://localhost:8000";
        [SerializeField] private bool debugMode = true;

        [Header("Managers")]
        [SerializeField] private SceneGenerationManager sceneManager;
        [SerializeField] private VoiceInputManager voiceManager;
        [SerializeField] private VRInteractionManager vrManager;
        [SerializeField] private ShotCaptureManager shotManager;

        [Header("Prefabs")]
        [SerializeField] private GameObject actorPrefab;
        [SerializeField] private GameObject cameraPrefab;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeManagers();
        }

        void Start()
        {
            SetupEventFlow();
            
            if (debugMode)
            {
                Debug.Log("[DirectorsEye] Application initialized");
                Debug.Log($"[DirectorsEye] Backend: {backendUrl}");
            }
        }

        private void InitializeManagers()
        {
            if (sceneManager == null)
                sceneManager = FindObjectOfType<SceneGenerationManager>();
            
            if (voiceManager == null)
                voiceManager = FindObjectOfType<VoiceInputManager>();
            
            if (vrManager == null)
                vrManager = FindObjectOfType<VRInteractionManager>();
            
            if (shotManager == null)
                shotManager = FindObjectOfType<ShotCaptureManager>();
        }

        private void SetupEventFlow()
        {
            // Voice → Scene Generation pipeline
            if (voiceManager != null && sceneManager != null)
            {
                voiceManager.OnTranscriptionComplete.AddListener(OnVoiceCommand);
            }

            // Scene Generated → Ready for interaction
            if (sceneManager != null)
            {
                sceneManager.OnSceneGenerated.AddListener(OnSceneReady);
            }
        }

        private async void OnVoiceCommand(string transcription)
        {
            Debug.Log($"[DirectorsEye] Voice command: {transcription}");

            var command = ParseCommand(transcription);

            switch (command.type)
            {
                case CommandType.GenerateScene:
                    await sceneManager.GenerateSceneFromPrompt(command.content);
                    break;

                case CommandType.RefineScene:
                    await sceneManager.RefineScene(command.content);
                    break;

                case CommandType.PlaceActor:
                    vrManager?.SetInteractionMode(VRInteractionManager.InteractionMode.PlaceActor);
                    DirectorsEyeUI.Instance?.SetStatus("Place actor mode - point and trigger");
                    break;

                case CommandType.PlaceCamera:
                    vrManager?.SetInteractionMode(VRInteractionManager.InteractionMode.PlaceCamera);
                    DirectorsEyeUI.Instance?.SetStatus("Place camera mode - point and trigger");
                    break;

                case CommandType.CaptureShot:
                    shotManager?.CaptureShot(command.content);
                    break;

                case CommandType.Unknown:
                default:
                    // Treat as scene generation by default
                    await sceneManager.GenerateSceneFromPrompt(transcription);
                    break;
            }
        }

        private void OnSceneReady(GameObject scene)
        {
            Debug.Log("[DirectorsEye] Scene ready for interaction");
            DirectorsEyeUI.Instance?.SetStatus("Scene ready - place actors and cameras");
            
            // Reset interaction mode
            vrManager?.SetInteractionMode(VRInteractionManager.InteractionMode.Pointer);
        }

        private ParsedCommand ParseCommand(string input)
        {
            var lower = input.ToLower().Trim();

            // Refinement commands
            if (lower.StartsWith("make it") || lower.StartsWith("add") || 
                lower.StartsWith("change") || lower.StartsWith("more") ||
                lower.StartsWith("less") || lower.StartsWith("remove"))
            {
                return new ParsedCommand(CommandType.RefineScene, input);
            }

            // Actor placement
            if (lower.Contains("place actor") || lower.Contains("add actor") ||
                lower.Contains("put actor"))
            {
                return new ParsedCommand(CommandType.PlaceActor, input);
            }

            // Camera placement
            if (lower.Contains("place camera") || lower.Contains("add camera") ||
                lower.Contains("set up camera"))
            {
                return new ParsedCommand(CommandType.PlaceCamera, input);
            }

            // Shot capture
            if (lower.Contains("capture") || lower.Contains("take shot") ||
                lower.Contains("save frame"))
            {
                var description = input.Replace("capture", "").Replace("take shot", "").Trim();
                return new ParsedCommand(CommandType.CaptureShot, description);
            }

            // Default: treat as scene description
            return new ParsedCommand(CommandType.GenerateScene, input);
        }

        public void GenerateDemo()
        {
            // Demo scene for testing
            _ = sceneManager?.GenerateSceneFromPrompt(
                "A moody jazz club interior at night, dim lighting, empty stage with spotlight, tables and chairs scattered around"
            );
        }
    }

    public enum CommandType
    {
        Unknown,
        GenerateScene,
        RefineScene,
        PlaceActor,
        PlaceCamera,
        CaptureShot
    }

    public struct ParsedCommand
    {
        public CommandType type;
        public string content;

        public ParsedCommand(CommandType type, string content)
        {
            this.type = type;
            this.content = content;
        }
    }
}
